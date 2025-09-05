using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Text.Json;
using FluentAssertions;
using Moq;

namespace CasinoServer.Tests;

public class GameStateTests
{
    [Fact]
    public void GameState_InitialState_ShouldHaveCorrectDefaults()
    {
        // Arrange & Act
        var gameState = new GameState();

        // Assert
        gameState.VideoUploaded.Should().BeFalse();
        gameState.LoopStarted.Should().BeFalse();
        gameState.PasswordUpdated.Should().BeFalse();
        gameState.DiscoCompleted.Should().BeFalse();
        gameState.DiscoState.Should().BeEmpty();
    }

    [Fact]
    public void CheckDiscoCompletion_WhenBothLightsAndMusicWithinWindow_ShouldReturnTrue()
    {
        // Arrange
        var gameState = new GameState();
        var now = DateTime.UtcNow;
        gameState.DiscoState["lights"] = now;
        gameState.DiscoState["music"] = now;

        // Act
        var result = gameState.CheckDiscoCompletion();

        // Assert
        result.Should().BeTrue();
        gameState.DiscoCompleted.Should().BeTrue();
    }

    [Fact]
    public void CheckDiscoCompletion_WhenBothLightsAndMusicOutsideWindow_ShouldReturnFalse()
    {
        // Arrange
        var gameState = new GameState();
        var now = DateTime.UtcNow;
        gameState.DiscoState["lights"] = now;
        gameState.DiscoState["music"] = now.AddSeconds(10); // Long time distance

        // Act
        var result = gameState.CheckDiscoCompletion();

        // Assert
        result.Should().BeFalse();
        gameState.DiscoCompleted.Should().BeFalse();
    }

    [Fact]
    public void CheckDiscoCompletion_WhenOnlyLights_ShouldReturnFalse()
    {
        // Arrange
        var gameState = new GameState();
        gameState.DiscoState["lights"] = DateTime.UtcNow;

        // Act
        var result = gameState.CheckDiscoCompletion();

        // Assert
        result.Should().BeFalse();
        gameState.DiscoCompleted.Should().BeFalse();
    }

    [Fact]
    public void CheckDiscoCompletion_WhenOnlyMusic_ShouldReturnFalse()
    {
        // Arrange
        var gameState = new GameState();
        gameState.DiscoState["music"] = DateTime.UtcNow;

        // Act
        var result = gameState.CheckDiscoCompletion();

        // Assert
        result.Should().BeFalse();
        gameState.DiscoCompleted.Should().BeFalse();
    }
}

public class GameStatePersistenceTests
{
    private readonly string _testFilePath = "test_gamestate.json";

    [Fact]
    public async Task SaveGameStateAsync_ShouldCreateFileWithCorrectContent()
    {
        // Arrange
        var persistence = new GameStatePersistence(_testFilePath);
        var gameState = new GameState
        {
            VideoUploaded = true,
            LoopStarted = true,
            PasswordUpdated = true,
            DiscoCompleted = true
        };
        gameState.DiscoState["lights"] = DateTime.UtcNow;
        gameState.DiscoState["music"] = DateTime.UtcNow.AddSeconds(1);

        try
        {
            // Act
            await persistence.SaveGameStateAsync(gameState);

            // Assert
            File.Exists(_testFilePath).Should().BeTrue();
            var fileContent = await File.ReadAllTextAsync(_testFilePath);
            fileContent.Should().NotBeNullOrEmpty();
            fileContent.Should().Contain("videoUploaded");
            fileContent.Should().Contain("true");
        }
        finally
        {
            // Cleanup
            if (File.Exists(_testFilePath))
                File.Delete(_testFilePath);
        }
    }

    [Fact]
    public async Task LoadGameStateAsync_WhenFileDoesNotExist_ShouldReturnNewGameState()
    {
        // Arrange
        var persistence = new GameStatePersistence("nonexistent.json");

        // Act
        var gameState = await persistence.LoadGameStateAsync();

        // Assert
        gameState.Should().NotBeNull();
        gameState.VideoUploaded.Should().BeFalse();
        gameState.LoopStarted.Should().BeFalse();
        gameState.PasswordUpdated.Should().BeFalse();
        gameState.DiscoCompleted.Should().BeFalse();
    }

    [Fact]
    public async Task LoadGameStateAsync_WhenFileExists_ShouldLoadCorrectState()
    {
        // Arrange
        var persistence = new GameStatePersistence(_testFilePath);
        var originalGameState = new GameState
        {
            VideoUploaded = true,
            LoopStarted = true,
            PasswordUpdated = false,
            DiscoCompleted = true
        };
        originalGameState.DiscoState["lights"] = DateTime.UtcNow;

        try
        {
            await persistence.SaveGameStateAsync(originalGameState);

            // Act
            var loadedGameState = await persistence.LoadGameStateAsync();

            // Assert
            loadedGameState.VideoUploaded.Should().BeTrue();
            loadedGameState.LoopStarted.Should().BeTrue();
            loadedGameState.PasswordUpdated.Should().BeFalse();
            loadedGameState.DiscoCompleted.Should().BeTrue();
            loadedGameState.DiscoState.Should().ContainKey("lights");
        }
        finally
        {
            // Cleanup
            if (File.Exists(_testFilePath))
                File.Delete(_testFilePath);
        }
    }

    [Fact]
    public void DeleteSavedState_WhenFileExists_ShouldDeleteFile()
    {
        // Arrange
        var persistence = new GameStatePersistence(_testFilePath);
        File.WriteAllText(_testFilePath, "test content");

        // Act
        persistence.DeleteSavedState();

        // Assert
        File.Exists(_testFilePath).Should().BeFalse();
    }

    [Fact]
    public void DeleteSavedState_WhenFileDoesNotExist_ShouldNotThrow()
    {
        // Arrange
        var persistence = new GameStatePersistence("nonexistent.json");

        // Act & Assert
        Action act = () => persistence.DeleteSavedState();
        act.Should().NotThrow();
    }
}