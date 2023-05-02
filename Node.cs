using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace StukturyBazDanych2
{

    [Serializable()]
    internal class Node
    {        
        protected int blockPos;
        public int GetBlockPos()
        {
            return blockPos;
        }

        protected int LeftSibling = -1;

        public int leftSibling { 
            set {
                LeftSibling = value;
                //BTreePlus.Serialize(this);
            }
            get
            {
                return LeftSibling;
            } 
        }
        protected int RightSibling = -1;
        public int rightSibling
        {
            set
            {
                RightSibling = value;
                //BTreePlus.Serialize(this);
            }
            get
            {
                return RightSibling;
            }
        }
        protected int Parent = -1;
        public int parent
        {
            set
            {
                Parent = value;
                //BTreePlus.Serialize(this);
            }
            get
            {
                return Parent;
            }
        }


        virtual public Record? GetRecord(int key) { return null; }
        public virtual Record? GetNextNode(int prevKey, int src) { return null; }
        public Record? GetPreviousNode(int key) { return null; }
        virtual public Record? GetNthRecord(int n) { return null; }
        virtual public void InsertRecord(RecordKey record) {}
        virtual public void UpdateRecord(RecordKey record) {}

        virtual public void DeleteRecord(int key) {}

        public virtual void Split() { }

        protected void SetSiblingsForSplit(Node rightNodeNew)
        {

            //if was the last node
            if (rightSibling == -1)
            {
                rightSibling = rightNodeNew.GetBlockPos();
                rightNodeNew.leftSibling = this.GetBlockPos();

                BTreePlus.Serialize(this);
                BTreePlus.Serialize(rightNodeNew);
            }
            else
            {

                Node rightSiblingOld = BTreePlus.Deserialize(rightSibling);

                this.rightSibling = rightNodeNew.GetBlockPos();

                rightNodeNew.leftSibling = this.GetBlockPos();
                rightNodeNew.rightSibling = rightSiblingOld.GetBlockPos();

                rightSiblingOld.leftSibling = rightNodeNew.GetBlockPos();

                BTreePlus.Serialize(this);
                BTreePlus.Serialize(rightNodeNew);
                BTreePlus.Serialize(rightSiblingOld);
            }
        }

        //correct siblings next to the deleted node
        protected void SetSiblingsForMerge(int leftNodeBlock, int rightNodeBlock)
        {
            Node leftNode = null, rightNode = null ;


            if(leftNodeBlock >= 0)
            {
                leftNode = BTreePlus.Deserialize(leftNodeBlock);
            }


            if (rightNodeBlock >= 0)
            {
                rightNode = BTreePlus.Deserialize(rightNodeBlock);
            }


            if (leftNode != null && rightNode != null)
            {
                leftNode.rightSibling = rightNode.GetBlockPos();
                rightNode.leftSibling = leftNode.GetBlockPos();

                BTreePlus.Serialize(leftNode);
                BTreePlus.Serialize(rightNode);
            }
            else
            {
                if(rightNode == null && leftNode == null)
                {
                    //there is nothing left in the tree
                }
                else
                {
                    if(leftNode == null)
                    {
                        rightNode.leftSibling = -1;
                        BTreePlus.Serialize(rightNode);
                    }
                    else
                    {
                        leftNode.rightSibling = -1;
                        BTreePlus.Serialize(leftNode);
                    }
                }
            }


        }

        public virtual void Merge(Node callingChild, bool isLeaf) { }

        public virtual void AddKey(int left,int key ,int right) { }
        public virtual int GetLastKey() { return 0; }
        public virtual int GetNextKey() { return 0; }
        public virtual int GetPrevKey() { return 0; }
        public virtual void Compensate() { }
        public virtual int GetKeysNumber() { return 0; }
        public string ToString()
        {
            return "";
        }

        public virtual string ToString(string depth)
        {
            return "";
        }

        public virtual byte[] NodeToBinary()
        {
            return null;
        }

    }
}
