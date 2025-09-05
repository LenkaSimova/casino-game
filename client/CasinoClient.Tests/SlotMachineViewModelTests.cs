using FluentAssertions;
using CasinoClient.ViewModels;
using Avalonia.Headless.XUnit;

namespace CasinoClient.Tests;

public class SlotMachineViewModelTests
{
    [AvaloniaFact]
    public void SlotMachineViewModel_Initialize_ShouldHaveCorrectDefaults()
    {
        // Arrange & Act
        var viewModel = new SlotMachineViewModel();

        // Assert
        viewModel.BetAmount.Should().Be(1);
        viewModel.LastWin.Should().Be(0);
        viewModel.CanSpin.Should().BeTrue();
        viewModel.IsSpinning.Should().BeFalse();
        viewModel.Reel1Symbol.Should().NotBeNull();
        viewModel.Reel2Symbol.Should().NotBeNull();
        viewModel.Reel3Symbol.Should().NotBeNull();
    }

    [AvaloniaFact]
    public void SlotMachineViewModel_BetAmountChanged_ShouldUpdateCanSpin()
    {
        // Arrange
        var viewModel = new SlotMachineViewModel();
        viewModel.Credits = 5;

        // Act
        viewModel.BetAmount = 10; // More than available credits

        // Assert
        viewModel.CanSpin.Should().BeFalse();
    }

    [AvaloniaFact]
    public void SlotMachineViewModel_CreditsChanged_ShouldUpdateCanSpin()
    {
        // Arrange
        var viewModel = new SlotMachineViewModel();
        viewModel.BetAmount = 5;

        // Act
        viewModel.Credits = 3; // Less than bet amount

        // Assert
        viewModel.CanSpin.Should().BeFalse();
    }

    [AvaloniaFact]
    public void SlotMachineViewModel_IsSpinningChanged_ShouldUpdateCanSpin()
    {
        // Arrange
        var viewModel = new SlotMachineViewModel();

        // Act
        viewModel.IsSpinning = true;

        // Assert
        viewModel.CanSpin.Should().BeFalse();
    }

    [AvaloniaFact]
    public void SlotMachineViewModel_OnKeyPressed_WhenSpinning_ShouldNotRecordKey()
    {
        // Arrange
        var viewModel = new SlotMachineViewModel();
        viewModel.IsSpinning = true;
        bool eventTriggered = false;
        viewModel.OnTerminalSwitchRequested += () => eventTriggered = true;

        // Act
        viewModel.OnKeyPressed('s');
        viewModel.OnKeyPressed('o');
        viewModel.OnKeyPressed('s');

        // Assert
        eventTriggered.Should().BeFalse();
    }

    [AvaloniaFact]
    public void SlotMachineViewModel_OnKeyPressed_WithCorrectSequence_ShouldTriggerEvent()
    {
        // Arrange
        var viewModel = new SlotMachineViewModel();
        viewModel.IsSpinning = false;
        bool eventTriggered = false;
        viewModel.OnTerminalSwitchRequested += () => eventTriggered = true;

        // Act - Press keys that match initial symbol configuration (star, star, star = s,s,s)
        viewModel.OnKeyPressed('s');
        viewModel.OnKeyPressed('s');
        viewModel.OnKeyPressed('s');

        // Assert
        eventTriggered.Should().BeTrue();
    }

    [AvaloniaFact]
    public void SlotMachineViewModel_OnKeyPressed_WithIncorrectSequence_ShouldNotTriggerEvent()
    {
        // Arrange
        var viewModel = new SlotMachineViewModel();
        viewModel.IsSpinning = false;
        bool eventTriggered = false;
        viewModel.OnTerminalSwitchRequested += () => eventTriggered = true;

        // Act
        viewModel.OnKeyPressed('-');
        viewModel.OnKeyPressed('-');
        viewModel.OnKeyPressed('-');

        // Assert
        eventTriggered.Should().BeFalse();
    }

