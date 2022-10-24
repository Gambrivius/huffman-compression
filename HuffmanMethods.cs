using System;
using System.Collections.Generic;
using System.Text;

namespace Huffman
{
    public static class HuffmanMethods
    {
        // the following bytes are reserved and are used for serializing the binary tree
        public const byte NULL_BYTE = 0;
        public const byte PARENT_NODE_BYTE = 1;
        public const byte EOF_BYTE = 4;

        // serialize a node in preorder DFS traversal
        public static List<byte> SerializeNode(HNode node)
        {
            List<byte> buffer = new List<byte>();
            if (node == null) buffer.Add(NULL_BYTE);
            else
            {
                buffer.Add((byte)node.Value);
                buffer.AddRange(SerializeNode(node.Left));
                buffer.AddRange(SerializeNode(node.Right));
            }
            return buffer;
        }
        // deserialize a node in a preorder DFS traversal
        public static HNode DeserializeNode(ref Queue<byte> bytes)
        {
            byte this_byte = bytes.Dequeue();
            if (this_byte == NULL_BYTE) return null;
            HNode n = new HNode((char)this_byte);
            n.Left = DeserializeNode(ref bytes);
            n.Right = DeserializeNode(ref bytes);
            return n;
        }
        public static HNode DeserializeTree(byte[] bytes, out byte[] bytesRemaining)
        {
            Queue<byte> byte_queue = new Queue<byte>(bytes);
            HNode root = DeserializeNode(ref byte_queue);
            bytesRemaining = byte_queue.ToArray();
            return root;
        }
        public static byte[] SerializeTree(HNode root)
        {

            // preorder traversal of tree
            List<byte> buffer = SerializeNode(root);
            return buffer.ToArray();
        }
        public static HNode[] FrequencyCount(string input)
        {
            HNode[] frequency = new HNode[256];
            for (int i = 0; i < 256; i++)
            {
                frequency[i] = new HNode((char)i);
            }
            // ensure string is utf8 encoded as this code assumes 8 bit characters.
            byte[] bytes = Encoding.Default.GetBytes(input);
            input = Encoding.UTF8.GetString(bytes);
            for (int i = 0; i < input.Length; i++)
            {
                char x = input[i];
                UInt16 ord = (UInt16)x;
                if (ord < 256) frequency[ord].Increment();
            }

            // ensure the eof byte gets encoded somewhere on the tree
            frequency[EOF_BYTE].Increment();
            return frequency;
        }

        public static HNode Treeify(HNode[] node_array)
        {
            // create a sorted list
            List<HNode> nodes = new List<HNode>();
            for (int i = 0; i < node_array.Length; i++)
            {
                if (node_array[i].GetCount() > 0)
                {
                    // don't bother counting characters with a frequency of 0
                    nodes.Add(node_array[i]);
                };
            }
            nodes.Sort();
            while (nodes.Count > 1)
            {
                HNode left = nodes[0];
                HNode right = nodes[1];
                nodes.RemoveAt(1);
                nodes.RemoveAt(0);
                // create a new node with combined counts as parent
                // add it back to the array and sort
                HNode n = new HNode((char)PARENT_NODE_BYTE);
                n.SetCount(left.GetCount() + right.GetCount());
                n.Left = left;
                n.Right = right;
                nodes.Add(n);
                nodes.Sort();
            }
            // the root node is now at index 0.
            return nodes[0];
        }

        public static byte[] EncodeString(ref HNode tree, string s)
        {
            BitStream bs = new BitStream(1);
            for (int i = 0; i < s.Length; i++)
            {
                tree.Encode(ref bs, s[i]);
            }
            bs.Truncate();
            return bs.ReadBytes();
        }
        public static string DecodeString(ref HNode tree, byte [] input)
        {
            BitStream bs = new BitStream(input);
            HNode cursor = tree;
            // must use stringbuilder class, otherwise string will be copied everytime it is appended to.
            StringBuilder buffer = new StringBuilder("");
            while (bs.HasBitsRemaining())
            {
                int bit = bs.ReadBit();
                switch (bit)
                {
                    case -1:
                        throw new ArgumentException("Unexpected end of byte stream");
                    case 1:
                        cursor =  cursor.Right;
                        break;
                    case 0:
                        cursor = cursor.Left;
                        break;
                    default:
                        throw new ArgumentException("Unexpected error");
                }
                if (cursor == null) throw new ArgumentException("Unexpected null child");
                // is the cursor at a leaf node?
                if (cursor.Left == null && cursor.Right == null)
                {
                    if (cursor.Value == EOF_BYTE) break;
                    buffer.Append(cursor.Value);
                    cursor = tree;
                }
            }
            
            return buffer.ToString();
        }

        public static byte[] FullEncode(string raw_text)
        {
            
            // files are stored in bytes, but the last byte might be padded with 0s, and we don't want to read those as characters
            // hence and EOF character.  This ensures that the we write a character that the decoder will recognize and stop decoding before it gets to the padded bits.
            raw_text += (char)EOF_BYTE;
            HNode[] frequencyMap = HuffmanMethods.FrequencyCount(raw_text);
            HNode root = HuffmanMethods.Treeify(frequencyMap);
            byte[] encoded_text = EncodeString(ref root, raw_text);
            byte[] serialized_tree = SerializeTree(root);
            int total_size = encoded_text.Length + serialized_tree.Length;
            byte[] output = new byte[total_size];
            serialized_tree.CopyTo(output, 0);
            
            encoded_text.CopyTo(output, serialized_tree.Length);
            return output;
        }
        public static string FullDecode(byte[] input)
        {
            string buffer = "";
            byte[] message;
            HNode root = HuffmanMethods.DeserializeTree(input, out message);
            buffer = DecodeString(ref root, message);
            return buffer;
        }
    }
}
