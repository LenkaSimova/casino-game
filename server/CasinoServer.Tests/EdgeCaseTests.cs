using FluentAssertions;
using System.Text.Json;

namespace CasinoServer.Tests;

public class EdgeCaseTests
{
    [Fact]
    public void GameState_CheckDiscoCompletion_WithNegativeTimeDifference_ShouldWork()
    {
        // Arrange
        var gameState = new GameState();
        var now = DateTime.UtcNow;
        gameState.DiscoState["lights"] = now.AddSeconds(2);
        gameState.DiscoState["music"] = now; // Music comes before lights

        // Act
        var result = gameState.CheckDiscoCompletion();

        // Assert
        result.Should().BeTrue();
        gameState.DiscoCompleted.Should().BeTrue();
    }

    [Fact]
    public async Task GameStatePersistence_WithInvalidJsonFile_ShouldReturnNewGameState()
    {
        // Arrange
        var testFilePath = "invalid_test.json";
        await File.WriteAllTextAsync(testFilePath, "{ invalid json content }");
        var persistence = new GameStatePersistence(testFilePath);

        try
        {
            // Act
            var gameState = await persistence.LoadGameStateAsync();

            // Assert
            gameState.Should().NotBeNull();
            gameState.VideoUploaded.Should().BeFalse(); // Should return default state
        }
        finally
        {
            // Cleanup
            if (File.Exists(testFilePath))
                File.Delete(testFilePath);
        }
    }

    [Fact]
    public async Task GameStatePersistence_WithEmptyFile_ShouldReturnNewGameState()
    {
        // Arrange
        var testFilePath = "empty_test.json";
        await File.WriteAllTextAsync(testFilePath, "");
        var persistence = new GameStatePersistence(testFilePath);

        try
        {
            // Act
            var gameState = await persistence.LoadGameStateAsync();

            // Assert
            gameState.Should().NotBeNull();
            gameState.VideoUploaded.Should().BeFalse(); // Should return default state
        }
        finally
        {
            // Cleanup
            if (File.Exists(testFilePath))
                File.Delete(testFilePath);
        }
    }

    [Fact]
    public async Task GameStatePersistence_WithNullGameState_ShouldHandleGracefully()
    {
        // Arrange
        var testFilePath = "null_test.json";
        await File.WriteAllTextAsync(testFilePath, "null");
        var persistence = new GameStatePersistence(testFilePath);

        try
        {
            // Act
            var gameState = await persistence.LoadGameStateAsync();

            // Assert
            gameState.Should().NotBeNull();
            gameState.VideoUploaded.Should().BeFalse(); // Should return default state
        }
        finally
        {
            // Cleanup
            if (File.Exists(testFilePath))
                File.Delete(testFilePath);
        }
    }

    [Fact]
    public void SerializableGameState_ShouldSerializeAndDeserializeCorrectly()
    {
        // Arrange
        var originalState = new SerializableGameState
        {
            VideoUploaded = true,
            LoopStarted = false,
            PasswordUpdated = true,
            DiscoState = new Dictionary<string, DateTime>
            {
                { "lights", DateTime.UtcNow },
                { "music", DateTime.UtcNow.AddSeconds(2) }
            },
            DiscoWindow = TimeSpan.FromSeconds(10),
            DiscoCompleted = true,
            SavedAt = DateTime.UtcNow
        };

        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        // Act
        var json = JsonSerializer.Serialize(originalState, options);
        var deserializedState = JsonSerializer.Deserialize<SerializableGameState>(json, options);

        // Assert
        deserializedState.Should().NotBeNull();
        deserializedState!.VideoUploaded.Should().Be(originalState.VideoUploaded);
        deserializedState.LoopStarted.Should().Be(originalState.LoopStarted);
        deserializedState.PasswordUpdated.Should().Be(originalState.PasswordUpdated);
        deserializedState.DiscoCompleted.Should().Be(originalState.DiscoCompleted);
        deserializedState.DiscoState.Should().HaveCount(2);
        deserializedState.DiscoWindow.Should().Be(originalState.DiscoWindow);
    }

    [Fact]
    public void GameState_MultipleDiscoCompletionChecks_ShouldMaintainState()
    {
        // Arrange
        var gameState = new GameState();
        var now = DateTime.UtcNow;
        gameState.DiscoState["lights"] = now;
        gameState.DiscoState["music"] = now.AddSeconds(2);

        // Act
        var firstCheck = gameState.CheckDiscoCompletion();
        var secondCheck = gameState.CheckDiscoCompletion();

        // Assert
        firstCheck.Should().BeTrue();
        secondCheck.Should().BeTrue();
        gameState.DiscoCompleted.Should().BeTrue();
    }

}
