using System;
using System.IO;
using System.Text;
namespace StarfallExArchiver
{
    class Program
    {
        public static void AppendAllBytes(string path, byte[] bytes)
        {
            //argument-checking here.

            using (var stream = new FileStream(path, FileMode.Append))
            {
                stream.Write(bytes, 0, bytes.Length);
            }
        }
        public static void ReferenceEndByte(string filename)
        {
            using (var stream = new FileStream(filename, FileMode.Open))
            {
                var bytes = BitConverter.GetBytes(Convert.ToUInt16(stream.Length));
                stream.Position = 12;
                stream.Write(bytes, 0, bytes.Length);
            }
        }
        public static byte[] GetEndByte(string filename)
        {
            using (var stream = new FileStream(filename, FileMode.Open))
            {
                var bytes = BitConverter.GetBytes(Convert.ToUInt32(stream.Length));
                return bytes;
            }
        }

        static void Main(string[] args)
        {
            String filename, path;
            Console.WriteLine("StarfallEx Archiver");
            if (args.Length >= 1)
            {
                filename = args[0];
            }
            else
            {
                Console.WriteLine("Enter filename:");
                filename = Console.ReadLine();
            }

            if (args.Length >= 2)
            {
                path = args[1];
            }
            else
            {
                Console.WriteLine("Enter path:");
                path = Console.ReadLine();
            }

            byte[] idstring = { 0x53, 0x58, 0x41, 0x52 };
            //byte[] fileCount = { 0x00, 0x00, 0x00, 0x00 };
            byte[] headerStart = { 0x10, 0x00, 0x00, 0x00 };
            byte[] dataStart = { 0x00, 0x00, 0x00, 0x00 };
            int fileCounti = 0;
            var dir = Directory.EnumerateFiles(path, "*.*", SearchOption.AllDirectories);
            foreach (string file in dir)
            {
                //Console.WriteLine(file);
                fileCounti += 1;
            }
            byte[] fileCount = BitConverter.GetBytes(Convert.ToUInt32(fileCounti));
            //fileCount[0] = Convert.ToByte(fileCounti);
            //Console.WriteLine("Hex: {0:X}", fileCount[1]);

            File.WriteAllBytes(filename, idstring);
            File.WriteAllText("content.bin", String.Empty);
            AppendAllBytes(filename, fileCount);
            AppendAllBytes(filename, headerStart);
            AppendAllBytes(filename, dataStart);
            foreach (string file in dir)
            {
                var tempFile = file.ToString().Substring(path.Length).Replace('\\', '/');
                var tempLength = Convert.ToByte(tempFile.Length);
                byte[] stream = File.ReadAllBytes(file);
                byte[] contentLength = GetEndByte("content.bin");

                //Console.WriteLine(Encoding.Default.GetString(contentLength));
                AppendAllBytes(filename, new byte[] { tempLength });
                AppendAllBytes(filename, Encoding.ASCII.GetBytes(tempFile));
                //figure out how to write from content.bin
                AppendAllBytes(filename, contentLength);
                
                var fileSize = BitConverter.GetBytes(Convert.ToUInt32(stream.Length));
                AppendAllBytes(filename, fileSize );
                AppendAllBytes("content.bin", stream);
            }
            ReferenceEndByte(filename);
            byte[] finalContent = File.ReadAllBytes("content.bin");
            AppendAllBytes(filename, finalContent);
        }
    }
}
