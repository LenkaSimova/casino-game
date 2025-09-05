using FluentAssertions;
using CasinoClient.Services.LLMHandlers;
using Moq;

namespace CasinoClient.Tests;

public class LocalLLMHandlerTests
{
    [Fact]
    public void LocalLLMHandler_Constructor_ShouldInitialize()
    {
        // Arrange & Act
        var handler = new LocalLLMHandler();

        // Assert
        handler.Should().NotBeNull();
    }

    [Fact]
    public void LocalLLMHandler_Constructor_WithCustomParameters_ShouldInitialize()
    {
        // Arrange & Act
        var handler = new LocalLLMHandler("http://custom:11434/", "custom-model");

        // Assert
        handler.Should().NotBeNull();
    }

    [Fact]
    public void LocalLLMHandler_AddSystemMessage_ShouldNotThrow()
    {
        // Arrange
        var handler = new LocalLLMHandler();

        // Act & Assert
        Action act = () => handler.AddSystemMessage("Test system message");
        act.Should().NotThrow();
    }

    [Fact]
    public void LocalLLMHandler_ClearHistory_ShouldNotThrow()
    {
        // Arrange
        var handler = new LocalLLMHandler();

        // Act & Assert
        Action act = () => handler.ClearHistory();
        act.Should().NotThrow();
    }

    [Fact]
    public async Task LocalLLMHandler_SendMessageAsync_WithEmptyMessage_ShouldReturnEmpty()
    {
        // Arrange
        var handler = new LocalLLMHandler();

        // Act
        var result = await handler.SendMessageAsync("");

        // Assert
        result.Should().Be(string.Empty);
    }

    [Fact]
    public async Task LocalLLMHandler_SendMessageAsync_WithNullMessage_ShouldReturnEmpty()
    {
        // Arrange
        var handler = new LocalLLMHandler();

        // Act
        var result = await handler.SendMessageAsync(null!);

        // Assert
        result.Should().Be(string.Empty);
    }

    [Fact]
    public async Task LocalLLMHandler_SendMessageAsync_WithWhitespaceMessage_ShouldReturnEmpty()
    {
        // Arrange
        var handler = new LocalLLMHandler();

        // Act
        var result = await handler.SendMessageAsync("   ");

        // Assert
        result.Should().Be(string.Empty);
    }


    [Fact]
    public void LocalLLMHandler_MultipleSystemMessages_ShouldNotThrow()
    {
        // Arrange
        var handler = new LocalLLMHandler();

        // Act & Assert
        Action act = () =>
        {
            handler.AddSystemMessage("System message 1");
            handler.AddSystemMessage("System message 2");
            handler.AddSystemMessage("System message 3");
        };
        act.Should().NotThrow();
    }

    [Fact]
    public void LocalLLMHandler_ClearHistoryAfterAddingMessages_ShouldNotThrow()
    {
        // Arrange
        var handler = new LocalLLMHandler();
        handler.AddSystemMessage("Test message");

        // Act & Assert
        Action act = () => handler.ClearHistory();
        act.Should().NotThrow();
    }
}