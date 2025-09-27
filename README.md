# File Transfer Tool

A simple and efficient tool for transferring files between devices on the same network. This tool provides a graphical user interface (GUI) to easily select, send, and receive files.

## Features

*   **User-Friendly Interface:** A clean and intuitive graphical user interface for easy operation.
*   **File Selection:** A file dialog allows users to browse and select any file they wish to transfer.
*   **IP Address and Port Configuration:** Manually configure the IP address and port number for both sending and receiving files.
*   **Send Functionality:**  Initiate a file transfer to a receiving device.
*   **Receive Functionality:**  Listen for incoming file transfers on a specified IP address and port.
*   **Cross-platform:** Built with .NET, this tool can be run on any operating system that supports the .NET runtime.

## Getting Started

Follow these instructions to get a copy of the project up and running on your local machine.

### Prerequisites

*   [.NET SDK](https://dotnet.microsoft.com/download)

### Installation

1.  **Clone the repository:**
    ```sh
    git clone https://github.com/MarkoArsov/File-Transfer-Tool.git
    ```
2.  **Navigate to the project directory:**
    ```sh
    cd File-Transfer-Tool/FileTransferTool
    ```
3.  **Build the project:**
    ```sh
    dotnet build
    ```

## Usage

1.  **Run the application:**
    ```sh
    dotnet run
    ```

2.  **To Send a File:**
    *   Click the **"Select File"** button to open a file dialog.
    *   Choose the file you want to send. The file path will appear in the text box.
    *   In the "IP Address" and "Port" fields, enter the IP address and port of the computer that will be receiving the file.
    *   Click the **"Send"** button to begin the transfer.

3.  **To Receive a File:**
    *   On the receiving computer, run the application.
    *   Enter the IP address and port that the sending computer will use to connect. This should be the IP address of the receiving computer itself.
    *   Click the **"Receive"** button. The application will now listen for an incoming file transfer.
    *   Once the file is received, it will be saved in the same directory where the application is running.

