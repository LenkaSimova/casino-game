using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using Avalonia.Data.Converters;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;
using CasinoClient.Models;
using CasinoClient.Services;
using Refit;


namespace CasinoClient.ViewModels;

public interface IVideoApi
{
    [Post("/video/upload")]
    Task<ApiResponse<string>> UploadVideo([AliasAs("device")] int device);
}

public partial class TerminalViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _currentInput = "";

    [ObservableProperty]
    private ObservableCollection<TerminalLine> _terminalLines = new();

    [ObservableProperty]
    private string _prompt = "casino@terminal:~$ "; // Default prompt, will be overridden by config

    [ObservableProperty]
    private bool _isInputFocused = true;

    private readonly List<string> _commandHistory = new();
    private int _historyIndex = -1;

    private readonly Dictionary<string, Func<string[], Task<string>>> _commands = new();
    private readonly Dictionary<string, string> _commandDescriptions = new();


    private TerminalConfig _config = new();


    public TerminalViewModel()
    {
        LoadConfiguration();
        InitializeCommands();
        AddWelcomeMessage();
    }

    private void LoadConfiguration()
    {
        _config = ConfigurationService.LoadConfig();
        Prompt = _config.Prompt;
    }

    private void InitializeCommands()
    {
        // Assign command handlers
        _commands["help"] = HandleHelpCommand;
        _commands["clear"] = HandleClearCommand;
        _commands["echo"] = HandleEchoCommand;
        _commands["status"] = HandleStatusCommand;
        _commands["camera"] = HandleCameraCommand;
        _commands["security"] = HandleSecurityCommand;
        _commands["vault"] = HandleVaultCommand;
        _commands["exit"] = HandleExitCommand;
        _commands["uploadvideo"] = HandleUploadVideoCommand;


        // Initialize command descriptions
        _commandDescriptions["help"] = "Show this help message";
        _commandDescriptions["clear"] = "Clear the terminal screen";
        _commandDescriptions["echo"] = "Echo text back to terminal";
        _commandDescriptions["status"] = "Show system status";
        _commandDescriptions["camera"] = "Access security camera controls";
        _commandDescriptions["security"] = "Access security systems";
        _commandDescriptions["vault"] = "Access vault controls";
        _commandDescriptions["exit"] = "Return to slot machine";
        _commandDescriptions["uploadvideo"] = "Upload video";
    }

    private void AddWelcomeMessage()
    {
        TerminalLines.Add(new TerminalLine("=== CASINO SECURITY TERMINAL ===", TerminalLineType.System));
        TerminalLines.Add(new TerminalLine("Access granted. Type 'help' for available commands.", TerminalLineType.System));
        TerminalLines.Add(new TerminalLine("", TerminalLineType.Normal));
    }

    private void ShowPrompt(string prompt_text = "")
    {
        TerminalLines.Add(new TerminalLine(Prompt + prompt_text, TerminalLineType.Prompt, isInput: true));
    }

    [RelayCommand]
    private async Task ExecuteCommand()
    {
        var input = CurrentInput.Trim();
        if (string.IsNullOrEmpty(input))
        {
            ShowPrompt();
            return;
        }
        ShowPrompt(input);

        // Add to command history
        _commandHistory.Add(input);
        _historyIndex = _commandHistory.Count;

        // Parse and execute command
        var parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length > 0)
        {
            var command = parts[0].ToLower();
            var args = parts.Skip(1).ToArray();

            // Check if command is allowed for this terminal
            if (!_config.AllowedCommands.Contains(command))
            {
                AddOutput($"Access denied: Command '{command}' not available on this terminal.", TerminalLineType.Error);
                CurrentInput = "";
                return;
            }

            if (_commands.ContainsKey(command))
            {
                try
                {
                    var result = await _commands[command](args);
                    if (!string.IsNullOrEmpty(result))
                    {
                        AddOutput(result);
                    }
                }
                catch (Exception ex)
                {
                    AddOutput($"Error executing command: {ex.Message}", TerminalLineType.Error);
                }
            }
            else
            {
                AddOutput($"Command not found: {command}. Type 'help' for available commands.", TerminalLineType.Error);
            }
        }

        CurrentInput = "";
    }

    public void OnKeyDown(string key)
    {
        switch (key)
        {
            case "Up":
                NavigateHistory(-1);
                break;
            case "Down":
                NavigateHistory(1);
                break;
        }
    }

    private void NavigateHistory(int direction)
    {
        if (_commandHistory.Count == 0) return;

        _historyIndex += direction;
        _historyIndex = Math.Max(-1, Math.Min(_historyIndex, _commandHistory.Count - 1));       // Keep between -1 and _commandHistory.Count - 1

        if (_historyIndex >= 0 && _historyIndex < _commandHistory.Count)
        {
            CurrentInput = _commandHistory[_historyIndex];
        }
        else if (_historyIndex == -1)
        {
            CurrentInput = "";
        }
    }

    private void AddOutput(string text, TerminalLineType type = TerminalLineType.Normal)
    {
        var lines = text.Split('\n');
        foreach (var line in lines)
        {
            TerminalLines.Add(new TerminalLine(line, type));
        }
    }

    // Command Handlers
    private Task<string> HandleHelpCommand(string[] args)
    {
        var help = new StringBuilder();
        help.AppendLine("Available commands:");

        // List all available commands
        foreach (var command in _config.AllowedCommands.Where(c => _commandDescriptions.ContainsKey(c)).OrderBy(c => c))
        {
            var description = _commandDescriptions[command];
            help.AppendLine($"  {command.PadRight(12)} - {description}");
        }

        help.AppendLine("");
        help.AppendLine("Use arrow keys to navigate command history.");
        return Task.FromResult(help.ToString());
    }

    private Task<string> HandleClearCommand(string[] args)
    {
        TerminalLines.Clear();
        return Task.FromResult("");
    }

    private Task<string> HandleEchoCommand(string[] args)
    {
        return Task.FromResult(string.Join(" ", args));
    }

    private Task<string> HandleStatusCommand(string[] args)
    {
        return Task.FromResult("System Status: ONLINE\nSecurity Level: HIGH\nActive Connections: 3\nLast Update: " + DateTime.Now.ToString("HH:mm:ss"));
    }

    private Task<string> HandleCameraCommand(string[] args)
    {
        if (args.Length == 0)
        {
            return Task.FromResult("Available cameras: CAM01, CAM02, CAM03, CAM04\nUsage: camera <id> [loop|stop|status]");
        }

        var cameraId = args[0].ToUpper();
        var action = args.Length > 1 ? args[1].ToLower() : "status";

        switch (action)
        {
            case "loop":
                return Task.FromResult($"Camera {cameraId}: Switching to loop playback...");
            case "stop":
                return Task.FromResult($"Camera {cameraId}: Stopping loop playback...");
            case "status":
                return Task.FromResult($"Camera {cameraId}: ACTIVE - Live feed");
            default:
                return Task.FromResult($"Unknown camera action: {action}");
        }
    }

    private Task<string> HandleSecurityCommand(string[] args)
    {
        return Task.FromResult("Security System Access:\n- All sectors: SECURE\n- Motion detectors: ACTIVE\n- Door locks: ENGAGED\n\nNo security breaches detected.");
    }

    private Task<string> HandleVaultCommand(string[] args)
    {
        return Task.FromResult("Vault Access: RESTRICTED\nRequired authorization level: ADMIN\nCurrent user level: OPERATOR\n\nAccess denied.");
    }

    private Task<string> HandleExitCommand(string[] args)
    {
        // Trigger returning to slot machine view
        OnExitRequested?.Invoke();
        return Task.FromResult("Returning to slot machine...");
    }

    private async Task<string> HandleUploadVideoCommand(string[] args)
    {
        try
        {
            var api = RestService.For<IVideoApi>("http://localhost:5122");
            var response = await api.UploadVideo(_config.Id);
            if (response.IsSuccessStatusCode)
            {
                return "Video upload successful!";
            }
            else
            {
                return $"Video upload failed: {response.StatusCode}";
            }
        }
        catch (Exception ex)
        {
            return $"Error uploading video: {ex.Message}";
        }
    }

    public event Action? OnExitRequested;
}

public class TerminalLine
{
    public string Text { get; set; }
    public TerminalLineType Type { get; set; }
    public bool IsInput { get; set; }

    public TerminalLine(string text, TerminalLineType type, bool isInput = false)
    {
        Text = text;
        Type = type;
        IsInput = isInput;
    }
}

public enum TerminalLineType
{
    Normal,
    Prompt,
    System,
    Error,
    Success
}

public class TerminalLineTypeToStyleConverter : IMultiValueConverter
{
    public static readonly TerminalLineTypeToStyleConverter Instance = new();

    public object Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values?.Count >= 2 &&
            values[0] is TerminalLineType type &&
            values[1] is bool isInput)
        {
            if (isInput)
                return "terminal-prompt";

            return type switch
            {
                TerminalLineType.Prompt => "terminal-prompt",
                TerminalLineType.System => "terminal-system",
                TerminalLineType.Error => "terminal-error",
                _ => "terminal-text"
            };
        }

        return "terminal-text";
    }
}