using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lz4;
using System.IO;
using System.IO.Compression;

namespace MatkoÖnLabProto
{
    class Program
    {
        
        static void Main(string[] args)
        {
            byte[] buffer = new byte[1024];
            using (var be = new FileStream(@"test.txt", FileMode.Open))
            {
                be.Read(buffer, 0, buffer.Length > (int)be.Length ? (int)be.Length : buffer.Length);
                using (var ki = File.Create("test.zip"))
                {
                    using (GZipStream compressionStream = new GZipStream(ki, CompressionMode.Compress))
                    {
                        compressionStream.Write(buffer, 0, buffer.Length > (int)be.Length ? (int)be.Length : buffer.Length);
                    }
                }
            }
            using (var test = new FileStream(@"test.zip", FileMode.Open))
            {
                using (GZipStream compressionStream = new GZipStream(test, CompressionMode.Decompress))
                {
                    compressionStream.Read(buffer, 0, buffer.Length > (int)test.Length ? (int)test.Length : buffer.Length);
                }
            }
            AddFileToArchive("test.txt", "arch.data");
            AddFileToArchive("test2.txt", "arch.data");
            GetFileFromArchive("arch.data", 1, "re.txt");
            Console.WriteLine(Encoding.Default.GetString(buffer));
            using (FileStream fA = new FileStream("test.txt", FileMode.Open))
            {
                fA.Read(buffer, 0, buffer.Length > (int)fA.Length ? (int)fA.Length : buffer.Length);
            }
            Console.WriteLine(Encoding.Default.GetString(buffer));
            using (FileStream fB = new FileStream("re.txt", FileMode.Open))
            {
                fB.Read(buffer, 0, buffer.Length > (int)fB.Length ? (int)fB.Length : buffer.Length);
            }
            Console.WriteLine(Encoding.Default.GetString(buffer));
            Console.ReadKey();
        }

        static void AddFileToArchive(string file, string archive)
        {
            using (FileStream archiveStream = new FileStream(archive, FileMode.OpenOrCreate))
            {
                using (FileStream fs = new FileStream(file, FileMode.Open))
                {
                    long initialOffset = archiveStream.Length;
                    archiveStream.Position = initialOffset;
                    archiveStream.Write(new byte[8], 0, 8); //place for compressed size
                    byte[] k = BitConverter.GetBytes(fs.Length);
                    archiveStream.Write(k, 0, 8); //full size
                    long compressedLength;
                    using (FileStream fileBuffer = new FileStream("FileBufferA.zip", FileMode.Create))
                    {
                        using (GZipStream compressionStream = new GZipStream(fileBuffer, CompressionMode.Compress))
                        {
                            byte[] buffer = new byte[1024];
                            for (long i = 0; i < fs.Length; i += buffer.Length)
                            {
                                fs.Read(buffer, 0, buffer.Length > fs.Length - i ? (int)(fs.Length - i) : buffer.Length);
                                compressionStream.Write(buffer, 0, buffer.Length > fs.Length - i ? (int)(fs.Length - i) : buffer.Length);
                            }
                        }
                    }
                    using (FileStream fileBuffer = new FileStream("FileBufferA.zip", FileMode.Open))
                    {
                        compressedLength = fileBuffer.Length;
                        byte[] buffer = new byte[1024];
                        archiveStream.Position = initialOffset;
                        archiveStream.Write(BitConverter.GetBytes(compressedLength), 0, 8);
                        archiveStream.Position = initialOffset + 16;
                        long remaining = compressedLength;
                        while (remaining > 0)
                        {
                            fileBuffer.Read(buffer, 0, buffer.Length > remaining ? (int)remaining : buffer.Length);
                            archiveStream.Write(buffer, 0, buffer.Length > remaining ? (int)remaining : buffer.Length);
                            remaining -= buffer.Length > remaining ? (int)remaining : buffer.Length;
                        }
                    }
                }
            }
        }

        static void GetFileFromArchive(string archive, int index, string file)
        {
            using (FileStream archiveStream = new FileStream(archive, FileMode.OpenOrCreate))
            {
                long offset = 0;
                byte[] jumpBuffer;
                while (index-- > 0 && offset < archiveStream.Length)
                {
                    archiveStream.Position = offset;
                    jumpBuffer = new byte[16];
                    archiveStream.Read(jumpBuffer, 0, 16);
                    offset += 16 + BitConverter.ToInt64(jumpBuffer, 0);
                }
                if (offset >= archiveStream.Length)
                    return;
                archiveStream.Position = offset;
                jumpBuffer = new byte[8];
                archiveStream.Read(jumpBuffer, 0, 8);
                long compressedFileSize = BitConverter.ToInt64(jumpBuffer, 0);
                archiveStream.Read(jumpBuffer, 0, 8);
                long uncompressedFileSize = BitConverter.ToInt64(jumpBuffer, 0);
                using (FileStream fileBuffer = new FileStream("FileBufferB.zip", FileMode.Create))
                {
                    byte[] buffer = new byte[1024];
                    long remaining = compressedFileSize;
                    while (remaining > 0)
                    {
                        archiveStream.Read(buffer, 0, buffer.Length > remaining ? (int)remaining : buffer.Length);
                        fileBuffer.Write(buffer, 0, buffer.Length > remaining ? (int)remaining : buffer.Length);
                        remaining -= buffer.Length > remaining ? (int)remaining : buffer.Length;
                    }
                }
                using (FileStream fileBuffer = new FileStream("FileBufferB.zip", FileMode.Open))
                {
                    using (GZipStream compressionStream = new GZipStream(fileBuffer, CompressionMode.Decompress))
                    {
                        using (FileStream fs = new FileStream(file, FileMode.Create))
                        {
                            byte[] buffer = new byte[1024];
                            long remaining = uncompressedFileSize;
                            while (remaining > 0)
                            {
                                compressionStream.Read(buffer, 0, buffer.Length > remaining ? (int)remaining : buffer.Length);
                                fs.Write(buffer, 0, buffer.Length > remaining ? (int)remaining : buffer.Length);
                                remaining -= buffer.Length > remaining ? (int)remaining : buffer.Length;
                            }
                        }
                    }
                }
            }
            return;
        }
    }
}
