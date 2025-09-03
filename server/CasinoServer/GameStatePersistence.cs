using System.Text.Json;
using System.Text.Json.Serialization;

public class GameStatePersistence
{
    private readonly string _filePath;
    private readonly JsonSerializerOptions _jsonOptions;

    public GameStatePersistence(string filePath = "gamestate.json")
    {
        _filePath = filePath;
        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new JsonStringEnumConverter() }
        };
    }

    public async Task SaveGameStateAsync(GameState gameState)
    {
        try
        {
            // Create a serializable version of the game state
            var serializableState = new SerializableGameState
            {
                VideoUploaded = gameState.VideoUploaded,
                LoopStarted = gameState.LoopStarted,
                PasswordUpdated = gameState.PasswordUpdated,
                DiscoState = gameState.DiscoState.ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
                DiscoWindow = gameState.DiscoWindow,
                DiscoCompleted = gameState.DiscoCompleted,
                SavedAt = DateTime.UtcNow
            };

            var json = JsonSerializer.Serialize(serializableState, _jsonOptions);
            await File.WriteAllTextAsync(_filePath, json);
        }
        catch (Exception ex)
        {
            // Log error but don't throw to avoid crashing the application
            Console.WriteLine($"Error saving game state: {ex.Message}");
        }
    }

    public async Task<GameState> LoadGameStateAsync()
    {
        try
        {
            if (!File.Exists(_filePath))
            {
                return new GameState();
            }

            var json = await File.ReadAllTextAsync(_filePath);
            var serializableState = JsonSerializer.Deserialize<SerializableGameState>(json, _jsonOptions);

            if (serializableState == null)
            {
                return new GameState();
            }

            var gameState = new GameState
            {
                VideoUploaded = serializableState.VideoUploaded,
                LoopStarted = serializableState.LoopStarted,
                PasswordUpdated = serializableState.PasswordUpdated,
                DiscoWindow = serializableState.DiscoWindow,
                DiscoCompleted = serializableState.DiscoCompleted
            };

            // Restore DiscoState
            foreach (var kvp in serializableState.DiscoState)
            {
                gameState.DiscoState.TryAdd(kvp.Key, kvp.Value);
            }

            Console.WriteLine($"Game state loaded from {_filePath} (saved at: {serializableState.SavedAt})");
            return gameState;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading game state: {ex.Message}");
            return new GameState();
        }
    }

    public void DeleteSavedState()
    {
        try
        {
            if (File.Exists(_filePath))
            {
                File.Delete(_filePath);
                Console.WriteLine($"Saved game state deleted from {_filePath}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting saved state: {ex.Message}");
        }
    }
}

// Serializable version of GameState for JSON persistence
public class SerializableGameState
{
    public bool VideoUploaded { get; set; }
    public bool LoopStarted { get; set; }
    public bool PasswordUpdated { get; set; }
    public Dictionary<string, DateTime> DiscoState { get; set; } = new();
    public TimeSpan DiscoWindow { get; set; }
    public bool DiscoCompleted { get; set; }
    public DateTime SavedAt { get; set; }
}
