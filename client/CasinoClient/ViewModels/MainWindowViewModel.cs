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

    public MainWindowViewModel()
    {
        // Set initial view model to SlotMachineViewModel
        var slotMachineViewModel = new SlotMachineViewModel();
        slotMachineViewModel.OnTerminalSwitchRequested += ShowTerminal;
        CurrentViewModel = slotMachineViewModel;
    }

    [RelayCommand]
    private void ShowTerminal()
    {
        CurrentViewModel = new TerminalViewModel();
    }
}