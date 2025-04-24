using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using System.Text;
using System.Text.Json.Serialization;
using System.Diagnostics;

namespace JsonToCsvConverter
{
    // Main application window
    public partial class MainWindow : Window
    {
        private string _selectedJsonFilePath;
        private AppState _appState;
        private readonly string _appDataPath;
        private string _lastTestResultPath;
        private bool _testRunning = false;

        public MainWindow()
        {
            InitializeComponent();
            
            // Set up app data directory
            _appDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "JsonToCsvConverter"
            );
            
            // Create directory if it doesn't exist
            if (!Directory.Exists(_appDataPath))
            {
                Directory.CreateDirectory(_appDataPath);
            }
            
            // Load app state
            LoadAppState();
            
            // Initialize UI
            UpdateHistoryList();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        // Load the application state from disk
        private void LoadAppState()
        {
            string statePath = Path.Combine(_appDataPath, "appState.json");
            
            if (File.Exists(statePath))
            {
                try
                {
                    string json = File.ReadAllText(statePath);
                    _appState = JsonSerializer.Deserialize<AppState>(json);
                }
                catch (Exception ex)
                {
                    // If loading fails, create a new state
                    Console.WriteLine($"Failed to load app state: {ex.Message}");
                    _appState = new AppState { ConversionHistory = new List<ConversionRecord>() };
                }
            }
            else
            {
                // Create new state if file doesn't exist
                _appState = new AppState { ConversionHistory = new List<ConversionRecord>() };
            }
        }

        // Save the application state to disk
        private void SaveAppState()
        {
            string statePath = Path.Combine(_appDataPath, "appState.json");
            
            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(_appState, options);
                File.WriteAllText(statePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to save app state: {ex.Message}");
            }
        }

        // Update the history list in the UI
        private void UpdateHistoryList()
        {
            var historyList = this.FindControl<ListBox>("HistoryListBox");
            
            if (historyList != null)
            {
                var items = new List<string>();
                
                foreach (var record in _appState.ConversionHistory)
                {
                    string fileName = Path.GetFileName(record.InputPath);
                    string date = record.Timestamp.ToString("yyyy-MM-dd HH:mm:ss");
                    items.Add($"{date} - {fileName}");
                }
                
                historyList.Items = items;
            }
        }

        // Handle Select JSON File button click
        private async void OnSelectFileClick(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Title = "Select JSON File",
                AllowMultiple = false,
                Filters = new List<FileDialogFilter>
                {
                    new FileDialogFilter { Name = "JSON Files", Extensions = new List<string> { "json" } }
                }
            };

            var result = await openFileDialog.ShowAsync(this);
            if (result != null && result.Length > 0)
            {
                _selectedJsonFilePath = result[0];
                
                var filePathTextBlock = this.FindControl<TextBlock>("SelectedFilePath");
                if (filePathTextBlock != null)
                {
                    filePathTextBlock.Text = _selectedJsonFilePath;
                }
                
                var fileInfoPanel = this.FindControl<Panel>("FileInfoPanel");
                if (fileInfoPanel != null)
                {
                    fileInfoPanel.IsVisible = true;
                }
            }
        }

