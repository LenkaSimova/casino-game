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

    /// <summary>
    /// Initializes the view and sets up event handlers.
    /// </summary>
    public TerminalView()
    {
        InitializeComponent();
        Loaded += OnLoaded;
        KeyDown += OnKeyDown;
        Focusable = true;
    }

    /// <summary>
    /// Called when the view is loaded; finds controls and sets up auto-scroll and focus.
    /// </summary>
    private void OnLoaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        // Find controls (input field and terminal output)
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

    /// <summary>
    /// Handles key events for command input and history navigation.
    /// </summary>
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

    /// <summary>
    /// Scrolls the terminal output to the bottom.
    /// </summary>
    private void ScrollToBottom()
    {
        _terminalScroller?.ScrollToEnd();
    }

    /// <summary>
    /// Ensures the command input stays focused when the view is clicked.
    /// </summary>
    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        // Keep focus on the command input
        base.OnPointerPressed(e);
        Focus();
        _commandInput?.Focus();
    }
}