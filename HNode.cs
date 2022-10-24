using System;
using System.Collections.Generic;
using System.Text;

namespace Huffman
{
    

    // this class represents a node on the Huffman Binary Tree
    // it is also used as a character frequency counter
    public class HNode : IComparable
    {
        private  UInt32 Count = 0;
        public char? Value { get; set; }
        public HNode Left { get; set; }            // left child or null
        public HNode Right { get; set; }           // right child or null

        public UInt32 GetCount()
        {
            return Count;
        }
        public void Increment()
        {
            Count++;
        }
        public void SetCount( UInt32 c)
        {
            Count = c;
        }
        public HNode (char? value)
        {
            Value = value;
        }
        
        public bool Encode (ref BitStream bs, char v)
        {
            VarLenByte vb = Find(v, null);
            if (vb == null) return false;
            vb.WriteToStream(ref bs);
            return true;
        }
        public VarLenByte? Find(char v, VarLenByte var_code)
        {
            if (var_code == null) var_code = new VarLenByte();
            if (Value == v) return var_code;
            if (Left != null) {
                VarLenByte l = Left.Find(v, var_code.NextBit(0));
                if (l!= null) return l;
            };
            if (Right != null)
            {
                VarLenByte r = Right.Find(v, var_code.NextBit(1));
                if (r != null) return r;
            };
            return null;
        }

        public int CompareTo(object obj)
        {
            if (obj == null) return 1;

            HNode orange = obj as HNode;
            if (orange != null)
                return this.Count.CompareTo(orange.GetCount());
            else
                throw new ArgumentException("Invalid Object Type");
        }

        public void PreorderPrettyPrint()
        {
            Console.WriteLine(Value);
            if (Left != null) Left.PreorderPrettyPrint();
            if (Right != null) Right.PreorderPrettyPrint();

        }
    }
}
