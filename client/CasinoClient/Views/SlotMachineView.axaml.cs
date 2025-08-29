using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;

using Avalonia.Markup.Xaml;

using CasinoClient.ViewModels;

namespace CasinoClient.Views;

public partial class SlotMachineView : UserControl
{
    public SlotMachineView()
    {
        InitializeComponent();
        this.KeyDown += OnKeyDown;
        this.AttachedToVisualTree += (_, __) => this.Focus();
        this.PropertyChanged += (_, e) =>
        {
            if (e.Property == IsVisibleProperty && this.IsVisible)
                this.Focus();
        };
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (DataContext is SlotMachineViewModel vm && e.Key.ToString().Length == 1)
        {
            char keyChar = e.Key.ToString().ToLower()[0];
            vm.OnKeyPressed(keyChar);
        }
    }
}