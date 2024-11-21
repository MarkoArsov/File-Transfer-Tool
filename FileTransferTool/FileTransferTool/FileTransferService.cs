using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace FileTransferTool
{
    public class FileTransferService
    {
        private readonly int ChunkSize;
        private readonly int MaxRetries;

        private readonly object LockObject;
        private long SharedOffeset;

        private Stopwatch Stopwatch;

        public FileTransferService()
        {
            ChunkSize = 1024 * 1024; //1MB
            MaxRetries = 3;

            LockObject = new object();
            SharedOffeset = 0;

            Stopwatch = new Stopwatch();
        }

        public FileTransferService(int chunkSize, int maxRetries)
        {
            ChunkSize = chunkSize;
            MaxRetries = maxRetries;

            LockObject = new object();
            SharedOffeset = 0;

            Stopwatch = new Stopwatch();
        }

        public bool TransferFile(string sourceFilePath, string destinationFolderPath, FileTransferMode mode)
        {
            Stopwatch.Start();

            string destinationFilePath = Path.Combine(destinationFolderPath, Path.GetFileName(sourceFilePath));

            FileInfo fileInfo = new FileInfo(sourceFilePath);
            long fileSize = fileInfo.Length;

            return mode switch
            {
                FileTransferMode.Default => TransferFileDefault(sourceFilePath, destinationFilePath, fileSize),
                FileTransferMode.Multithreaded => TransferFileWithTwoThreads(sourceFilePath, destinationFilePath, fileSize),
                FileTransferMode.MultithreadedWithCounter => TransferFileWithTwoThreadsAndCounter(sourceFilePath, destinationFilePath, fileSize),
                _ => throw new ArgumentException("Invalid transfer mode selected")
            };
        }

        private bool TransferFileDefault(string sourceFilePath, string destinationFilePath, long fileSize)
        {
            TransferAndVerifyChunks(sourceFilePath, destinationFilePath, 0, fileSize);

            TransferCompletedNotification();

            return CompareChecksums(sourceFilePath, destinationFilePath);
        }

        private bool TransferFileWithTwoThreads(string sourceFilePath, string destinationFilePath, long fileSize)
        {
            long middlePoint = fileSize / 2;
            
            Thread thread1 = new Thread(() => TransferAndVerifyChunks(sourceFilePath, destinationFilePath, 0, middlePoint));
            Thread thread2 = new Thread(() => TransferAndVerifyChunks(sourceFilePath, destinationFilePath, middlePoint, fileSize - middlePoint));

            thread1.Start();
            thread2.Start();

            thread1.Join();
            thread2.Join();

            TransferCompletedNotification();

            return CompareChecksums(sourceFilePath, destinationFilePath);
        }

        private bool TransferFileWithTwoThreadsAndCounter(string sourceFilePath, string destinationFilePath, long fileSize)
        {
            Thread thread1 = new Thread(() => TransferAndVerifyChunksWithCounter(sourceFilePath, destinationFilePath, fileSize));
            Thread thread2 = new Thread(() => TransferAndVerifyChunksWithCounter(sourceFilePath, destinationFilePath, fileSize));

            thread1.Start();
            thread2.Start();

            thread1.Join();
            thread2.Join();

            SharedOffeset = 0;

            TransferCompletedNotification();

            return CompareChecksums(sourceFilePath, destinationFilePath);
        }

        private bool TransferFileWithMultipleThreads(string sourceFilePath, string destinationFilePath, long fileSize, int numberOfThreads)
        {
            if (numberOfThreads > 2)
                throw new ArgumentException("Number of threads must be greater than one");

            long chunkSize = fileSize / numberOfThreads;
            List<Thread> threads = new List<Thread>();

            for (int i = 0; i < numberOfThreads; i++)
            {
                long start = i * chunkSize;
                long end = (i == numberOfThreads - 1) ? fileSize : start + chunkSize;

                Thread thread = new Thread(() => TransferAndVerifyChunks(sourceFilePath, destinationFilePath, start, end - start));
                threads.Add(thread);
                thread.Start();
            }

            foreach (Thread thread in threads)
            {
                thread.Join();
            }

            TransferCompletedNotification();

            return CompareChecksums(sourceFilePath, destinationFilePath);
        }

        private bool TransferFileWithMultipleThreadsAndCounter(string sourceFilePath, string destinationFilePath, long fileSize, int numberOfThreads)
        {
            if (numberOfThreads > 2)
                throw new ArgumentException("Number of threads must be greater than one");

            List<Thread> threads = new List<Thread>();

            for (int i = 0; i < numberOfThreads; i++)
            {
                Thread thread = new Thread(() => TransferAndVerifyChunksWithCounter(sourceFilePath, destinationFilePath, fileSize));
                threads.Add(thread);
                thread.Start();
            }

            foreach (Thread thread in threads)
            {
                thread.Join();
            }

            SharedOffeset = 0;

            TransferCompletedNotification();

            return CompareChecksums(sourceFilePath, destinationFilePath);
        }

        private void TransferAndVerifyChunks(string sourceFilePath, string destinationFilePath, long startOffset, long length)
        {     
            using (FileStream sourceStream = new FileStream(sourceFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (FileStream destinationStream = new FileStream(destinationFilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                long offset = startOffset;
                while (length > 0)
                {
                    var (buffer, bytesRead) = TransferChunk(sourceStream, destinationStream, offset);

                    int attempt = 0;

                    while (!VerifyChunk(buffer, bytesRead, destinationStream, offset) && attempt < MaxRetries)
                    {
                        (buffer, bytesRead) = TransferChunk(sourceStream, destinationStream, offset);
                        attempt++;
                    }

                    offset += bytesRead;
                    length -= bytesRead;
                }
            }
        }

        private void TransferAndVerifyChunksWithCounter(string sourceFilePath, string destinationFilePath, long length)
        {
            using (FileStream sourceStream = new FileStream(sourceFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (FileStream destinationStream = new FileStream(destinationFilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                long offset;

                while (true)
                {
                    lock (LockObject)
                    {
                        if (SharedOffeset >= length)
                            break;

                        offset = SharedOffeset;
                        SharedOffeset += ChunkSize;
                    }

                    var (buffer, bytesRead) = TransferChunk(sourceStream, destinationStream, offset);

                    int attempt = 0;

                    while (!VerifyChunk(buffer, bytesRead, destinationStream, offset) && attempt < MaxRetries)
                    {
                        (buffer, bytesRead) = TransferChunk(sourceStream, destinationStream, offset);
                        attempt++;
                    }
                }
            }
        }

        private (byte[] Buffer, int BytesRead) TransferChunk(FileStream sourceStream, FileStream destinationStream, long offset)
        {
            byte[] buffer = new byte[ChunkSize];
            sourceStream.Seek(offset, SeekOrigin.Begin);
            int bytesRead = sourceStream.Read(buffer, 0, buffer.Length);

            destinationStream.Seek(offset, SeekOrigin.Begin);
            destinationStream.Write(buffer, 0, bytesRead);

            return (buffer, bytesRead);
        }

        private bool VerifyChunk(byte[] buffer, int bytesRead, FileStream destinationStream, long offset)
        {
            byte[] destinationBuffer = new byte[ChunkSize];

            destinationStream.Seek(offset, SeekOrigin.Begin);
            destinationStream.Read(destinationBuffer, 0, bytesRead);

            byte[] chunkHash = MD5.HashData(buffer);
            byte[] destinationHash = MD5.HashData(destinationBuffer);

            if (chunkHash.SequenceEqual(destinationHash))
            {
                Console.WriteLine($"position = {offset / 1024}, hash = {GetChecksumString(chunkHash)}");
                return true;
            }

            return false;
        }

        private bool CompareChecksums(string source, string destination)
        {
            using (FileStream sourceStream = new FileStream(source, FileMode.Open, FileAccess.Read))
            using (FileStream destinationStream = new FileStream(destination, FileMode.Open, FileAccess.Read))
            {
                byte[] sourceChecksum = SHA256.HashData(sourceStream);
                byte[] destinationChecksum = SHA256.HashData(destinationStream);

                Console.WriteLine("===========================================================");
                Console.WriteLine($"source checksum: {GetChecksumString(sourceChecksum)}");
                Console.WriteLine($"destination checksum: {GetChecksumString(destinationChecksum)}");
                Console.WriteLine("===========================================================");

                if (sourceChecksum.SequenceEqual(destinationChecksum))
                {
                    Console.WriteLine("successfull file transfer");
                    return true;
                }

                Console.WriteLine("checksums don't match");
                return false;
            }
           
        }
        
        private void TransferCompletedNotification()
        {
            Stopwatch.Stop();
            Console.WriteLine("===========================================================");
            Console.WriteLine($"transfer completed in {Stopwatch.Elapsed.TotalSeconds:F2} seconds");
            Console.WriteLine("===========================================================");
            Console.WriteLine("comparing checksums...");
        }

        private string GetChecksumString(byte[] checksum)
        {
            return BitConverter.ToString(checksum).Replace("-", "").ToLowerInvariant();
        }
    }
}