    [AvaloniaFact]
    public void SlotMachineViewModel_OnKeyPressed_CaseInsensitive_ShouldWork()
    {
        // Arrange
        var viewModel = new SlotMachineViewModel();
        viewModel.IsSpinning = false;
        bool eventTriggered = false;
        viewModel.OnTerminalSwitchRequested += () => eventTriggered = true;

        // Act - Mix of upper and lower case
        viewModel.OnKeyPressed('S');
        viewModel.OnKeyPressed('s');
        viewModel.OnKeyPressed('S');

        // Assert
        eventTriggered.Should().BeTrue();
    }

    [AvaloniaFact]
    public void SlotMachineViewModel_OnKeyPressed_OnlyLastThreeKeysCount()
    {
        // Arrange
        var viewModel = new SlotMachineViewModel();
        viewModel.IsSpinning = false;
        bool eventTriggered = false;
        viewModel.OnTerminalSwitchRequested += () => eventTriggered = true;

        // Act - Press more than 3 keys, but last 3 should match
        viewModel.OnKeyPressed('a');
        viewModel.OnKeyPressed('b');
        viewModel.OnKeyPressed('s');
        viewModel.OnKeyPressed('s');
        viewModel.OnKeyPressed('s');

        // Assert
        eventTriggered.Should().BeTrue();
    }


    [AvaloniaFact]
    public async Task SlotMachineViewModel_SpinCommand_WhenEnoughCredits_ShouldDeductBet()
    {
        // Arrange
        var viewModel = new SlotMachineViewModel();
        var initialCredits = viewModel.Credits;
        var betAmount = viewModel.BetAmount;

        // Act
        await viewModel.SpinCommand.ExecuteAsync(null);

        // Assert
        // Credits should be deducted by bet amount (possibly adjusted by winnings)
        viewModel.Credits.Should().BeLessOrEqualTo(initialCredits - betAmount + viewModel.LastWin);
    }

    [AvaloniaFact]
    public void SlotMachineViewModel_SpinCommand_CanExecute_ShouldRespectCanSpin()
    {
        // Arrange
        var viewModel = new SlotMachineViewModel();

        // Act & Assert - With sufficient credits
        viewModel.Credits = 10;
        viewModel.BetAmount = 1;
        viewModel.IsSpinning = false;
        viewModel.SpinCommand.CanExecute(null).Should().BeTrue();

        // Act & Assert - With insufficient credits
        viewModel.Credits = 0;
        viewModel.SpinCommand.CanExecute(null).Should().BeFalse();

        // Act & Assert - While spinning
        viewModel.Credits = 10;
        viewModel.IsSpinning = true;
        viewModel.SpinCommand.CanExecute(null).Should().BeFalse();
    }
}

public class SlotMachineViewModelIntegrationTests
{
    [AvaloniaFact]
    public async Task SlotMachineViewModel_FullSpinCycle_ShouldCompleteSuccessfully()
    {
        // Arrange
        var viewModel = new SlotMachineViewModel();
        var initialCredits = viewModel.Credits;
        var betAmount = viewModel.BetAmount;

        // Act
        await viewModel.SpinCommand.ExecuteAsync(null);

        // Wait a bit longer than the spin animation
        await Task.Delay(SlotMachineViewModel.SpinDuration + 100);

        // Assert
        viewModel.IsSpinning.Should().BeFalse();
        viewModel.Credits.Should().BeLessOrEqualTo(initialCredits - viewModel.BetAmount + viewModel.LastWin); // Credits should be affected by bet
        viewModel.LastWin.Should().BeGreaterOrEqualTo(0); // Win can be 0 or positive

        // Symbols should be loaded (not null)
        viewModel.Reel1Symbol.Should().NotBeNull();
        viewModel.Reel2Symbol.Should().NotBeNull();
        viewModel.Reel3Symbol.Should().NotBeNull();
    }
}
