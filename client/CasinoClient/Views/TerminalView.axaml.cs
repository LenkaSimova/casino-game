using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using CasinoClient.ViewModels;

namespace CasinoClient.Views;

public partial class TerminalView : UserControl
{
    private TextBox? _commandInput;
    private ScrollViewer? _terminalScroller;

    public TerminalView()
    {
        InitializeComponent();
        Loaded += OnLoaded;
        KeyDown += OnKeyDown;
        Focusable = true;
    }

    private void OnLoaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        // Find controls
        _commandInput = this.FindControl<TextBox>("CommandInput");
        _terminalScroller = this.FindControl<ScrollViewer>("TerminalScroller");

        // Focus the input
        Focus();
        _commandInput?.Focus();

        // Subscribe to terminal lines changes to auto-scroll
        if (DataContext is TerminalViewModel viewModel)
        {
            viewModel.TerminalLines.CollectionChanged += (s, e) =>
            {
                Dispatcher.UIThread.InvokeAsync(() =>
                {
                    ScrollToBottom();
                });
            };
        }
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (DataContext is not TerminalViewModel viewModel) return;

        switch (e.Key)
        {
            case Key.Enter:
                e.Handled = true;       // Prevent default enter key behavior
                viewModel.ExecuteCommandCommand.Execute(null);
                break;
            case Key.Up:
                e.Handled = true;
                viewModel.OnKeyDown("Up");
                break;
            case Key.Down:
                e.Handled = true;
                viewModel.OnKeyDown("Down");
                break;
        }
    }

    private void ScrollToBottom()
    {
        _terminalScroller?.ScrollToEnd();
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        // Keep focus on the command input
        base.OnPointerPressed(e);
        Focus();
        _commandInput?.Focus();
    }
}