# JSON to CSV Converter for RHEL (C# Implementation)

A desktop application for RHEL that reads JSON files, converts them to CSV format, and stores both the original and converted data.

## Project Structure

```
json-to-csv-converter/
├── Assets/
│   └── app-icon.ico         # Application icon
├── App.axaml                # Application XAML
├── App.axaml.cs             # Application code-behind
├── MainWindow.axaml         # Main window XAML
├── MainWindow.axaml.cs      # Main window code-behind
├── Program.cs               # Entry point
├── JsonToCsvConverter.csproj # Project file
├── bin/                     # Build output
└── obj/                     # Intermediate build files
```

## Features

- Modern UI with tabbed interface
- JSON to CSV conversion with support for various JSON structures
- Saves conversion history with timestamps
- Stores both original JSON and converted CSV data
- Error handling and validation
- Cross-platform (works on RHEL, other Linux distributions, Windows, and macOS)

## Development Options

### Option 1: Develop Directly on RHEL

#### Prerequisites

1. Install .NET SDK:
```bash
# Add Microsoft repository
sudo rpm -Uvh https://packages.microsoft.com/config/rhel/8/packages-microsoft-prod.rpm

# Install .NET SDK
sudo dnf install -y dotnet-sdk-6.0
```

2. Install Visual Studio Code:
```bash
# Add VS Code repository
sudo rpm --import https://packages.microsoft.com/keys/microsoft.asc
sudo sh -c 'echo -e "[code]\nname=Visual Studio Code\nbaseurl=https://packages.microsoft.com/yumrepos/vscode\nenabled=1\ngpgcheck=1\ngpgkey=https://packages.microsoft.com/keys/microsoft.asc" > /etc/yum.repos.d/vscode.repo'

# Install VS Code
sudo dnf check-update
sudo dnf install -y code
```

3. Install VS Code C# extensions:
   - Open VS Code
   - Install "C#" extension by Microsoft
   - Install "Avalonia for VS Code" extension

#### Setup

```bash
# Create project directory
mkdir json-to-csv-converter
cd json-to-csv-converter

# Create project files using the provided code artifacts
# (Create each file manually or copy from your development environment)

# Restore dependencies
dotnet restore
```

### Option 2: Develop on Windows and Deploy to RHEL

#### Prerequisites (Windows)

1. Install Visual Studio 2022 (Community edition is free) or Visual Studio Code
2. Install .NET 6.0 SDK or later
3. Install Avalonia extensions/templates

#### Development Workflow

1. Develop and test on Windows
2. Build for RHEL target:
```bash
dotnet publish -c Release -r rhel-x64 --self-contained true /p:PublishSingleFile=true
```
3. Transfer the published files to RHEL
4. Test and deploy on RHEL

## Building and Running

### Development

```bash
# Build the project
dotnet build

# Run the application
dotnet run
```

### Production Build

```bash
# For RHEL
dotnet publish -c Release -r rhel-x64 --self-contained true /p:PublishSingleFile=true

# The executable will be in bin/Release/net6.0/rhel-x64/publish/
```

### Creating an RPM Package

```bash
# Install FPM (Effing Package Management)
sudo dnf install ruby ruby-devel gcc make rpm-build
gem install fpm

# Create RPM package
fpm -s dir -t rpm -n json-to-csv-converter -v 1.0.0 \
    --description "JSON to CSV Converter" \
    bin/Release/net6.0/rhel-x64/publish/=/usr/local/bin/json-to-csv-converter/ \
    desktop/json-to-csv-converter.desktop=/usr/share/applications/json-to-csv-converter.desktop
```

## Usage

1. Launch the application
2. Click "Select JSON File" and choose a JSON file
3. Click "Convert to CSV" and specify where to save the CSV file
4. View conversion history in the "History" tab
5. View details of past conversions in the "Details" tab

## Key Code Components

- **MainWindow.axaml.cs**: Contains the main application logic including JSON to CSV conversion
- **ConversionRecord**: Data structure to store information about each conversion
- **AppState**: Manages the application state and conversion history
