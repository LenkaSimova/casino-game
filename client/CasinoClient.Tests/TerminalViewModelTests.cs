using FluentAssertions;
using CasinoClient.ViewModels;
using CasinoClient.Models;
using Moq;
using System.Collections.ObjectModel;

namespace CasinoClient.Tests;

public class TerminalViewModelTests
{
    [Fact]
    public void TerminalViewModel_Initialize_ShouldHaveCorrectDefaults()
    {
        // Arrange & Act
        var viewModel = new TerminalViewModel();

        // Assert
        viewModel.CurrentInput.Should().Be("");
        viewModel.TerminalLines.Should().NotBeNull();
        viewModel.TerminalLines.Should().HaveCountGreaterThan(0); // Should have welcome messages
        viewModel.IsInputFocused.Should().BeTrue();
        viewModel.CurrentState.Should().Be(TerminalState.Normal);
        viewModel.Prompt.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task TerminalViewModel_ExecuteCommand_WithEmptyInput_ShouldShowPrompt()
    {
        // Arrange
        var viewModel = new TerminalViewModel();
        var initialLineCount = viewModel.TerminalLines.Count;
        viewModel.CurrentInput = "";

        // Act
        await viewModel.ExecuteCommandCommand.ExecuteAsync(null);

        // Assert
        viewModel.TerminalLines.Should().HaveCount(initialLineCount + 1);
        viewModel.CurrentInput.Should().Be("");
    }

    [Fact]
    public async Task TerminalViewModel_ExecuteCommand_WithHelpCommand_ShouldShowHelp()
    {
        // Arrange
        var viewModel = new TerminalViewModel();
        viewModel.CurrentInput = "help";

        // Act
        await viewModel.ExecuteCommandCommand.ExecuteAsync(null);

        // Assert
        viewModel.TerminalLines.Should().Contain(line =>
            line.Text.Contains("Available commands"));
        viewModel.CurrentInput.Should().Be("");
    }

    [Fact]
    public async Task TerminalViewModel_ExecuteCommand_WithClearCommand_ShouldClearLines()
    {
        // Arrange
        var viewModel = new TerminalViewModel();
        viewModel.CurrentInput = "clear";

        // Act
        await viewModel.ExecuteCommandCommand.ExecuteAsync(null);

        // Assert
        viewModel.TerminalLines.Should().BeEmpty();
        viewModel.CurrentInput.Should().Be("");
    }

    [Fact]
    public async Task TerminalViewModel_ExecuteCommand_WithEchoCommand_ShouldEchoText()
    {
        // Arrange
        var viewModel = new TerminalViewModel();
        viewModel.CurrentInput = "echo Hello World";

        // Act
        await viewModel.ExecuteCommandCommand.ExecuteAsync(null);

        // Assert
        viewModel.TerminalLines.Should().Contain(line =>
            line.Text.Contains("Hello World") && line.Type == TerminalLineType.Normal);
    }

    [Fact]
    public async Task TerminalViewModel_ExecuteCommand_WithExitCommand_ShouldTriggerExitEvent()
    {
        // Arrange
        var viewModel = new TerminalViewModel();
        bool exitTriggered = false;
        viewModel.OnExitRequested += () => exitTriggered = true;
        viewModel.CurrentInput = "exit";

        // Act
        await viewModel.ExecuteCommandCommand.ExecuteAsync(null);

        // Assert
        exitTriggered.Should().BeTrue();
    }

    [Fact]
    public async Task TerminalViewModel_ExecuteCommand_WithUnknownCommand_ShouldShowError()
    {
        // Arrange
        var viewModel = new TerminalViewModel();
        viewModel.CurrentInput = "unknowncommand";

        // Act
        await viewModel.ExecuteCommandCommand.ExecuteAsync(null);

        // Assert
        viewModel.TerminalLines.Should().Contain(line =>
            line.Type == TerminalLineType.Error);
    }

    [Fact]
    public async Task TerminalViewModel_OnKeyDown_WithUpArrow_StopWhenOnFirstCommand()
    {
        // Arrange
        var viewModel = new TerminalViewModel();

        // Execute a command first to add it to history
        viewModel.CurrentInput = "test command";
        await viewModel.ExecuteCommandCommand.ExecuteAsync(null);

        // Act
        viewModel.OnKeyDown("Up");

        // Assert
        viewModel.CurrentInput.Should().Be("test command");
    }

    [Fact]
    public async Task TerminalViewModel_OnKeyDown_WithDownArrow_ShouldNavigateHistory()
    {
        // Arrange
        var viewModel = new TerminalViewModel();

        // Execute commands to add them to history
        viewModel.CurrentInput = "command1";
        await viewModel.ExecuteCommandCommand.ExecuteAsync(null);
        viewModel.CurrentInput = "command2";
        await viewModel.ExecuteCommandCommand.ExecuteAsync(null);

        // Navigate up twice to get to first command
        viewModel.OnKeyDown("Up");
        viewModel.OnKeyDown("Up");

        // Act
        viewModel.OnKeyDown("Down");

        // Assert
        viewModel.CurrentInput.Should().Be("command2");
    }

    [Fact]
    public void TerminalViewModel_OnKeyDown_InLLMMode_ShouldNotNavigateHistory()
    {
        // Arrange
        var viewModel = new TerminalViewModel();
        viewModel.CurrentState = TerminalState.LLMConversation;
        var originalInput = viewModel.CurrentInput;

        // Act
        viewModel.OnKeyDown("Up");

        // Assert
        viewModel.CurrentInput.Should().Be(originalInput);
    }
}

public class TerminalViewModelLLMTests
{
    [Fact]
    public async Task TerminalViewModel_HandleLLMConversationCommand_ShouldEnterLLMMode()
    {
        // Arrange
        var viewModel = new TerminalViewModel();
        viewModel.CurrentInput = "llm";

        // Act
        await viewModel.ExecuteCommandCommand.ExecuteAsync(null);

        // Assert
        viewModel.CurrentState.Should().Be(TerminalState.LLMConversation);
    }

    [Fact]
    public async Task TerminalViewModel_LLMMode_WithExitCommand_ShouldExitLLMMode()
    {
        // Arrange
        var viewModel = new TerminalViewModel();
        // Enter LLM mode first
        viewModel.CurrentInput = "llm";
        await viewModel.ExecuteCommandCommand.ExecuteAsync(null);

        // Act
        viewModel.CurrentInput = "exit";
        await viewModel.ExecuteCommandCommand.ExecuteAsync(null);

        // Assert
        viewModel.CurrentState.Should().Be(TerminalState.Normal);
    }

    [Fact]
    public async Task TerminalViewModel_LLMMode_WithQuitCommand_ShouldExitLLMMode()
    {
        // Arrange
        var viewModel = new TerminalViewModel();
        // Enter LLM mode first
        viewModel.CurrentInput = "llm";
        await viewModel.ExecuteCommandCommand.ExecuteAsync(null);

        // Act
        viewModel.CurrentInput = "quit";
        await viewModel.ExecuteCommandCommand.ExecuteAsync(null);

        // Assert
        viewModel.CurrentState.Should().Be(TerminalState.Normal);
    }
}
