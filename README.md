# C# Utilities Repository

## Overview
The **Utilities** repository provides a collection of helper classes designed to simplify and enhance various development tasks in .NET projects. This repository includes essential functionalities such as logging, file management, data formatting, security, validation, and more.

## Features
- **Console.cs**: Enhancements for console-based applications, including color-coded output and input handling.
- **File.cs**: File manipulation utilities for reading, writing, and managing files.
- **Format.cs**: Data formatting helpers for common transformations and string manipulations.
- **Logger.cs**: A simple logging mechanism for debugging and application monitoring.
- **NumberSystemConverter.cs**: Conversion utilities between different number systems (binary, decimal, hexadecimal, etc.).
- **Security.cs**: Basic encryption, decryption, and hashing utilities.
- **Utilities.cs**: General helper functions used across multiple areas.
- **Validation.cs**: Input validation utilities for strings, numbers, and other data types.
- **Windows.cs**: Windows-specific utilities for interacting with the OS.

## Installation
Clone the repository using the following command:
```sh
git clone https://github.com/Yousef-Refat/CS_Utilities.git
```
Include the required files in your .NET project.

## Usage
Each class provides static methods that can be directly used in your application. Below is a basic example of how to use the **clsUtil** class:
```csharp
    clsUtil.CreateFolderIfDoesNotExist("C:\\Users\\Yousef\\Documents\\New Folder");
```
For more details on each utility, refer to the inline documentation within the respective files.

## Contributing
Contributions are welcome! If you find a bug or have an enhancement suggestion, feel free to submit a pull request.

## License
This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.