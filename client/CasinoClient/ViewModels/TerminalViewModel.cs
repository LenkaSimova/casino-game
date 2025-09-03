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

using CasinoClient.Services.Apis;
using CasinoClient.Services.LLMHandlers;


namespace CasinoClient.ViewModels;

public enum TerminalState
{
    Normal,
    LLMConversation
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
    private string _llmprompt = ">>>  "; // Default prompt, will be overridden by config

    [ObservableProperty]
    private bool _isInputFocused = true;

    [ObservableProperty]
    private TerminalState _currentState = TerminalState.Normal;

    private readonly List<string> _commandHistory = new();
    private int _historyIndex = -1;

    private readonly Dictionary<string, Func<string[], Task<CommandResult>>> _commands = new();
    private readonly Dictionary<string, string> _commandDescriptions = new();

    private TerminalConfig _config = new();

    private ILLMHandler _llmHandler = new GeminiLLMHandler();

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
        _commands["security"] = HandleSecurityCommand;
        _commands["vault"] = HandleVaultCommand;
        _commands["exit"] = HandleExitCommand;
        _commands["uploadvideo"] = HandleUploadVideoCommand;
        _commands["loopvideo"] = HandleLoopVideoCommand;
        _commands["musicon"] = HandleMusicOnCommand;
        _commands["lightson"] = HandleLightsOnCommand;
        _commands["llm"] = HandleLLMConversationCommand;
        _commands["updatequery"] = HandleUpdateLLMCommand;



