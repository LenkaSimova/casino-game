using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;

using CasinoClient.ViewModels;

namespace CasinoClient.Views;

/// <summary>
/// UserControl for the slot machine view. Handles keyboard input and focus management.
/// </summary>
public partial class SlotMachineView : UserControl
{
    /// <summary>
    /// Initializes the SlotMachineView, sets up event handlers for keyboard and focus.
    /// </summary>
    public SlotMachineView()
    {
        InitializeComponent();
        
        // Set up focus properties
        Focusable = true;
        
        // Listen for key presses to forward to the view model
        this.KeyDown += OnKeyDown;
        
        // Ensure control is focused when attached to the visual tree
        this.AttachedToVisualTree += (_, __) => this.Focus();
        
        // Refocus when visibility changes
        this.PropertyChanged += (_, e) =>
        {
            if (e.Property == IsVisibleProperty && this.IsVisible)
                this.Focus();
        };
        
        // Listen for focus changes within the control tree
        this.GotFocus += OnGotFocus;
        this.LostFocus += OnLostFocus;
        
        // Handle pointer events to maintain focus
        this.PointerPressed += OnPointerPressed;
        
        Focus();
    }

    /// <summary>
    /// Handles key down events and passes single-character keys to the view model.
    /// </summary>
    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (DataContext is SlotMachineViewModel vm && e.Key.ToString().Length == 1)
        {
            char keyChar = e.Key.ToString().ToLower()[0];
            vm.OnKeyPressed(keyChar);
        }
    }

    /// <summary>
    /// Handles when the control gains focus.
    /// </summary>
    private void OnGotFocus(object? sender, GotFocusEventArgs e)
    {
        // Control has gained focus, no additional action needed
    }

    /// <summary>
    /// Handles when the control loses focus and attempts to regain it.
    /// </summary>
    private void OnLostFocus(object? sender, RoutedEventArgs e)
    {
        // If we lost focus to a child control, try to regain focus
        if (e.Source != this)
        {
            // Use a small delay to allow the current event to complete
            Dispatcher.UIThread.Post(() => this.Focus(), DispatcherPriority.Background);
        }
    }

    /// <summary>
    /// Handles pointer pressed events to maintain focus.
    /// </summary>
    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        // Ensure we maintain focus when clicked
        this.Focus();
    }
}