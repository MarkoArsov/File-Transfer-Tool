# File Transfer Tool

A secure, multi-threaded command-line tool for transferring files over a network, built with .NET. This application uses a client-server architecture to send and receive files, with AES encryption to ensure data privacy during transit.

## Core Features

*   **Secure Transfers:** All files are encrypted using AES (Advanced Encryption Standard) before transmission and decrypted upon arrival.
*   **Multi-threaded Server:** The server is capable of handling multiple client connections simultaneously, allowing for concurrent file transfers.
*   **Command-Line Interface:** A straightforward and scriptable CLI for initiating transfers.
*   **Cross-Platform:** Developed with .NET, it can be compiled and run on Windows, macOS, and Linux.

## Getting Started

Follow these instructions to get a copy of the project up and running on your local machine for development and testing purposes.

### Prerequisites

You will need the [.NET SDK](https://dotnet.microsoft.com/download) installed on your machine.

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
