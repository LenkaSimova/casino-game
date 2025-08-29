using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using System.Threading.Tasks;

namespace CasinoClient.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly Random _random = new();
    // Load bitmap images once at startup
    private readonly Bitmap[] _symbolBitmaps;

    private readonly Dictionary<string, int> _payouts = new()
    {
        { "diamond_diamond_diamond", 100 },
        { "star_star_star", 50 },
        { "bell_bell_bell", 30 },
        { "cherry_cherry_cherry", 20 },
        { "grape_grape_grape", 15 },
        { "lemon_lemon_lemon", 10 },
        { "orange_orange_orange", 8 },
        { "apple_apple_apple", 5 },
        { "cherry_cherry", 3 },
        { "cherry", 1 }
    };

    // Symbol names for tracking wins
    private readonly string[] _symbolNames = { "apple", "orange", "lemon", "grape", "cherry", "star", "diamond", "bell" };

    public MainWindowViewModel()
    {
        // Load all symbol bitmaps
        _symbolBitmaps = new Bitmap[8];
        _symbolBitmaps[0] = LoadBitmap("apple.jpg");
        _symbolBitmaps[1] = LoadBitmap("orange.jpg");
        _symbolBitmaps[2] = LoadBitmap("lemon.jpg");
        _symbolBitmaps[3] = LoadBitmap("grape.jpg");
        _symbolBitmaps[4] = LoadBitmap("cherry.jpg");
        _symbolBitmaps[5] = LoadBitmap("star.png");
        _symbolBitmaps[6] = LoadBitmap("diamond.jpg");
        _symbolBitmaps[7] = LoadBitmap("bell.png");

        // Initialize reel symbols
        _reel1Symbol = _symbolBitmaps[5];
        _reel2Symbol = _symbolBitmaps[5];
        _reel3Symbol = _symbolBitmaps[5];
    }

    private Bitmap LoadBitmap(string fileName)
    {
        var uri = new Uri($"avares://CasinoClient/Assets/{fileName}");
        return new Bitmap(AssetLoader.Open(uri));
    }

    [ObservableProperty]
    private int _credits = 10;

    [ObservableProperty]
    private int _betAmount = 1;

    [ObservableProperty]
    private Bitmap _reel1Symbol;

    [ObservableProperty]
    private Bitmap _reel2Symbol;

    [ObservableProperty]
    private Bitmap _reel3Symbol;

    // Track current symbol indices for win checking
    private int _reel1Index;
    private int _reel2Index;
    private int _reel3Index;

    [ObservableProperty]
    private int _lastWin = 0;

    [ObservableProperty]
    private bool _canSpin = true;

    [ObservableProperty]
    private bool _isSpinning = false;

    private int _spinSpeed = 50; // milliseconds between symbol changes

    [RelayCommand(CanExecute = nameof(CanSpin))]
    private async Task SpinAsync()
    {
        if (Credits < BetAmount)
        {
            return;
        }

        // Deduct bet amount
        Credits -= BetAmount;
        LastWin = 0;
        IsSpinning = true;

        // Generate final results
        _reel1Index = _random.Next(_symbolBitmaps.Length);
        _reel2Index = _random.Next(_symbolBitmaps.Length);
        _reel3Index = _random.Next(_symbolBitmaps.Length);

        await AnimateSpinning();

        // Set final symbols
        Reel1Symbol = _symbolBitmaps[_reel1Index];
        Reel2Symbol = _symbolBitmaps[_reel2Index];
        Reel3Symbol = _symbolBitmaps[_reel3Index];

        // Check for wins
        CheckForWins();

        IsSpinning = false;
        // Update CanSpin based on remaining credits
        CanSpin = Credits >= BetAmount;
    }

    private async Task AnimateSpinning()
    {
        int spinDuration = 2000; // Total spin time in milliseconds
        int elapsed = 0;
        int currentSpinSpeed = _spinSpeed;

        while (elapsed < spinDuration)
        {
            // Show random symbols during spinning
            Reel1Symbol = _symbolBitmaps[_random.Next(_symbolBitmaps.Length)];
            Reel2Symbol = _symbolBitmaps[_random.Next(_symbolBitmaps.Length)];
            Reel3Symbol = _symbolBitmaps[_random.Next(_symbolBitmaps.Length)];

            await Task.Delay(currentSpinSpeed);
            elapsed += currentSpinSpeed;

            // Gradually slow down the spinning
            if (elapsed > spinDuration * 0.7)
            {
                currentSpinSpeed = Math.Min(currentSpinSpeed + 20, 200);
            }
        }
    }

    private void CheckForWins()
    {
        string symbol1 = _symbolNames[_reel1Index];
        string symbol2 = _symbolNames[_reel2Index];
        string symbol3 = _symbolNames[_reel3Index];

        string combination = $"{symbol1}_{symbol2}_{symbol3}";

        // Check for exact three-symbol matches
        if (_payouts.ContainsKey(combination))
        {
            int payout = _payouts[combination] * BetAmount;
            Credits += payout;
            LastWin = payout;
            return;
        }

        // Check for two cherries
        if ((symbol1 == "cherry" && symbol2 == "cherry") ||
            (symbol2 == "cherry" && symbol3 == "cherry") ||
            (symbol1 == "cherry" && symbol3 == "cherry"))
        {
            int payout = _payouts["cherry_cherry"] * BetAmount;
            Credits += payout;
            LastWin = payout;
            return;
        }

        // Check for single cherry
        if (symbol1 == "cherry" || symbol2 == "cherry" || symbol3 == "cherry")
        {
            int payout = _payouts["cherry"] * BetAmount;
            Credits += payout;
            LastWin = payout;
        }
    }

    partial void OnBetAmountChanged(int value)
    {
        CanSpin = Credits >= value && !IsSpinning;
        SpinCommand.NotifyCanExecuteChanged();
    }

    partial void OnCreditsChanged(int value)
    {
        CanSpin = value >= BetAmount && !IsSpinning;
        SpinCommand.NotifyCanExecuteChanged();
    }

    partial void OnIsSpinningChanged(bool value)
    {
        CanSpin = Credits >= BetAmount && !value;
        SpinCommand.NotifyCanExecuteChanged();
    }
}