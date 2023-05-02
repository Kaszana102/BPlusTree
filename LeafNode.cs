using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace StukturyBazDanych2 
{    
    internal class LeafNode:Node
    {
        List<RecordKey>? records;
        
        int degree;
        ///                                       records                           max 
        ///                                  type number   family              records number
        public static int LeafNodeBinarySize = 1 +  4 +     3*4    +         2 * BTreePlus.getDegree() * RecordKey.RecordKeysBinarySize;

        public LeafNode(int blockPos)
        {            
            this.degree = BTreePlus.getDegree();
            records = new List<RecordKey>();
            this.blockPos = blockPos;            
        }


        public LeafNode(List<RecordKey> records, int blockPos)
        {
            this.degree = BTreePlus.getDegree();
            this.records = records;
            this.blockPos = blockPos;
        }


        public List<RecordKey> GetRecordsList()
        {
            return records;
        }

        override public Record? GetRecord(int key)
        {
            foreach(var record in records)
            {
                if(record.key == key)
                {
                    return record.record;
                }
            }
            return null;
        }


        public override Record? GetNthRecord(int n)
        {
            foreach(RecordKey key in records)
            {
                if (n == 0)
                {
                    return key.record;
                }
                n--;
            }
            if (parent!= -1)
            {
                return BTreePlus.Deserialize(parent).GetNextNode(records.Last().key, blockPos);
            }
            else
            {
                return null;
            }
        }

        public override void UpdateRecord(RecordKey newRecord)
        {
            foreach (var record in records)
            {
                if (record.key == newRecord.key)
                {
                    record.record = newRecord.record;
                    BTreePlus.Serialize(this);
                    return;
                }
            }
        }

        public override void InsertRecord(RecordKey record)
        {            
            if (records.Count() > 0)
            {
                int index = 0;
                foreach (var nodeRecord in records)
                {
                    if (nodeRecord.key > record.key)
                    {
                        records.Insert(index, record);
                        break;
                    }
                    index++;
                }

                if(index == records.Count())
                {
                    records.Add(record);
                }
                

                if(records.Count() == degree * 2 + 1)
                {
                    //check siblings!
                    Compensate();
                }
                else
                {
                    BTreePlus.Serialize(this);
                }
            }
            else
            {
                records.Add(record);
                BTreePlus.Serialize(this);
            }
            
        }


        public void CompensateDelete()
        {

            if (leftSibling != -1)
            {
                LeafNode leftSiblingNode = (LeafNode) BTreePlus.Deserialize(leftSibling);
                if (leftSiblingNode.GetKeysNumber() > degree)
                {
                    //move biggest record here
                    RecordKey recordKey = leftSiblingNode.GetLastRecodKey();

                    leftSiblingNode.DeleteRecord(recordKey.key);
                    InsertRecord(recordKey);

                    BTreePlus.Serialize(this);
                    BTreePlus.Serialize(leftSiblingNode);

                    ((NoneLeafNode) BTreePlus.Deserialize(leftSiblingNode.parent)).UpdateKeysRecurently();
                }
                else
                {
                    if (rightSibling != -1)
                    {
                        LeafNode rightSiblingNode = (LeafNode)BTreePlus.Deserialize(rightSibling);
                        if (rightSiblingNode.GetKeysNumber() > degree)
                        {                            
                            //move smallest record here
                            RecordKey recordKey = rightSiblingNode.GetFirstRecodKey();

                            rightSiblingNode.DeleteRecord(recordKey.key);
                            InsertRecord(recordKey);


                            BTreePlus.Serialize(this);
                            BTreePlus.Serialize(rightSiblingNode);

                            ((NoneLeafNode)BTreePlus.Deserialize(parent)).UpdateKeysRecurently();
                        }
                        else
                        {
                            if (parent != -1)
                            {
                                ((NoneLeafNode)BTreePlus.Deserialize(parent)).Merge(this, true);
                            }
                        }
                    }
                    else
                    {
                        if (parent != -1)
                        {
                            ((NoneLeafNode)BTreePlus.Deserialize(parent)).Merge(this, true);
                        }
                    }
                }
            }
            else
            {
                if (rightSibling != -1)
                {
                    LeafNode rightSiblingNode = (LeafNode)BTreePlus.Deserialize(rightSibling);
                    if (rightSiblingNode.GetKeysNumber() > degree)
                    {
                        //move smallest record here
                        RecordKey recordKey = rightSiblingNode.GetFirstRecodKey();

                        rightSiblingNode.DeleteRecord(recordKey.key);
                        InsertRecord(recordKey);


                        BTreePlus.Serialize(this);
                        BTreePlus.Serialize(rightSiblingNode);

                        ((NoneLeafNode)BTreePlus.Deserialize(parent)).UpdateKeysRecurently();
                    }
                    else
                    {
                        if (parent != -1)
                        {
                            ((NoneLeafNode)BTreePlus.Deserialize(parent)).Merge(this, true);
                        }                        
                    }
                }
                else
                {
                    if (parent != -1)
                    {
                        ((NoneLeafNode)BTreePlus.Deserialize(parent)).Merge(this, true);
                    }
                    else
                    {
                        //it's root node

                        if (records.Count() == 0)
                        {
                            //whole tree is empty
                            BTreePlus.SetRoot(-1);
                            BTreePlus.FreeBlock(this.GetBlockPos());
                        }
                        else
                        {
                            BTreePlus.Serialize(this);
                        }
                    }
                }
            }
        }


        public override void DeleteRecord(int key)
        {            

            foreach(var record in records)
            {
                if (record.key == key)
                {
                    records.Remove(record);

                    //check number of records!                    
                    if(records.Count() < degree)
                    {
                        CompensateDelete();
                    }
                    else
                    {
                        BTreePlus.Serialize(this);
                    }                    
                    break;
                }
            }            
        }              


        /// <summary>
        /// if it can't compensate it calls the split function
        /// </summary>
        public override void Compensate()
        {
            if (leftSibling != -1)
            {
                LeafNode leftSiblingNode = (LeafNode)BTreePlus.Deserialize(leftSibling);
                if (leftSiblingNode.GetKeysNumber() < 2 * degree)
                {
                    //move smallest record there
                    RecordKey recordKey = records.First();
                    records.RemoveAt(0);
                    leftSiblingNode.InsertRecord(recordKey);

                    BTreePlus.Serialize(this);
                    BTreePlus.Serialize(leftSiblingNode);

                    ((NoneLeafNode)BTreePlus.Deserialize(leftSiblingNode.parent)).UpdateKeysRecurently();                    
                }
                else
                {
                    if(rightSibling != -1)
                    {
                        LeafNode rightSiblingNode = (LeafNode)BTreePlus.Deserialize(leftSibling);
                        if (rightSiblingNode.GetKeysNumber() < 2 * degree)
                        {
                            //move biggest record there
                            RecordKey recordKey = records.Last();
                            records.RemoveAt(records.Count() - 1);
                            rightSiblingNode.InsertRecord(recordKey);

                            BTreePlus.Serialize(this);
                            BTreePlus.Serialize(rightSiblingNode);
                            ((NoneLeafNode)BTreePlus.Deserialize(parent)).UpdateKeysRecurently();

                        }
                        else
                        {
                            Split();
                        }
                    }
                    else
                    {
                        Split();
                    }
                }
            }
            else
            {
                if (rightSibling != -1)
                {
                    LeafNode rightSiblingNode = (LeafNode)BTreePlus.Deserialize(rightSibling);
                    if (rightSiblingNode.GetKeysNumber() < 2 * degree)
                    {
                        //move biggest record there
                        RecordKey recordKey = records.Last();
                        records.RemoveAt(records.Count()-1);
                        rightSiblingNode.InsertRecord(recordKey);

                        BTreePlus.Serialize(this);
                        BTreePlus.Serialize(rightSiblingNode);

                        ((NoneLeafNode)BTreePlus.Deserialize(parent)).UpdateKeysRecurently();                        
                    }
                    else
                    {
                        Split();
                    }
                }
                else
                {
                    Split();
                }
            }            
        }

        public RecordKey GetLastRecodKey()
        {
            return records.Last();
        }

        public RecordKey GetFirstRecodKey()
        {
            return records.First();
        }

        public override int GetLastKey()
        {
            return records.Last().key;
        }


        public override int GetNextKey()
        {
            return records.Last().key;
        }
        public override int GetPrevKey()
        {
            return records.First().key;
        }

        public override int GetKeysNumber()
        {
            return records.Count();
        }

        public override string ToString(string depth)
        {
            string otp="";            

            foreach (var record in records)
            {
                otp += depth + record +" id:"+ BTreePlus.recordIndex++ + "\n" ;
            }

            return otp;
        }   

        public override void Split()
        {
            LeafNode rightNode;

            rightNode = new LeafNode(BTreePlus.ReserveNewBlock());            

            //move records
            for (int i = 0; i <= degree; i++) {
                rightNode.InsertRecord(records.Last());
                records.RemoveAt(records.Count()-1);//delete last
            }

            //get parent
            NoneLeafNode parentNode;
            if (parent == -1)
            {
                parentNode = new NoneLeafNode(BTreePlus.ReserveNewBlock());
                parent = parentNode.GetBlockPos();
                

                BTreePlus.SetRoot(parent);
            }
            else
            {
                parentNode = BTreePlus.Deserialize(parent) as NoneLeafNode;
            }


            //set the same parent, it may change in AddKey down there
            rightNode.parent = parent;


            //set siblings (doesn't change parents)
            SetSiblingsForSplit(rightNode);//Serialize all modified nodes
            


            parentNode.AddKey(blockPos, records.Last().key, rightNode.GetBlockPos());

            //this.parent = BTreePlus.Deserialize(blockPos).parent; //może być zmieniony parent przy
                                                                  //wywołaniu funkcji addKey, a nie zmienia to lokalnego tutaj węzła,
                                                                  //a konkretnie pola parent!

            //rightNode = BTreePlus.Deserialize(rightNode.blockPos) as LeafNode;
            //rightNode.parent = parent;
            //BTreePlus.Serialize(rightNode);                                              
            
            

        }


        



        public override byte[] NodeToBinary()
        {            
            List<byte> otp = new List<byte>();
            //first byte is whether leafNode or not
            otp.Add(0);


            //insert record counter
            foreach(byte Byte in IntConvert.intToByte4(records.Count()))
            {
                otp.Add(Byte);
            }


            //insert family
            //parent
            foreach (byte Byte in IntConvert.intToByte4(parent))
            {
                otp.Add(Byte);
            }            
            //left sibling
            foreach (byte Byte in IntConvert.intToByte4(leftSibling))
            {
                otp.Add(Byte);
            }            
            //right sibling
            foreach (byte Byte in IntConvert.intToByte4(rightSibling))
            {
                otp.Add(Byte);
            }            


            //simply serialize records here
            for (int i=0; i< records.Count(); i++)
            {
                otp.AddRange(records[i].ToBinary());
            }

            return otp.ToArray();
        }
        


        //it doesn't receive nodetype byte!!!!
        static public LeafNode NodeFromBinary(byte[] src, int blockIndex)
        {
            //get record counter
            int count = IntConvert.byte4Toint(src); //size of 4 bytes!!
            src = src.Skip(4).ToArray();

            //get family
            //parent
            int parent = IntConvert.byte4Toint(src);
            src = src.Skip(4).ToArray();
            //left sibling
            int leftSibling = IntConvert.byte4Toint(src);
            src = src.Skip(4).ToArray();
            //right sibling
            int rightSibling = IntConvert.byte4Toint(src);
            src = src.Skip(4).ToArray();




            List<RecordKey> records = new List<RecordKey>();


            //simply unserialize records here
            for (int i = 0; i < count; i++)
            {                
                records.Add(RecordKey.FromBinary(src));
                src = src.Skip(RecordKey.RecordKeysBinarySize).ToArray();
            }

            LeafNode otp = new LeafNode(records, blockIndex);
            otp.parent = parent;
            otp.leftSibling = leftSibling;
            otp.rightSibling = rightSibling;

            return otp;
        }

    }   
}
