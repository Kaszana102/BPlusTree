using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace StukturyBazDanych2
{
    [Serializable()]
    internal class NoneLeafNode : Node
    {
        public List<int>? keys { get; set; } //holds prevs key

        //holds childNodes blockIndexes
        public  List<int>? childNodes { get; set; }
        int degree;
        //                                             keys     keys space                                            childNodes space
        //                                       type  number   int  degree                       family                    keysNumber+1
        public static int NoneLeafNodeBinarySize = 1 +   4  +   2 * 4 * BTreePlus.getDegree() +    3*4     +       (2 * BTreePlus.getDegree() + 1) * 4;
        public NoneLeafNode(int blockPos)
        {
            this.degree = BTreePlus.getDegree();
            keys = new List<int>();
            childNodes = new List<int>();
            this.blockPos = blockPos;
        } 
        public NoneLeafNode(List<int> keys, List<int> childNodes, int blockPos)
        {
            this.degree = BTreePlus.getDegree();
            this.keys = keys;
            this.childNodes = childNodes;
            this.blockPos = blockPos;
        }

        public override int GetKeysNumber()
        {
            return keys.Count();    
        }


        override public Record? GetRecord(int key)
        {

            //0 0 1 1 2
            //p k p k p

            //sprawdzanie czy szukamy pomiedzy
            for (int i = 0; i < keys.Count; i++)
            {
                if (key <= keys[i])
                {
                    return BTreePlus.Deserialize(childNodes[i]).GetRecord(key);
                }
            }
            //bierzemy ostatni wezel
            return BTreePlus.Deserialize(childNodes.Last()).GetRecord(key);
        }

        public override Record? GetNthRecord(int n)
        {
            return BTreePlus.Deserialize(childNodes.First()).GetNthRecord(n);
        }


        public override Record? GetNextNode(int prevKey, int srcBlockIndex)
        {
            if (srcBlockIndex != parent)
            {
                //going upwards
                int index = 0;
                foreach (var key in keys)
                {
                    if (key > prevKey)
                    {
                        return BTreePlus.Deserialize(childNodes[index]).GetNextNode(prevKey, blockPos);//found in next child
                    }
                    index++;
                }

                //next node migth be the last
                if (srcBlockIndex != childNodes.Last())
                {
                    return BTreePlus.Deserialize(childNodes.Last()).GetNextNode(prevKey, blockPos);//found in next child
                }
                else
                {
                    //it was, so we go upwards
                    if (parent != -1)
                    {
                        return BTreePlus.Deserialize(parent).GetNextNode(prevKey, blockPos);
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            else
            {
                //going downwards                
                //and we choose the first child!
                return BTreePlus.Deserialize(childNodes.First()).GetNextNode(prevKey, blockPos);

            }
        }
        public Record? GetPreviousNode() { return null; }

        public override void InsertRecord(RecordKey record)
        {
            int index = 0;
            foreach (var key in keys)
            {
                if (record.key < key)
                {
                    break;
                }
                index++;
            }
            BTreePlus.Deserialize(childNodes[index]).InsertRecord(record);
        }

        public override void UpdateRecord(RecordKey record)
        {
            int index = 0;
            foreach (var key in keys)
            {
                if (record.key < key)
                {
                    break;
                }
                index++;
            }
            BTreePlus.Deserialize(childNodes[index]).UpdateRecord(record);
        }


        public override void DeleteRecord(int key)
        {
            for (int i = 0; i < keys.Count; i++)
            {
                if (key <= keys[i])
                {
                    BTreePlus.Deserialize(childNodes[i]).DeleteRecord(key);
                    return;
                }
            }
            //bierzemy ostatni wezel
            BTreePlus.Deserialize(childNodes.Last()).DeleteRecord(key);
        }

        ///       5        10    \/    \/    16
        ///                   11 12 13 14 15
        ///                   
        /// 
        /// 5  10   \/\/   13      16
        ///       11 12 13    14 15
        /// 
        ///                   


        ///  litery-wskaźniki
        ///
        ///           a 1 b 2 c 3 d 4 e 5
        ///      
        ///            f 3 g
        ///   a 1 b 2 c      d 4 e 5   
        /// 
        /// 
        ///         
        


        //insert new childnodeaccording to the newKey. if this node is empty it also adds left
        public override void AddKey(int left, int newKey, int right)
        {
            if (keys.Count() > 0)
            {
                int index = 0;
                foreach (var key in keys)
                {
                    if (key > newKey)
                    {
                        break;
                    }
                    index++;
                }


                keys.Insert(index, newKey);
                childNodes.Insert(index + 1, right);
                

                if (keys.Count() == 2 * degree + 1)
                {
                    Split();
                }
                else
                {
                    BTreePlus.Serialize(this);
                }
            }
            else
            {
                childNodes.Add(left);
                keys.Add(newKey);
                childNodes.Add(right);
                BTreePlus.Serialize(this);
            }            
        }

        ///
        ///  . 1 . 3 . 5 . 7 . 
        ///  
        /// 
        ///  . 1 . 3 . 5 , 7 , 9 
        /// 
        /// 
        ///             . 5 .
        ///  .1 . 3 . 5 .   , 7 , 9 ,
        ///              

        

        public override void Split()
        {
            NoneLeafNode right;

            right = new NoneLeafNode(BTreePlus.ReserveNewBlock());

            //copying to right node

            //this node will be left

            //move degree keys to the right
            //moving from the center
            for (int i = 0; i < degree; i++)
            {
                right.AddKey(childNodes[degree+1+i], keys[degree+1 + i], childNodes[degree+2 + i]);                
            }

            //delete moved data
            for (int i = 0; i <= degree; i++)
            {
                keys.RemoveAt(degree);
                childNodes.RemoveAt(degree + 1);
            }

            
            //update their parent
            foreach (var childBlock in right.childNodes)
            {
                Node child = BTreePlus.Deserialize(childBlock);
                child.parent = right.GetBlockPos();
                BTreePlus.Serialize(child);
            }

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

            SetSiblingsForSplit(right); //serializes all modified nodes


            right.parent = parent;
            right.UpdateKeys(); //serializes node
            this.UpdateKeys();  //serializes node

            parentNode.AddKey(blockPos, this.GetNextKey(), right.blockPos);

            //this.parent = BTreePlus.Deserialize(blockPos).parent; //może być zmieniony parent przy
                                                                  //wywołaniu funkcji addKey, a nie zmienia to lokalnego tutaj węzła,
                                                                  //a konkretnie pola parent!            
                        

            //parentNode.UpdateKeys(); //serializes node            

        }


        /// <summary>
        /// updates siblings of these child nodes:
        /// 
        ///   . n . key . m .
        ///   /\  /\    /\  /\
        ///   |    |    |    |
        ///  id-1  id  id+1  id+2
        ///  
        /// </summary>
        /// <param name="key"></param>
        public void UpdateChildSiblings(int keyCenter)
        {
            int index = 0;
            foreach (var key in keys)
            {
                if (keyCenter == key)
                {
                    break;
                }
                index++;
            }

            //going from most left
            if (index - 1 >= 0) //if most left exist
            {
                Node child = BTreePlus.Deserialize(childNodes[index - 1]);
                //updates its family
                if (index - 2 >= 0)
                {
                    child.leftSibling = childNodes[index - 2];
                }
                else
                {
                    child.leftSibling = -1;
                }
                child.parent = blockPos;
                child.rightSibling = childNodes[index];

                BTreePlus.Serialize(child);
            }

            ////updates left family
            {
                Node child = BTreePlus.Deserialize(childNodes[index]);
                if (index - 1 >= 0) //if mostleft exist
                {
                    child.leftSibling = childNodes[index - 1];
                }
                else
                {
                    child.leftSibling = -1;
                }
                child.parent = blockPos;             

                if (index + 1 < childNodes.Count()) //to moze byc zbyteczne?
                {
                    child.rightSibling = childNodes[index + 1];
                }
                else
                {
                    child.rightSibling = -1;
                }
                BTreePlus.Serialize(child);
            }



            //right
            if (index + 1 < childNodes.Count()) //if right exist
            {
                Node child = BTreePlus.Deserialize(childNodes[index+1]);

                child.leftSibling = childNodes[index];

                child.parent = blockPos;

                if (index + 2 < childNodes.Count())
                {
                    child.rightSibling = childNodes[index + 2];
                }
                else
                {
                    child.rightSibling = -1;
                }
                BTreePlus.Serialize(child);
            }

            //most right
            if (index + 2 < childNodes.Count()) //if most right exist
            {
                Node child = BTreePlus.Deserialize(childNodes[index + 2]);

                child.leftSibling = childNodes[index + 1];

                child.parent = blockPos;
                if (index + 3 < childNodes.Count()) //if it has right sibling
                {
                    child.rightSibling = childNodes[index + 3];
                }
                else
                {
                    child.rightSibling = -1;
                }

                BTreePlus.Serialize(child);
            }

            UpdateKeys();
        }




        public void MergeLeaves(int leftNodeIndex, LeafNode leftNode, LeafNode rightNode)
        {

            List<RecordKey> rightRecords = rightNode.GetRecordsList();

            //put all record to leftNode
            foreach (RecordKey record in rightRecords)
            {
                leftNode.InsertRecord(record);
            }

            //delete right node
            BTreePlus.FreeBlock(rightNode.GetBlockPos());



            //update this node            
            childNodes.Remove(rightNode.GetBlockPos());
            keys.RemoveAt(leftNodeIndex);

            UpdateKeys();//serializes this node
            BTreePlus.Serialize(leftNode);

            SetSiblingsForMerge(leftNode.GetBlockPos(),rightNode.rightSibling);            
            //if not root
            if (parent != -1)
            {
                if (keys.Count() < degree)
                {
                    BTreePlus.Deserialize(parent).Merge(this, false);
                }
            }
            else
            {
                //it's root                
                if(keys.Count() == 0)
                {
                    //this node is empty
                    //so change root and free memory block

                    BTreePlus.FreeBlock(this.GetBlockPos());
                    BTreePlus.SetRoot(childNodes.First());

                    Node newRoot = BTreePlus.Deserialize(childNodes.First());
                    newRoot.parent = -1;
                    BTreePlus.Serialize(newRoot);
                }
                
            }



        }

        public void MergeNonLeaves(int leftNodeIndex, NoneLeafNode leftNode, NoneLeafNode rightNode)
        {
            //put everything to leftNode            

            //put all record to leftNode
            foreach (int node in rightNode.childNodes)
            {
                leftNode.childNodes.Add(node);

                Node nodeDeserialized = BTreePlus.Deserialize(node);
                nodeDeserialized.parent = leftNode.blockPos;
                BTreePlus.Serialize(nodeDeserialized);
                
            }

            //add new key from last leftNodes child
            leftNode.keys.Add(BTreePlus.Deserialize(leftNode.childNodes[degree - 1]).GetNextKey());

            //add all rightnode Keys
            foreach (int key in rightNode.keys)
            {
                leftNode.keys.Add(key);
            }


            BTreePlus.Serialize(leftNode);
            
            //delete right node
            BTreePlus.FreeBlock(rightNode.GetBlockPos());

            //update this node            
            childNodes.Remove(rightNode.GetBlockPos());
            keys.RemoveAt(leftNodeIndex);


            UpdateKeys();//serializes this node
            BTreePlus.Serialize(leftNode);


            if (parent != -1)
            {
                SetSiblingsForMerge(leftNode.GetBlockPos(),rightNode.rightSibling);
                if (keys.Count() < degree)
                {
                    BTreePlus.Deserialize(parent).Merge(this, false);
                }
            }
            else
            {

                //it's root
                //so set new root
                if (keys.Count() == 0)
                {
                    //whole tree is empty                    
                    BTreePlus.FreeBlock(this.GetBlockPos());
                    BTreePlus.SetRoot(childNodes.First());

                    Node newRoot = BTreePlus.Deserialize(childNodes[0]);
                    newRoot.parent = -1;
                    BTreePlus.Serialize(newRoot);
                }
                else
                {
                    BTreePlus.Serialize(this);
                }

                

                //((NoneLeafNode)BTreePlus.Deserialize(childNodes.First())).UpdateChildSiblings(leftNodeIndex);

            }
        }



        //return true if can compensate
        public bool CompensateDelete(int callingChildIndex)
        {
            NoneLeafNode nodeInHelp = (NoneLeafNode)BTreePlus.Deserialize(childNodes[callingChildIndex]);



            if (callingChildIndex > 0)
            {
                NoneLeafNode leftSiblingNode = (NoneLeafNode)BTreePlus.Deserialize(childNodes[callingChildIndex-1]);
                if (leftSiblingNode.GetKeysNumber() > degree)
                {
                    //compensate with him
                    //move biggest record here                    

                    nodeInHelp.childNodes.Insert(0,leftSiblingNode.childNodes.Last());
                    nodeInHelp.keys.Insert(0, leftSiblingNode.keys.Last());

                    leftSiblingNode.childNodes.RemoveAt(leftSiblingNode.childNodes.Count() - 1);
                    leftSiblingNode.keys.RemoveAt(leftSiblingNode.keys.Count() - 1);


                    //update childs parent
                    Node child = BTreePlus.Deserialize(nodeInHelp.childNodes[0]);
                    child.parent = nodeInHelp.GetBlockPos();
                    BTreePlus.Serialize(child);


                    BTreePlus.Serialize(nodeInHelp);
                    BTreePlus.Serialize(leftSiblingNode);

                    leftSiblingNode.UpdateKeysRecurently();
                    return true;
                }
                else
                {
                    if (callingChildIndex + 1 <= childNodes.Count() -1 )
                    {
                        NoneLeafNode rightSiblingNode = (NoneLeafNode)BTreePlus.Deserialize(childNodes[callingChildIndex + 1]);
                        if (rightSiblingNode.GetKeysNumber() > degree)
                        {
                            nodeInHelp.childNodes.Add(rightSiblingNode.childNodes[0]);
                            nodeInHelp.keys.Add(rightSiblingNode.keys[0]);

                            rightSiblingNode.childNodes.RemoveAt(0);
                            rightSiblingNode.keys.RemoveAt(0);


                            //update childs parent
                            Node child = BTreePlus.Deserialize(nodeInHelp.childNodes.Last());
                            child.parent = nodeInHelp.GetBlockPos();
                            BTreePlus.Serialize(child);


                            BTreePlus.Serialize(nodeInHelp);
                            BTreePlus.Serialize(rightSiblingNode);

                            this.UpdateKeysRecurently();
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            else
            {
                if (callingChildIndex + 1 <= childNodes.Count() - 1)
                {
                    NoneLeafNode rightSiblingNode = (NoneLeafNode)BTreePlus.Deserialize(childNodes[callingChildIndex + 1]);
                    if (rightSiblingNode.GetKeysNumber() > degree)
                    {
                        nodeInHelp.childNodes.Add(rightSiblingNode.childNodes[0]);
                        nodeInHelp.keys.Add(rightSiblingNode.keys[0]);

                        rightSiblingNode.childNodes.RemoveAt(0);
                        rightSiblingNode.keys.RemoveAt(0);


                        //update childs parent
                        Node child = BTreePlus.Deserialize(nodeInHelp.childNodes.Last());
                        child.parent = nodeInHelp.GetBlockPos();
                        BTreePlus.Serialize(child);


                        BTreePlus.Serialize(nodeInHelp);
                        BTreePlus.Serialize(rightSiblingNode);

                        this.UpdateKeysRecurently();
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
        }

        public override void Compensate() {
        
        
        }



        ///          .  |  .  |  .  |  .
        ///          |     |     |     |
        ///        |---| |---| |---| |---|  
        ///                      cD
        /// 
        public override void Merge(Node callingChild, bool isLeaf)
        {
            int callingChildIndex = 0;

            callingChildIndex = childNodes.IndexOf(callingChild.GetBlockPos());

            Node leftNode = null, rightNode = null;

            if (callingChildIndex + 1 < childNodes.Count)//if right sibling exist
            {
                rightNode = BTreePlus.Deserialize(childNodes[callingChildIndex + 1]);
                leftNode = callingChild;
            }
            else
            {
                if (callingChildIndex - 1 >= 0)
                {
                    rightNode = callingChild;
                    leftNode = BTreePlus.Deserialize(childNodes[callingChildIndex - 1]);
                }
            }



            if (isLeaf)
            {                

                if (leftNode != null && rightNode != null)
                {
                    MergeLeaves(childNodes.IndexOf(leftNode.GetBlockPos()), (LeafNode)leftNode, (LeafNode)rightNode);
                }
            }
            else
            {
                //if can't compensate, merge noneleafNodes
                if (!CompensateDelete(callingChildIndex))
                {
                    if (leftNode != null && rightNode != null)
                    {
                        MergeNonLeaves(childNodes.IndexOf(leftNode.GetBlockPos()), (NoneLeafNode)leftNode, (NoneLeafNode)rightNode);
                    }
                }
            }
        }


        //sufficient in splitting
        public void UpdateKeys()
        {            
            int newKey;
            for (int i = 0; i < keys.Count(); i++)
            {
                newKey= BTreePlus.Deserialize(childNodes[i]).GetNextKey();                
                keys[i] = newKey;
            }
            BTreePlus.Serialize(this);            
        }


        //needed due to compensation
        public void UpdateKeysRecurently()
        {
            UpdateKeys();
            if (parent != -1) {
                ((NoneLeafNode)BTreePlus.Deserialize(parent)).UpdateKeysRecurently();
            }            
        }


        public override int GetNextKey()
        {
            return BTreePlus.Deserialize(childNodes.Last()).GetNextKey();
        }

        public override int GetPrevKey()
        {
            return BTreePlus.Deserialize(childNodes.First()).GetPrevKey();
        }

        public override int GetLastKey()
        {
            return keys.Last();
        }


        /// <summary>
        /// print like this:
        /// --|-1 
        /// | |-2
        /// 3 
        /// |
        /// |-|-4
        ///   |-5        
        /// </summary>        
        public override string ToString(string depth)
        {
            string otp = "";
            for (int i = 0; i < keys.Count(); i++)
            {
                otp += BTreePlus.Deserialize(childNodes[i]).ToString(depth + "  ");
                otp += depth + keys[i] + "\n";
            }

            otp += BTreePlus.Deserialize(childNodes.Last()).ToString(depth + "  ");

            return otp;
        }


        public override byte[] NodeToBinary()
        {
            List<byte> otp = new List<byte>();
            //first byte is whether leafNode or not
            otp.Add(1);


            //insert keys number            
            foreach (byte Byte in IntConvert.intToByte4(keys.Count()))
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



            //serialize keys 
            for (int i = 0; i < keys.Count(); i++)
            {
                otp.AddRange(IntConvert.intToByte4(keys[i]));                
            }

            //serialize chilnodes
            for (int i = 0; i < childNodes.Count(); i++)
            {
                otp.AddRange(IntConvert.intToByte4(childNodes[i]));
            }                       

            return otp.ToArray();
        }


        //it doesn't receive nodetype byte!!!!!
        static public NoneLeafNode NodeFromBinary(byte[] src, int blockIndex)
        {
            //get record counter
            int keysNumber = IntConvert.byte4Toint(src); //size of 4 bytes!!
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



            //unserialize keys 
            List<int> keys = new List<int>();
            for (int i = 0; i < keysNumber; i++)
            {
                keys.Add(IntConvert.byte4Toint(src));
                src = src.Skip(4).ToArray();
            }
            

            //unserialize chilnodes
            List<int> childNodes = new List<int>();
            for (int i = 0; i < keysNumber + 1; i++)
            {
                childNodes.Add(IntConvert.byte4Toint(src));
                src = src.Skip(4).ToArray();
            }


            NoneLeafNode otp = new NoneLeafNode(keys, childNodes, blockIndex);
            otp.parent = parent;
            otp.leftSibling = leftSibling;
            otp.rightSibling = rightSibling;


            return otp;
        }
    }
}
