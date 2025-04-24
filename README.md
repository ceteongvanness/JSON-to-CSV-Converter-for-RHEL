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

## Development with VS Code

### Setting Up VS Code for Development

#### Option 1: Set Up on RHEL

1. **Install .NET SDK**:
```bash
# Add Microsoft repository
sudo rpm -Uvh https://packages.microsoft.com/config/rhel/8/packages-microsoft-prod.rpm

# Install .NET SDK
sudo dnf install -y dotnet-sdk-6.0
```

2. **Install Visual Studio Code**:
```bash
# Add VS Code repository
sudo rpm --import https://packages.microsoft.com/keys/microsoft.asc
sudo sh -c 'echo -e "[code]\nname=Visual Studio Code\nbaseurl=https://packages.microsoft.com/yumrepos/vscode\nenabled=1\ngpgcheck=1\ngpgkey=https://packages.microsoft.com/keys/microsoft.asc" > /etc/yum.repos.d/vscode.repo'

# Install VS Code
sudo dnf check-update
sudo dnf install -y code
```

#### Option 2: Set Up on Windows and Deploy to RHEL

1. Install .NET 6.0 SDK or later from the [Microsoft website](https://dotnet.microsoft.com/download)
2. Install Visual Studio Code from the [official website](https://code.visualstudio.com/)
3. Build for RHEL when ready:
```bash
dotnet publish -c Release -r rhel-x64 --self-contained true /p:PublishSingleFile=true
```
4. Transfer the published files to RHEL for testing

### Essential VS Code Extensions

Install these extensions in VS Code for the best C# development experience:

1. **C# Extension** (by Microsoft):
   - Provides C# language support
   - Enables IntelliSense and code navigation
   - Integrated debugging support

2. **Avalonia for VS Code**:
   - XAML preview and editing features
   - Avalonia-specific IntelliSense

3. **C# XML Documentation Comments**:
   - Better documentation support

4. **.NET Core Tools**:
   - Enhanced .NET project management

### Project Setup in VS Code

1. **Create project folder and open in VS Code**:
```bash
mkdir json-to-csv-converter
cd json-to-csv-converter
code .
```

2. **Initialize a new project in the VS Code terminal** (Ctrl+`):
```bash
dotnet new console
```

3. **Add required NuGet packages**:
```bash
dotnet add package Avalonia --version 0.10.18
dotnet add package Avalonia.Desktop --version 0.10.18
dotnet add package System.Text.Json --version 6.0.7
```

4. **Create project files based on the provided artifacts**:
   - Create each file with correct naming and location
   - Replace the default `.csproj` file with the provided one

5. **Configure debugging**:
   - VS Code will automatically create `.vscode` folder with `launch.json` and `tasks.json`
   - Press F5 to start debugging your application

### VS Code Keyboard Shortcuts for C# Development

| Shortcut | Action |
|----------|--------|
| F5 | Start debugging |
| Ctrl+F5 | Run without debugging |
| F11 | Step into |
| F10 | Step over |
| Shift+F5 | Stop debugging |
| Ctrl+Space | Trigger suggestions |
| F12 | Go to definition |
| Alt+F12 | Peek definition |
| Shift+F12 | Find all references |
| Ctrl+K Ctrl+C | Comment selection |
| Ctrl+K Ctrl+U | Uncomment selection |

## Building and Running in VS Code

### Using VS Code Tasks

VS Code provides convenient ways to build and run your application using tasks:

1. **Open the Command Palette** (Ctrl+Shift+P)
2. Type "Tasks: Configure Default Build Task" and select it
3. Choose "Create tasks.json file from template"
4. Select ".NET Core"

This creates a `.vscode/tasks.json` file. You can customize it or use the default tasks:

### Build Tasks

- **Build**: Press Ctrl+Shift+B or use Command Palette → "Tasks: Run Build Task"
- **Clean**: Use Command Palette → "Tasks: Run Task" → "clean"
- **Publish**: Use Command Palette → "Tasks: Run Task" → "publish"

### Running and Debugging

- **Run without debugging**: Press Ctrl+F5
- **Start debugging**: Press F5
- **Attach to process**: Use Command Palette → "Debug: Attach to Process"

### Running from Terminal in VS Code

You can also use the integrated terminal (Ctrl+`) to run commands:

```bash
# Build the project
dotnet build

# Run the application
dotnet run

# Create a production build for RHEL
dotnet publish -c Release -r rhel-x64 --self-contained true /p:PublishSingleFile=true
```

The published executable will be in `bin/Release/net6.0/rhel-x64/publish/`

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

## VS Code Workflow Tips

### Effective C# Development in VS Code

1. **Use the Solution Explorer**:
   - Install the "Solution Explorer" extension for a Visual Studio-like experience
   - Makes it easier to navigate between project files

2. **Customize IntelliSense**:
   - Go to Settings (Ctrl+,) and search for "intellisense"
   - Adjust settings like suggestion mode, tab completion, etc.

3. **Use the C# Interactive Window**:
   - Install the ".NET Interactive Notebooks" extension
   - Test code snippets quickly without modifying your project

4. **Utilize Code Snippets**:
   - Type "prop" and press Tab to create a property
   - Type "ctor" and press Tab for a constructor
   - Type "cw" and press Tab for Console.WriteLine()

5. **Organize Imports**:
   - Right-click in editor and select "Organize Imports" to clean up using statements
   - Configure automatic organization in settings

### Cross-Platform Development Tips

When developing on one platform and deploying to another:

1. **Use Path.Combine() for file paths** instead of hardcoding separators
2. **Test file permissions logic** carefully when moving from Windows to Linux
3. **Avoid platform-specific APIs** that might not be available on RHEL
4. **Set up a CI/CD pipeline** to automatically test on both platforms

### Troubleshooting Common Issues

1. **OmniSharp Not Loading**:
   - Check the Output panel (View > Output) and select "OmniSharp Log"
   - Verify .NET SDK is properly installed
   - Try running `dotnet restore` manually
   
2. **XAML Preview Not Working**:
   - Make sure the Avalonia extension is properly installed
   - Restart VS Code and/or OmniSharp
   
3. **Build Errors**:
   - Check the Problems panel (Ctrl+Shift+M)
   - Verify all NuGet packages are properly restored
   - Clean solution and rebuild: `dotnet clean && dotnet build`
   
4. **Runtime Errors on RHEL**:
   - Check for missing dependencies: `ldd your-application`
   - Verify permissions: `chmod +x your-application`
   - Test with verbose output: `DOTNET_ROOT=/usr/share/dotnet ./your-application`
