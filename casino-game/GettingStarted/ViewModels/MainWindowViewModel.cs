using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace GettingStarted.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly Random _random = new();
    private readonly string[] _symbols = { "🍎", "🍊", "🍋", "🍇", "🍒", "⭐", "💎", "🔔" };
    private readonly Dictionary<string, int> _payouts = new()
    {
        { "💎💎💎", 100 },  // Triple diamonds - jackpot
        { "⭐⭐⭐", 50 },   // Triple stars
        { "🔔🔔🔔", 30 },   // Triple bells
        { "🍒🍒🍒", 20 },   // Triple cherries
        { "🍇🍇🍇", 15 },   // Triple grapes
        { "🍋🍋🍋", 10 },   // Triple lemons
        { "🍊🍊🍊", 8 },    // Triple oranges
        { "🍎🍎🍎", 5 },    // Triple apples
        { "🍒🍒", 3 },      // Two cherries
        { "🍒", 1 }         // One cherry
    };

    [ObservableProperty]
    private int _credits = 10;

    [ObservableProperty]
    private int _betAmount = 1;

    [ObservableProperty]
    private string _reel1Symbol = "🎰";

    [ObservableProperty]
    private string _reel2Symbol = "🎰";

    [ObservableProperty]
    private string _reel3Symbol = "🎰";

    [ObservableProperty]
    private int _lastWin = 0;

    [ObservableProperty]
    private bool _canSpin = true;

    [RelayCommand(CanExecute = nameof(CanSpin))]
    private void Spin()
    {
        if (Credits < BetAmount)
        {
            return;
        }

        // Deduct bet amount
        Credits -= BetAmount;
        LastWin = 0;

        // Spin the reels
        Reel1Symbol = _symbols[_random.Next(_symbols.Length)];
        Reel2Symbol = _symbols[_random.Next(_symbols.Length)];
        Reel3Symbol = _symbols[_random.Next(_symbols.Length)];

        // Check for wins
        CheckForWins();

        // Update CanSpin based on remaining credits
        CanSpin = Credits >= BetAmount;
    }

    private void CheckForWins()
    {
        string combination = Reel1Symbol + Reel2Symbol + Reel3Symbol;

        // Check for exact three-symbol matches
        if (_payouts.ContainsKey(combination))
        {
            int payout = _payouts[combination] * BetAmount;
            Credits += payout;
            LastWin = payout;
            return;
        }

        // Check for two cherries
        if ((Reel1Symbol == "🍒" && Reel2Symbol == "🍒") ||
            (Reel2Symbol == "🍒" && Reel3Symbol == "🍒") ||
            (Reel1Symbol == "🍒" && Reel3Symbol == "🍒"))
        {
            int payout = _payouts["🍒🍒"] * BetAmount;
            Credits += payout;
            LastWin = payout;
            return;
        }

        // Check for single cherry
        if (Reel1Symbol == "🍒" || Reel2Symbol == "🍒" || Reel3Symbol == "🍒")
        {
            int payout = _payouts["🍒"] * BetAmount;
            Credits += payout;
            LastWin = payout;
        }
    }

    partial void OnBetAmountChanged(int value)
    {
        CanSpin = Credits >= value;
        SpinCommand.NotifyCanExecuteChanged();
    }

    partial void OnCreditsChanged(int value)
    {
        CanSpin = value >= BetAmount;
        SpinCommand.NotifyCanExecuteChanged();
    }
}