using System.Collections.Concurrent;

public class GameState
{
    public bool VideoUploaded { get; set; } = false;
    public bool LoopStarted { get; set; } = false;
    public bool PasswordUpdated { get; set; } = false;
    public ConcurrentDictionary<string, DateTime> DiscoState { get; set; } = new();
    public TimeSpan DiscoWindow { get; set; } = TimeSpan.FromSeconds(5); // 5s window for disco
    public bool DiscoCompleted { get; set; } = false;

    // Method to check if disco is completed and update flag
    public void CheckDiscoCompletion()
    {
        if (DiscoState.TryGetValue("lights", out var lightsTime) &&
            DiscoState.TryGetValue("music", out var musicTime))
        {
            // Check if both lights and music were activated within the time window
            if (Math.Abs((lightsTime - musicTime).TotalSeconds) <= DiscoWindow.TotalSeconds)
            {
                DiscoCompleted = true;
            }
        }
    }
}
