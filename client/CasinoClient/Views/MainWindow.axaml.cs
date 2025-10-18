using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using System;
using System.ComponentModel;
using System.Diagnostics;

namespace CasinoClient.Views;

public partial class MainWindow : Window
{
    private bool _allowClose = false;

    public MainWindow()
    {
        InitializeComponent();

        // Force fullscreen mode
        WindowState = WindowState.FullScreen;
        SystemDecorations = SystemDecorations.None;
        Topmost = true;
        CanResize = false;

        // Block keyboard shortcuts for exiting
        this.KeyDown += OnKeyDown;

        // Prevent window closing
        this.Closing += OnClosing;

        // Handle window state changes to prevent exiting fullscreen
        this.PropertyChanged += OnPropertyChanged;
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        // Block common exit shortcuts
        // Alt+F4 (Windows/Linux)
        if (e.Key == Key.F4 && e.KeyModifiers.HasFlag(KeyModifiers.Alt))
        {
            e.Handled = true;
            return;
        }

        // Cmd+Q (macOS)
        if (e.Key == Key.Q && e.KeyModifiers.HasFlag(KeyModifiers.Meta))
        {
            e.Handled = true;
            return;
        }

        // Alt+Tab (Windows/Linux)
        if (e.Key == Key.Tab && e.KeyModifiers.HasFlag(KeyModifiers.Alt))
        {
            e.Handled = true;
            return;
        }

        // Cmd+Tab (macOS)
        if (e.Key == Key.Tab && e.KeyModifiers.HasFlag(KeyModifiers.Meta))
        {
            e.Handled = true;
            return;
        }

        // F11 (toggle fullscreen)
        if (e.Key == Key.F11)
        {
            e.Handled = true;
            return;
        }

        // Escape key
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            return;
        }

        // Windows key
        if (e.Key == Key.LWin || e.Key == Key.RWin)
        {
            e.Handled = true;
            return;
        }

        // Ctrl+Alt+Delete combo (partial blocking on Windows)
        if (e.KeyModifiers.HasFlag(KeyModifiers.Control) &&
            e.KeyModifiers.HasFlag(KeyModifiers.Alt) &&
            e.Key == Key.Delete)
        {
            e.Handled = true;
            return;
        }
    }

    private void OnClosing(object? sender, WindowClosingEventArgs e)
    {
        // Prevent closing unless explicitly allowed
        if (!_allowClose)
        {
            e.Cancel = true;
        }
    }

    private void OnPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        // Force window to stay in fullscreen if user tries to change it
        if (e.Property.Name == nameof(WindowState))
        {
            if (WindowState != WindowState.FullScreen)
            {
                WindowState = WindowState.FullScreen;
            }
        }
    }

    /// <summary>
    /// Call this method to allow the application to close (e.g., from admin interface)
    /// </summary>
    public void AllowClose()
    {
        _allowClose = true;
    }

}