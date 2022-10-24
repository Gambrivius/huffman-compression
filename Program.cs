using System;
using System.IO;

namespace Huffman
{
    class Program
    {
        static public void PrintBytes(byte[] buffer)
        {
            for (int i = 0; i < buffer.Length; i ++)
            {
                Console.WriteLine($"{buffer[i]}, {(char)buffer[i]}");
            }
        }
        static void Main()
        {
            string test_file = "wap.txt";
            string output_file = "encode_wap.dat";
            Console.WriteLine("Running the Huffman Compression Algorithm Test Program");
            Console.WriteLine("Developed by Chris Laponsie");
            Console.WriteLine("--- Test 1:  Hello World String ---");
            string test = "Hello World!";
            Console.WriteLine($"Original string '{test}' is {test.Length} bytes.");
            byte[] encoded = HuffmanMethods.FullEncode(test);
            Console.WriteLine($"'{test}' encoded to {encoded.Length} bytes. (This includes the serialized binary tree)");
            string output = HuffmanMethods.FullDecode(encoded);
            Console.WriteLine($"Decoded '{output}' to {output.Length} bytes");
            Console.WriteLine("Press any key to run Test 2...");
            Console.ReadKey();
            Console.WriteLine("--- Test 2:  War and Peace ---");

            if (!File.Exists(test_file))
            {
                Console.WriteLine("ERROR: File does not exist!");
                throw new FileLoadException("File does not exist");
            }
            string raw_text = File.ReadAllText(test_file);
            int original_size = raw_text.Length;
            Console.WriteLine($"Loaded text file as {raw_text.Length} bytes.");
            Console.WriteLine("Encoding file.  This could take a minute.  Code has not been optimized for speed.");
            // this takes a minute.  I know I could speed this up by enabling "unsafe" as a compiler option and using a pointer
            // to navigate the tree.  I think the tree is being copied a bunch due to pass by value in the encoding algorithm.
            encoded = HuffmanMethods.FullEncode(raw_text);
            int encoded_size = encoded.Length;
            Console.WriteLine($"Encoded file to {encoded.Length} bytes.");
            double reduction = Math.Round((1 - ((double)encoded_size / (double)original_size)) * 100);
            Console.WriteLine($"This is a {reduction}% reduction.");
            Console.WriteLine($"Writing to file {output_file}");
            using (var fs = new FileStream(output_file, FileMode.Create, FileAccess.Write))
            {
                fs.Write(encoded, 0, encoded.Length);
            }
            Console.WriteLine("Decoding file.  This could take a minute.  Code has not been optimized for speed.");
            output = HuffmanMethods.FullDecode(encoded);
            Console.WriteLine($"File decoded to {output.Length} bytes.");

            /*
            Console.WriteLine("Binary tree:");
            root.PreorderPrettyPrint();
            byte[] serial_tree = HuffmanMethods.SerializeTree(root);
            Console.WriteLine("Recontructed Binary tree:");
            HNode root2 = HuffmanMethods.DeserializeTree(serial_tree);
            root2.PreorderPrettyPrint();*/





        }
    }
}
