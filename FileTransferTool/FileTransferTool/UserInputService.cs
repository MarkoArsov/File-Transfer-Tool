using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileTransferTool
{
    public class UserInputService
    {
        public (string SourceFilePath, string DestinationFolderPath, FileTransferMode FileTransferMode) GetFilePathsAndTransferMode()
        {
            string sourceFilePath = GetValidSourceFilePath();
            string destinationFolderPath = GetValidDestinationFolderPath();
            FileTransferMode fileTransferMode = GetFileTransferMode();

            return (sourceFilePath, destinationFolderPath, fileTransferMode);
        }

        private string GetValidSourceFilePath()
        {
            while (true)
            {
                Console.Write("Enter source file path: ");
                string? sourceFilePath = Console.ReadLine();

                if (IsValidFilePath(sourceFilePath))
                    return sourceFilePath!;

                Console.WriteLine("Invalid file path, try again.");
            }
        }

        private string GetValidDestinationFolderPath()
        {
            while (true)
            {
                Console.Write("Enter destination folder path: ");
                string? destinationFolderPath = Console.ReadLine();

                if (!IsValidFolderPath(destinationFolderPath))
                {
                    Console.WriteLine("Invalid folder path, try again.");
                    continue;
                }

                destinationFolderPath = GetValidFolderPathWithCreation(destinationFolderPath!);

                if (destinationFolderPath != null)
                    return destinationFolderPath!;
            } 
        }

        private FileTransferMode GetFileTransferMode()
        {
            while (true)
            {
                Console.WriteLine("Select file transfer mode:");
                Console.WriteLine("1. Default");
                Console.WriteLine("2. Multithreaded (2 threads)");
                Console.WriteLine("2. Multithreaded with shared offset (2 threads)");
                Console.Write("Enter your choice (1, 2 or 3): ");
                string? fileTransferMode = Console.ReadLine();

                if (int.TryParse(fileTransferMode, out int choice) && Enum.IsDefined(typeof(FileTransferMode), choice))
                {
                    return (FileTransferMode)choice;
                }

                Console.WriteLine("Invalid input. Please enter 1, 2 or 3");
            }
        }

        private string? GetValidFolderPathWithCreation(string destinationFolderPath)
        {
            if (Directory.Exists(destinationFolderPath))
                return destinationFolderPath;

            Console.WriteLine($"The folder '{destinationFolderPath}' does not exist.");
            Console.Write("Do you want to create it? (Y/N): ");

            string? response = Console.ReadLine();

            if (response != null && response.ToUpper() == "Y")
            {
                Directory.CreateDirectory(destinationFolderPath);
                return destinationFolderPath;
            }
         
            Console.WriteLine("Operation cancelled.");
            return null; 
        }
        private bool IsValidFilePath(string? path)
        {
            return !string.IsNullOrWhiteSpace(path) && File.Exists(path);
        }

        private bool IsValidFolderPath(string? path)
        {
            return !string.IsNullOrWhiteSpace(path);
        }
    }
}
