using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using System.Threading.Tasks;
using System.Diagnostics;

namespace CasinoClient.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty]
    private ViewModelBase _currentViewModel;

    private SlotMachineViewModel? _slotMachineViewModel;

    public MainWindowViewModel()
    {
        // Set initial view model to SlotMachineViewModel
        _slotMachineViewModel = new SlotMachineViewModel();
        _slotMachineViewModel.OnTerminalSwitchRequested += ShowTerminal;
        CurrentViewModel = _slotMachineViewModel;
    }

    [RelayCommand]
    private void ShowTerminal()
    {
        var terminalViewModel = new TerminalViewModel();
        terminalViewModel.OnExitRequested += ShowSlotMachine;
        CurrentViewModel = terminalViewModel;
    }

    [RelayCommand]
    private void ShowSlotMachine()
    {
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