using FileTransferTool;

var userInputService = new UserInputService();
var fileTransferService = new FileTransferService();

try
{
    var (sourceFilePath, destinationFolderPath, fileTransferMode) = userInputService.GetFilePathsAndTransferMode();
    fileTransferService.TransferFile(sourceFilePath, destinationFolderPath, fileTransferMode);
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}