        // Handle Convert to CSV button click
        private async void OnConvertClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_selectedJsonFilePath))
            {
                return;
            }
            
            try
            {
                // Read the JSON file
                string jsonContent = File.ReadAllText(_selectedJsonFilePath);
                JsonDocument jsonDocument = JsonDocument.Parse(jsonContent);
                
                // Convert to CSV
                string csvContent = ConvertJsonToCsv(jsonDocument);
                
                // Show save dialog
                var saveFileDialog = new SaveFileDialog
                {
                    Title = "Save CSV File",
                    InitialFileName = Path.GetFileNameWithoutExtension(_selectedJsonFilePath) + ".csv",
                    Filters = new List<FileDialogFilter>
                    {
                        new FileDialogFilter { Name = "CSV Files", Extensions = new List<string> { "csv" } }
                    }
                };

                var outputPath = await saveFileDialog.ShowAsync(this);
                if (!string.IsNullOrEmpty(outputPath))
                {
                    // Save the CSV file
                    File.WriteAllText(outputPath, csvContent);
                    
                    // Create a conversion record
                    var record = new ConversionRecord
                    {
                        Timestamp = DateTime.Now,
                        InputPath = _selectedJsonFilePath,
                        OutputPath = outputPath,
                        JsonData = jsonContent,
                        CsvData = csvContent
                    };
                    
                    // Add to history
                    _appState.ConversionHistory.Add(record);
                    
                    // Save app state
                    SaveAppState();
                    
                    // Update UI
                    UpdateHistoryList();
                    
                    var statusPanel = this.FindControl<Panel>("StatusPanel");
                    var statusMessage = this.FindControl<TextBlock>("StatusMessage");
                    
                    if (statusPanel != null && statusMessage != null)
                    {
                        statusMessage.Text = "Conversion completed successfully!";
                        statusPanel.Classes.Remove("error");
                        statusPanel.Classes.Add("success");
                        statusPanel.IsVisible = true;
                    }
                }
            }
            catch (Exception ex)
            {
                var statusPanel = this.FindControl<Panel>("StatusPanel");
                var statusMessage = this.FindControl<TextBlock>("StatusMessage");
                
                if (statusPanel != null && statusMessage != null)
                {
                    statusMessage.Text = $"Error during conversion: {ex.Message}";
                    statusPanel.Classes.Remove("success");
                    statusPanel.Classes.Add("error");
                    statusPanel.IsVisible = true;
                }
            }
        }

        // Handle selection change in history list
        private void OnHistorySelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var historyList = sender as ListBox;
            
            if (historyList != null && historyList.SelectedIndex >= 0 && historyList.SelectedIndex < _appState.ConversionHistory.Count)
            {
                var record = _appState.ConversionHistory[historyList.SelectedIndex];
                
                var detailsTab = this.FindControl<TabItem>("DetailsTab");
                if (detailsTab != null)
                {
                    // Switch to details tab
                    var tabControl = this.FindControl<TabControl>("MainTabControl");
                    if (tabControl != null)
                    {
                        tabControl.SelectedIndex = 2; // Details tab
                    }
                    
                    // Update details content
                    var detailsTimestamp = this.FindControl<TextBlock>("DetailsTimestamp");
                    var detailsInputPath = this.FindControl<TextBlock>("DetailsInputPath");
                    var detailsOutputPath = this.FindControl<TextBlock>("DetailsOutputPath");
                    var detailsJsonData = this.FindControl<TextBox>("DetailsJsonData");
                    var detailsCsvData = this.FindControl<TextBox>("DetailsCsvData");
                    
                    if (detailsTimestamp != null) detailsTimestamp.Text = record.Timestamp.ToString("yyyy-MM-dd HH:mm:ss");
                    if (detailsInputPath != null) detailsInputPath.Text = record.InputPath;
                    if (detailsOutputPath != null) detailsOutputPath.Text = record.OutputPath;
                    if (detailsJsonData != null) detailsJsonData.Text = record.JsonData;
                    if (detailsCsvData != null) detailsCsvData.Text = record.CsvData;
                }
            }
        }

        // Handle Run Test button click
        private async void OnRunTestClick(object sender, RoutedEventArgs e)
        {
            if (_testRunning)
            {
                return;
            }

            _testRunning = true;
            var statusPanel = this.FindControl<Panel>("TestStatusPanel");
            var statusMessage = this.FindControl<TextBlock>("TestStatusMessage");
            var resultPanel = this.FindControl<StackPanel>("TestResultPanel");
            
            if (statusPanel != null && statusMessage != null && resultPanel != null)
            {
                // Hide result panel
                resultPanel.IsVisible = false;
                
                // Show running status
                statusPanel.IsVisible = true;
                statusMessage.Text = "Running test automation...";
                statusPanel.Classes.Clear();
                statusPanel.Classes.Add("info");
                
                try
                {
                    // Get parameters from text boxes
                    var sutIp = this.FindControl<TextBox>("SutIpTextBox")?.Text ?? "146.208.63.182";
                    var eggDriveIp = this.FindControl<TextBox>("EggDriveIpTextBox")?.Text ?? "10.66.59.118";
                    var testFilter = this.FindControl<TextBox>("TestFilterTextBox")?.Text ?? "F03_0125_01";
                    
                    // Build command
                    string command = $"python /home/keysight/repo/pwst_qa/Eggplant_Projects/TAF/Services/RunTestAutomation.py " +
                                    $"--epf /usr/GNUstep/Local/Applications/Eggplant.app/runscript " +
                                    $"--sut {sutIp} " +
                                    $"--epfSuite C:\\repo\\pwst_qa\\Eggplant_Projects\\Test.suite " +
                                    $"--test /home/keysight/repo/pwst_qa/TestCase " +
                                    $"--script /home/keysight/repo/pwst_qa/Eggplant_Projects/Test.suite/ExecutionPlan " +
                                    $"--eggDriveServer http://{eggDriveIp}:5400 " +
                                    $"--result /home/keysight/repo/pwst_qa/Results " +
                                    $"--apps ST64c " +
                                    $"--filter {testFilter}";
                    
                    // Execute command
                    var result = await RunCommandAsync(command);
                    
                    // Assume the latest JSON file is in the Results directory
                    string resultsDir = "/home/keysight/repo/pwst_qa/Results";
                    
                    // Typically, you'd extract the actual path from the command output
                    // But for now, let's assume it's in a predictable location based on the filter
                    _lastTestResultPath = Path.Combine(resultsDir, $"{testFilter}.json");
                    
                    statusMessage.Text = "Test completed successfully!";
                    statusPanel.Classes.Clear();
                    statusPanel.Classes.Add("success");
                    
                    // Show result panel
                    resultPanel.IsVisible = true;
                }
                catch (Exception ex)
                {
                    statusMessage.Text = $"Error running test: {ex.Message}";
                    statusPanel.Classes.Clear();
                    statusPanel.Classes.Add("error");
                }
                finally
                {
                    _testRunning = false;
                }
            }
        }

        // Handle Import Results button click
        private void OnImportResultsClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_lastTestResultPath) || !File.Exists(_lastTestResultPath))
            {
                var statusPanel = this.FindControl<Panel>("TestStatusPanel");
                var statusMessage = this.FindControl<TextBlock>("TestStatusMessage");
                
                if (statusPanel != null && statusMessage != null)
                {
                    statusMessage.Text = "Cannot find test result file.";
                    statusPanel.Classes.Clear();
                    statusPanel.Classes.Add("error");
                }
                return;
            }
            
            // Set the selected file path
            _selectedJsonFilePath = _lastTestResultPath;
            
            // Update UI in the Converter tab
            var filePathTextBlock = this.FindControl<TextBlock>("SelectedFilePath");
            var fileInfoPanel = this.FindControl<Panel>("FileInfoPanel");
            
            if (filePathTextBlock != null)
            {
                filePathTextBlock.Text = _selectedJsonFilePath;
            }
            
            if (fileInfoPanel != null)
            {
                fileInfoPanel.IsVisible = true;
            }
            
            // Switch to Converter tab
            var tabControl = this.FindControl<TabControl>("MainTabControl");
            if (tabControl != null)
            {
                tabControl.SelectedIndex = 0; // Converter tab
            }
        }

        // Helper method to run command asynchronously
        private async Task<string> RunCommandAsync(string command)
        {
            // For Windows
            var processInfo = new ProcessStartInfo("cmd.exe", $"/c {command}")
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                WorkingDirectory = Environment.CurrentDirectory
            };
            
            // For Linux, uncomment this and comment the Windows version above
            /*
            var processInfo = new ProcessStartInfo("/bin/bash", $"-c \"{command}\"")
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                WorkingDirectory = Environment.CurrentDirectory
            };
            */
            
            var process = new Process { StartInfo = processInfo };
            
            var output = new StringBuilder();
            var error = new StringBuilder();
            
            process.OutputDataReceived += (sender, args) => output.AppendLine(args.Data);
            process.ErrorDataReceived += (sender, args) => error.AppendLine(args.Data);
            
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            
            await process.WaitForExitAsync();
            
            if (process.ExitCode != 0)
            {
                throw new Exception($"Command failed with exit code {process.ExitCode}. Error: {error}");
            }
            
            return output.ToString();
        }

        // Convert JSON to CSV
        private string ConvertJsonToCsv(JsonDocument jsonDocument)
        {
            // Check if the JSON is an array
            if (jsonDocument.RootElement.ValueKind == JsonValueKind.Array)
            {
                return ConvertJsonArrayToCsv(jsonDocument.RootElement);
            }
            // Check if the JSON is an object with an array property
            else if (jsonDocument.RootElement.ValueKind == JsonValueKind.Object)
            {
                // Try to find an array property
                foreach (var property in jsonDocument.RootElement.EnumerateObject())
                {
                    if (property.Value.ValueKind == JsonValueKind.Array)
                    {
                        return ConvertJsonArrayToCsv(property.Value);
                    }
                }
                
                // If no array property found, convert the object directly
                return ConvertJsonObjectToCsv(jsonDocument.RootElement);
            }
            
            throw new InvalidOperationException("JSON structure not supported for CSV conversion");
        }

        // Convert a JSON array to CSV
        private string ConvertJsonArrayToCsv(JsonElement jsonArray)
        {
            if (jsonArray.GetArrayLength() == 0)
            {
                return string.Empty;
            }
            
            // Extract all possible headers from all objects
            var headers = new HashSet<string>();
            foreach (var element in jsonArray.EnumerateArray())
            {
                if (element.ValueKind == JsonValueKind.Object)
                {
                    foreach (var property in element.EnumerateObject())
                    {
                        headers.Add(property.Name);
                    }
                }
            }
            
            // Sort headers for consistent output
            var sortedHeaders = headers.OrderBy(h => h).ToList();
            
            // Build CSV
            var csvBuilder = new StringBuilder();
            
            // Add header row
            csvBuilder.AppendLine(string.Join(",", sortedHeaders.Select(EscapeCsvField)));
            
            // Add data rows
            foreach (var element in jsonArray.EnumerateArray())
            {
                if (element.ValueKind == JsonValueKind.Object)
                {
                    var values = new List<string>();
                    
                    foreach (var header in sortedHeaders)
                    {
                        if (element.TryGetProperty(header, out var property))
                        {
                            values.Add(GetJsonElementValue(property));
                        }
                        else
                        {
                            values.Add(string.Empty);
                        }
                    }
                    
                    csvBuilder.AppendLine(string.Join(",", values.Select(EscapeCsvField)));
                }
            }
            
            return csvBuilder.ToString();
        }

        // Convert a JSON object to CSV (single row)
        private string ConvertJsonObjectToCsv(JsonElement jsonObject)
        {
            var headers = new List<string>();
            var values = new List<string>();
            
            foreach (var property in jsonObject.EnumerateObject())
            {
                headers.Add(property.Name);
                values.Add(GetJsonElementValue(property.Value));
            }
            
            var csvBuilder = new StringBuilder();
            csvBuilder.AppendLine(string.Join(",", headers.Select(EscapeCsvField)));
            csvBuilder.AppendLine(string.Join(",", values.Select(EscapeCsvField)));
            
            return csvBuilder.ToString();
        }

        // Get string value from a JsonElement based on its type
        private string GetJsonElementValue(JsonElement element)
        {
            switch (element.ValueKind)
            {
                case JsonValueKind.String:
                    return element.GetString() ?? string.Empty;
                case JsonValueKind.Number:
                    return element.GetRawText();
                case JsonValueKind.True:
                    return "true";
                case JsonValueKind.False:
                    return "false";
                case JsonValueKind.Null:
                    return string.Empty;
                case JsonValueKind.Object:
                case JsonValueKind.Array:
                    return JsonSerializer.Serialize(element);
                default:
                    return string.Empty;
            }
        }

        // Escape CSV field according to RFC 4180
        private string EscapeCsvField(string field)
        {
            if (string.IsNullOrEmpty(field))
            {
                return string.Empty;
            }
            
            bool needsQuoting = field.Contains(',') || field.Contains('"') || field.Contains('\n') || field.Contains('\r');
            
            if (needsQuoting)
            {
                // Double up any quotes and wrap in quotes
                return $"\"{field.Replace("\"", "\"\"")}\"";
            }
            
            return field;
        }
    }

    // Classes for data storage
    public class AppState
    {
        public List<ConversionRecord> ConversionHistory { get; set; }
    }

    public class ConversionRecord
    {
        public DateTime Timestamp { get; set; }
        public string InputPath { get; set; }
        public string OutputPath { get; set; }
        public string JsonData { get; set; }
        public string CsvData { get; set; }
    }
}