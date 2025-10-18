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
        if (DataContext is SlotMachineViewModel vm)
        {
            char? keyChar = GetCharFromKey(e.Key);
            if (keyChar.HasValue)
            {
                vm.OnKeyPressed(keyChar.Value);
            }
        }
    }

    /// <summary>
    /// Converts a Key enum value to its corresponding character.
    /// </summary>
    private char? GetCharFromKey(Key key)
    {
        // Handle letter keys (A-Z)
        if (key >= Key.A && key <= Key.Z)
        {
            return (char)('a' + (key - Key.A));
        }

        // Handle number keys (0-9) from main keyboard
        if (key >= Key.D0 && key <= Key.D9)
        {
            return (char)('0' + (key - Key.D0));
        }

        // Handle number keys from numpad
        if (key >= Key.NumPad0 && key <= Key.NumPad9)
        {
            return (char)('0' + (key - Key.NumPad0));
        }

        // Handle special characters if needed
        // You can add more cases here for other special characters

        return null;
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