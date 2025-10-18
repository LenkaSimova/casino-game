using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CasinoClient.ViewModels;

/// <summary>
/// Main view model for the application window. Handles switching between SlotMachine and Terminal views.
/// </summary>
public partial class MainWindowViewModel : ViewModelBase
{
    /// <summary>
    /// The currently active view model displayed in the main window.
    /// </summary>
    [ObservableProperty]
    private ViewModelBase _currentViewModel;

    /// <summary>
    /// Cached instance of SlotMachineViewModel to preserve its state when switching views.
    /// </summary>
    private SlotMachineViewModel? _slotMachineViewModel;

    /// <summary>
    /// Initializes the main window view model and sets the initial view to SlotMachine.
    /// </summary>
    public MainWindowViewModel()
    {
        // Create SlotMachineViewModel and subscribe to its event for switching to Terminal view
        _slotMachineViewModel = new SlotMachineViewModel();
        _slotMachineViewModel.OnTerminalSwitchRequested += ShowTerminal;

        CurrentViewModel = _slotMachineViewModel;
    }

    /// <summary>
    /// Command to switch to the Terminal view.
    /// </summary>
    [RelayCommand]
    private void ShowTerminal()
    {
        // Create TerminalViewModel and subscribe to its event for returning to SlotMachine view
        var terminalViewModel = new TerminalViewModel();
        terminalViewModel.OnExitRequested += ShowSlotMachine;
        terminalViewModel.OnAppExitRequested += ExitApplication;

        CurrentViewModel = terminalViewModel;
    }

    /// <summary>
    /// Exits the application (used by hidden orgexit command)
    /// </summary>
    private void ExitApplication()
    {
        OnAppExitRequested?.Invoke();
    }

    /// <summary>
    /// Event to notify the app to exit completely
    /// </summary>
    public event System.Action? OnAppExitRequested;

    /// <summary>
    /// Command to switch back to the SlotMachine view.
    /// </summary>
    [RelayCommand]
    private void ShowSlotMachine()
    {
        // If SlotMachineViewModel exists, reuse it; otherwise, create a new instance
        if (_slotMachineViewModel != null)
        {
            CurrentViewModel = _slotMachineViewModel;
        }
        else
        {
            _slotMachineViewModel = new SlotMachineViewModel();
            _slotMachineViewModel.OnTerminalSwitchRequested += ShowTerminal;
            CurrentViewModel = _slotMachineViewModel;
        }
    }
}