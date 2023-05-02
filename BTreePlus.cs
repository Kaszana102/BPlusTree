using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace StukturyBazDanych2
{

    internal class BTreePlus
    {
        static int BLOCKSIZE =   //choose block size procedurally
            NoneLeafNode.NoneLeafNodeBinarySize > LeafNode.LeafNodeBinarySize ?
            NoneLeafNode.NoneLeafNodeBinarySize :
            LeafNode.LeafNodeBinarySize;


        static bool createdFile = false;

        static int rootBlockIndex = -1;        
        static float fillFactor = 2;
        public static int recordIndex;//needed while printing

        public static bool showNumberOfDiskOperations = false;
        static int numberOfReads;
        static int numberOfWrites;
        static public int totalNumberOfReads = 0;
        static public int totalNumberOfWrites = 0;

        static public int getDegree()
        {
            return 2;
        }



        static void zeroNumberOfReads()
        {
            numberOfReads = 0;
            numberOfWrites = 0;
        }

        static void PrintOperationsNumber()
        {
            if (showNumberOfDiskOperations)
            {
                Console.WriteLine("reads: " + numberOfReads);
                Console.WriteLine("writes: " + numberOfWrites);
                Console.WriteLine("total: " + (numberOfReads + numberOfWrites));
            }

            totalNumberOfReads += numberOfReads;
            totalNumberOfWrites += numberOfWrites;
        }        
        static public void InsertRecord(RecordKey record)
        {
            zeroNumberOfReads();
            if (rootBlockIndex == -1)
            {

                rootBlockIndex = ReserveNewBlock();
                Node root = new LeafNode(rootBlockIndex);
                Serialize(root, rootBlockIndex);
            }
            Deserialize(rootBlockIndex).InsertRecord(record);
            PrintOperationsNumber();
        }
        static public Record? ReadRecord(int key)
        {
            zeroNumberOfReads();
            if (rootBlockIndex == -1)
            {
                return null;
            }
            else
            {
                return Deserialize(rootBlockIndex).GetRecord(key);
            }
            PrintOperationsNumber();
        }

        static public Record? GetNthRecord(int n)
        {
            zeroNumberOfReads();
            if (rootBlockIndex == -1)
            {
                return null;
            }
            else
            {
                return Deserialize(rootBlockIndex).GetNthRecord(n);
            }
            PrintOperationsNumber();
        }

        static public void UpdateRecord(RecordKey record)
        {
            zeroNumberOfReads();
            if (rootBlockIndex != -1)
            {
                Deserialize(rootBlockIndex).UpdateRecord(record);
            }
            PrintOperationsNumber();
        }

        static public void DeleteRecord(int key)
        {
            zeroNumberOfReads();
            if (rootBlockIndex != -1)
            {
                Deserialize(rootBlockIndex).DeleteRecord(key);
            }
            PrintOperationsNumber();
        }

        static public void Reorganize()
        {
            zeroNumberOfReads();

            PrintOperationsNumber();
        } //TODO co to ma właście robić? Czy jest to budowanie drzewa od nowa?        

        static public void SetRoot(int newRootBlockIndex)
        {
            rootBlockIndex = newRootBlockIndex;
        }


        static public string ToString()
        {
            //zeroNumberOfReads();
            recordIndex = 0;
            if (rootBlockIndex != -1)
            {
                return Deserialize(rootBlockIndex).ToString("");
            }
            else
            {
                return "empty";
            }
            //PrintOperationsNumber();
        }

        static public void Serialize(Node node)
        {
            Serialize(node, node.GetBlockPos());
        }


        static public void Serialize(Node node, int blockPos)
        {
            numberOfWrites++;
            Stream stream = File.Open("BTreePlus.txt", FileMode.OpenOrCreate);

            stream.Position = blockPos * BLOCKSIZE;

            byte[] serializedNode = node.NodeToBinary();

            stream.Write(serializedNode);

            stream.Close();
        }


        static public Node Deserialize(int blockPos)
        {
            numberOfReads++;
            if (blockPos >= 0)
            {
                //Open file.
                Stream streamRead = File.Open("BTreePlus.txt", FileMode.OpenOrCreate);
                //Deserailze file back to object.

                streamRead.Position = blockPos * BLOCKSIZE;

                Node NodeDeserialized;
                if (streamRead.ReadByte() == 0)
                {
                    byte[] buffer = new byte[LeafNode.LeafNodeBinarySize];
                    streamRead.Read(buffer);

                    NodeDeserialized = LeafNode.NodeFromBinary(buffer, blockPos);
                }
                else
                {

                    byte[] buffer = new byte[NoneLeafNode.NoneLeafNodeBinarySize];
                    streamRead.Read(buffer);
                    NodeDeserialized = NoneLeafNode.NodeFromBinary(buffer, blockPos);
                }

                streamRead.Close();

                return NodeDeserialized;
            }
            else
            {
                return null;
            }
        }



        //1-zajęte
        //0-wolne


        // 8 7 6 5 4 3 2 1       15 14 13 12 11 10 9  ...
        static public int ReserveNewBlock()
        {

            if (!createdFile)
            {
                Stream newFile = File.Open("BlockBitmap.txt", FileMode.Create);
                newFile.Close();
                createdFile = true;

            }

            //utwórz bitmapę zajętości sektorów
            //znajdź pierwsze wolne miejsce (0)
            //zamień na 1 zwróc jego indeks. done

            int newBlockIndex = 0;


            using (FileStream streamRead = new FileStream("BlockBitmap.txt", FileMode.OpenOrCreate))
            {
                //Deserailze file back to object.            

                byte Byte;
                int ByteInt;
                ByteInt = streamRead.ReadByte();

                while (ByteInt != -1)
                {
                    Byte = Convert.ToByte(ByteInt);

                    //if there is one zero
                    if ((Byte ^ 0xFF) != 0xFF)
                    {
                        for (int i = 0; i < 8; i++)
                        {
                            //if i'th right digit is zero
                            if ((Byte & (1 << i)) == 0)
                            {
                                //we found free blocK!                                
                                Byte = Convert.ToByte(Byte | (1 << i)); //set flag
                                streamRead.Position -= 1;
                                streamRead.WriteByte(Byte);
                                streamRead.Close();
                                return newBlockIndex + i;
                            }
                        }
                    }

                    ByteInt = streamRead.ReadByte();
                    newBlockIndex += 8;
                }


                //we need to write new byte, for new block!

                streamRead.WriteByte(Convert.ToByte(1));                



                streamRead.Close();
                return newBlockIndex;
            }
        }


        static public void FreeBlock(int blockIndex)
        {
            using (FileStream streamRead = new FileStream("BlockBitmap.txt", FileMode.Open))
            {
                //Deserailze file back to object.                            
                byte Byte;
                int ByteInt;
                ByteInt = streamRead.ReadByte();

                streamRead.Position = blockIndex / 8;

                ByteInt = streamRead.ReadByte();

                if (ByteInt != -1)
                {
                    Byte = Convert.ToByte(ByteInt | (1 << blockIndex % 8));

                    streamRead.Position -= 1;
                    streamRead.WriteByte(Byte);

                }

                streamRead.Close();
            }
        }


        static public int MemorySize()
        {
            int size = 0;
            byte Byte;
            int ByteInt;
            using (FileStream streamRead = new FileStream("BlockBitmap.txt", FileMode.Open))
            {
                ByteInt = streamRead.ReadByte();
                while (ByteInt != -1)
                {
                    Byte = Convert.ToByte(ByteInt);

                    for (int i = 0; i < 8; i++)
                    {
                        //if i'th right digit is 1
                        if ((Byte & (1 << i)) == 1 << i)
                        {
                            size++;
                        }
                    }

                    ByteInt = streamRead.ReadByte();
                }

            }
            return size * BLOCKSIZE;
        }
    }
    
}




///b tree schema
///
///       
/// 
/// 
///