        // Initialize command descriptions
        _commandDescriptions["help"] = "Show this help message";
        _commandDescriptions["clear"] = "Clear the terminal screen";
        _commandDescriptions["echo"] = "Echo text back to terminal";
        _commandDescriptions["status"] = "Show system status";
        _commandDescriptions["security"] = "Access security systems";
        _commandDescriptions["vault"] = "Access vault controls";
        _commandDescriptions["exit"] = "Return to slot machine";
        _commandDescriptions["uploadvideo"] = "Upload video";
        _commandDescriptions["loopvideo"] = "Loop video";
        _commandDescriptions["uploadvideo"] = "Upload video";
        _commandDescriptions["loopvideo"] = "Loop video";
        _commandDescriptions["musicon"] = "Turn the music on";
        _commandDescriptions["lightson"] = "Turn the disco lights on";
        _commandDescriptions["llm"] = "Start LLM conversation mode";
        _commandDescriptions["updatequery"] = "Update LLM query";

    }

    private void AddWelcomeMessage()
    {
        TerminalLines.Add(new TerminalLine("=== CASINO SECURITY TERMINAL ===", TerminalLineType.System));
        TerminalLines.Add(new TerminalLine("Access granted. Type 'help' for available commands.", TerminalLineType.System));
        TerminalLines.Add(new TerminalLine("", TerminalLineType.Normal));
    }

    private void ShowPrompt(string prompt_text = "")
    {
        var promptType = CurrentState == TerminalState.LLMConversation ? TerminalLineType.LLMPrompt : TerminalLineType.Normal;
        TerminalLines.Add(new TerminalLine(Prompt + prompt_text, promptType, isInput: true));
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

        if (CurrentState == TerminalState.LLMConversation)
        {
            await HandleLLMInput(input);
        }
        else
        {
            await HandleNormalCommand(input);
        }

        CurrentInput = "";
    }

    private async Task HandleNormalCommand(string input)
    {
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

            if (_commands.ContainsKey(command))
            {
                // Check if command is allowed for this terminal
                if (!_config.AllowedCommands.Contains(command))
                {
                    AddOutput($"Access denied: Command '{command}' not available on this terminal.", TerminalLineType.Error);
                    return;
                }
                try
                {
                    var result = await _commands[command](args);
                    if (!string.IsNullOrEmpty(result.Message))
                    {
                        // Use the IsSuccess property to determine the message type
                        var messageType = result.IsSuccess ? TerminalLineType.Normal : TerminalLineType.Error;
                        AddOutput(result.Message, messageType);
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
    }

    private async Task HandleLLMInput(string input)
    {
        ShowPrompt(input);

        // Handle special LLM commands
        if (input.ToLower() == "exit" || input.ToLower() == "quit")
        {
            ExitLLMMode();
            return;
        }

        // Send to LLM and show response
        try
        {
            AddOutput("Thinking...", TerminalLineType.LLMSystem);

            var result = await _llmHandler.SendMessageAsync(input);

            // Remove the "Thinking..." line
            if (TerminalLines.LastOrDefault()?.Type == TerminalLineType.LLMSystem)
            {
                TerminalLines.RemoveAt(TerminalLines.Count - 1);
            }
            AddOutput(result, TerminalLineType.LLMResponse);


            // if (result.IsSuccess && !string.IsNullOrEmpty(result.Message))
            // {
            //     AddOutput(result.Message, TerminalLineType.LLMResponse);
            // }
            // else
            // {
            //     AddOutput($"LLM Error: {result.Message}", TerminalLineType.Error);
            // }
        }
        catch (Exception ex)
        {
            AddOutput($"Error communicating with LLM: {ex.Message}", TerminalLineType.Error);
        }
    }

    public void OnKeyDown(string key)
    {
        if (CurrentState == TerminalState.LLMConversation)
        {
            // In LLM mode, only handle 'exit' or 'quit' commands via Enter key
            return;
        }

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
    private Task<CommandResult> HandleHelpCommand(string[] args)
    {
        var help = new StringBuilder();
        help.AppendLine("Available commands:");

        // List all available commands
        foreach (var command in _config.AllowedCommands.Where(c => _commandDescriptions.ContainsKey(c)))
        {
            var description = _commandDescriptions[command];
            help.AppendLine($"  {command.PadRight(12)} - {description}");
        }

        help.AppendLine("");
        help.AppendLine("Use arrow keys to navigate command history.");
        return Task.FromResult(new CommandResult(true, help.ToString()));
    }

    private Task<CommandResult> HandleClearCommand(string[] args)
    {
        TerminalLines.Clear();
        return Task.FromResult(new CommandResult(true, ""));
    }

    private Task<CommandResult> HandleEchoCommand(string[] args)
    {
        return Task.FromResult(new CommandResult(true, string.Join(" ", args)));
    }

    private Task<CommandResult> HandleStatusCommand(string[] args)
    {
        return Task.FromResult(new CommandResult(true, "System Status: ONLINE\nSecurity Level: HIGH\nActive Connections: 3\nLast Update: " + DateTime.Now.ToString("HH:mm:ss")));
    }

    private Task<CommandResult> HandleSecurityCommand(string[] args)
    {
        return Task.FromResult(new CommandResult(true, "Security System Access:\n- All sectors: SECURE\n- Motion detectors: ACTIVE\n- Door locks: ENGAGED\n\nNo security breaches detected."));
    }

    private Task<CommandResult> HandleVaultCommand(string[] args)
    {
        return Task.FromResult(new CommandResult(false, "Vault Access: RESTRICTED\nRequired authorization level: ADMIN\nCurrent user level: OPERATOR\n\nAccess denied."));
    }

    private Task<CommandResult> HandleExitCommand(string[] args)
    {
        // Trigger returning to slot machine view
        OnExitRequested?.Invoke();
        return Task.FromResult(new CommandResult(true, "Returning to slot machine..."));
    }


    private async Task<CommandResult> HandleApiCommand<T>(Func<T, Task<ApiResponse<string>>> apiCall, string successMessage, string operation, bool showContent = false)
    {
        try
        {
            var api = RestService.For<T>(_config.ServerBaseUrl);
            var response = await apiCall(api);

            if (response.IsSuccessStatusCode)
            {
                string message;
                if (showContent && !string.IsNullOrEmpty(response.Content))
                {
                    message = $"{successMessage}\n\nResponse:\n{response.Content}";
                }
                else
                {
                    message = successMessage;
                }
                return new CommandResult(true, message);
            }
            else
            {
                return new CommandResult(false, $"{operation} failed: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            return new CommandResult(false, $"Error during {operation.ToLower()}: {ex.Message}");
        }
    }

    private async Task<CommandResult> HandleUploadVideoCommand(string[] args)
    {
        return await HandleApiCommand<IVideoApi>(
            api => api.UploadVideo(_config.Id),
            "Video upload successful!",
            "Video upload"
        );
    }

    private async Task<CommandResult> HandleLoopVideoCommand(string[] args)
    {
        return await HandleApiCommand<IVideoApi>(
            api => api.LoopVideo(_config.Id),
            "Video loop started successfully!",
            "Video loop"
        );
    }

    private async Task<CommandResult> HandleMusicOnCommand(string[] args)
    {
        return await HandleApiCommand<IDiscoApi>(
            api => api.MusicOn(_config.Id),
            "The music is playing!",
            "turning on music"
        );
    }

    private async Task<CommandResult> HandleLightsOnCommand(string[] args)
    {
        return await HandleApiCommand<IDiscoApi>(
            api => api.LightsOn(_config.Id),
            "The lights are on!",
            "turning on lights"
        );
    }


    private async Task<CommandResult> HandleUpdateLLMCommand(string[] args)
    {
        return await HandleApiCommand<ILLMApi>(
            api => api.UpdatePassword(_config.Id),
            "LLM query updated successfully!",
            "Updating LLM query"
        );
    }

    private async Task<CommandResult> HandleLLMConversationCommand(string[] args)
    {
        await EnterLLMMode();
        return new CommandResult(true, "");
    }

    private async Task EnterLLMMode()
    {
        CurrentState = TerminalState.LLMConversation;
        Prompt = Llmprompt;
        AddOutput("=== LLM CONVERSATION MODE ===", TerminalLineType.LLMSystem);
        AddOutput("You are now in conversation with the LLM. Type 'exit' or 'quit' to return to normal mode.", TerminalLineType.LLMSystem);
        AddOutput("", TerminalLineType.Normal);
        _llmHandler.ClearHistory();
        await SetPasswordIfUpdated();
    }

    private void ExitLLMMode()
    {
        CurrentState = TerminalState.Normal;
        Prompt = _config.Prompt;
        AddOutput("", TerminalLineType.Normal);
        AddOutput("=== Terminal ===", TerminalLineType.System);
        AddOutput("", TerminalLineType.Normal);
    }

    private async Task<bool> SetPasswordIfUpdated()
    {
        try
        {
            var api = RestService.For<ILLMApi>(_config.ServerBaseUrl);
            var response = await api.IsUpdated(_config.Id);

            if (response.IsSuccessStatusCode)
            {
                _llmHandler.AddSystemMessage($"For question 'What is the password?' answer '{_config.Password}'");
                return true;
            }

            return false;
        }
        catch (Exception)
        {
            // do nothing
            return false;
        }
    }

    public event Action? OnExitRequested;
}

public class CommandResult
{
    public bool IsSuccess { get; }
    public string Message { get; }

    public CommandResult(bool isSuccess, string message)
    {
        IsSuccess = isSuccess;
        Message = message;
    }
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
    Success,
    LLMPrompt,
    LLMResponse,
    LLMSystem
}

public class TerminalLineTypeToStringConverter : IValueConverter
{
    public static readonly TerminalLineTypeToStringConverter Instance = new();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is TerminalLine line)
        {
            return line.Type switch
            {
                TerminalLineType.Prompt => "prompt",
                TerminalLineType.System => "system",
                TerminalLineType.Error => "error",
                TerminalLineType.Success => "success",
                // TerminalLineType.LLMPrompt => "llm-prompt",
                // TerminalLineType.LLMResponse => "llm-response",
                // TerminalLineType.LLMSystem => "llm-system",
                _ => "normal"
            };
        }

        return "normal";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}