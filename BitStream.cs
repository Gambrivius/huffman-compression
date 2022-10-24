using System;
using System.Collections.Generic;
using System.Text;

namespace Huffman
{
    // this class allows the writing to a bytes object one bit at time, growing if necessary
    public class BitStream
    {
        private UInt64 index;
        private byte[] buffer;
        private UInt64 size;
        private UInt64 actualSize;
        
        public byte[] ReadBytes()
        {
            return buffer;
        }
        public int GetSize()
        {
            return (int)size;
        }
        public BitStream(UInt64 Size)
        {
            // Size is a starting size; can copy and grow later
            size = Size;
            buffer = new byte[size];
            index = 0;
        }
        // this is a special constructor that can be used for reading a bytes[] object 1 bit at a time.
        public BitStream(byte[] input)
        {
            // Size is a starting size; can copy and grow later
            size = (UInt64)input.Length;
            buffer = new byte[size];
            input.CopyTo(buffer, 0);
            index = 0;
        }
        // handles growing the byte array similarly to a vector implementation
        private void Grow()
        {
            UInt64 newSize = (UInt64)(size * 1.2 + 1);
            byte[] newBuffer = new byte[newSize];
            buffer.CopyTo(newBuffer, 0);
            buffer = newBuffer;
            size = newSize;
        }
        // growing will increase the size beyond what may actually be needed
        // this is just to reduce the number of times an O(N) array copy must be executed
        // Truncate() will reduce the size to what was actually written.  This should be called before writing to a file.
        public void Truncate()
        {
            if (size > actualSize)
            {
                Array.Resize(ref buffer, (int)actualSize+1);
                size = actualSize+1;
            }
        }
        // default signature for WriteBit writes the bit to the current index and then increments the index.
        public void WriteBit(uint bit)
        {
            WriteBit(bit, index);
            index++;
            
        }
        public void WriteBit(uint bit, UInt64 position)
        {
            UInt64 byteIndex = position / 8;   // integer division by 8 will take truncate the decimal places / result in floor
            UInt16 bitPosition = (UInt16)(position - (byteIndex * 8));
            if (byteIndex > actualSize) actualSize = byteIndex;
            if (byteIndex >= size) Grow();
            if (bit > 0)
            {
                // turn a bit on.
                buffer[byteIndex] |= (byte)(1 << bitPosition);  // bitwise OR the previous byte with 00000001 shifted to the left byteIndex times...
            } else
            {
                // turn a bit off.
                buffer[byteIndex] &= (byte)(~(1 << bitPosition)); // bitwise AND the previous byte with 11111110 shifted to the left...
            }
        }
        public bool HasBitsRemaining()
        {
            UInt64 byteIndex = index / 8;   // integer division by 8 will take truncate the decimal places / result in floor
            if (byteIndex >= size) return false;
            return true;
        }
        // default signature for ReadBit reads the bit to the current index and then increments the index.
        public int ReadBit()
        {
            int i =  ReadBit(index);
            index++;
            return i;

        }
        public int ReadBit(UInt64 position)
        {
            UInt64 byteIndex = position / 8;   // integer division by 8 will take truncate the decimal places / result in floor
            UInt16 bitPosition = (UInt16)(position - (byteIndex * 8));
            if (byteIndex >= size) return -1;  // end of file
            return (buffer[byteIndex] >> bitPosition) & 1;
        }
    }
    // helper class for encoding values into the byte stream; this object gets passed along the huffman tree to build the sequence of 0s and 1s the represent variable length codes
    public class VarLenByte
    {
        private int length;
        private int value;
        public VarLenByte()
        {

        }
        public VarLenByte (int l, int b)
        {
            length = l;
            value  = b;
        }
        public int GetValue()
        {
            return value;
        }
        // grows the variable length byte and sets the value
        public void AppendBit(int bit)
        {
            if (length >= 32) throw new ArgumentException("VariableByte overflow:  only 32 bits are supported"); 
            if (bit > 0)
            {
                // turn a bit on.
                value |= (1 << length);  // bitwise OR the previous byte with 00000001 shifted to the left byteIndex times...
            }
            else
            {
                // turn a bit off.
                value &= (~(1 << length)); // bitwise AND the previous byte with 11111110 shifted to the left...
            }
            length += 1;
        }
        public int ReadBit (int index)
        {
            if (index >= 32 || index < 0) throw new ArgumentException("Index out of range.");
            return (value >> index) & 1;
        }

        // creates a copy and appends data
        public VarLenByte NextBit(int bit)
        {
            VarLenByte b = new VarLenByte(length, value);
            b.AppendBit(bit);
            return b;
        }

        public void WriteToStream (ref BitStream bs)
        {
            for (int i = 0; i < length; i ++)
            {
                bs.WriteBit((uint)ReadBit(i));
            }
        }
    }
}
