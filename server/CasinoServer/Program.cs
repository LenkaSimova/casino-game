
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// Initialize persistence service
var persistence = new GameStatePersistence();

// Load game state from file (or create new if file doesn't exist)
var gameState = await persistence.LoadGameStateAsync();

// Helper: get device id from query
string? GetDeviceId(HttpContext ctx)
{
    if (ctx.Request.Query.TryGetValue("device", out var query))
        return query.ToString();
    return null;
}

// 1. Video upload/loop endpoints
app.MapPost("/video/upload", (HttpContext ctx) =>
{
    var device = GetDeviceId(ctx);
    if (device != "1") return Results.BadRequest("Only device 1 can confirm upload.");
    gameState.VideoUploaded = true;
    persistence.SaveGameStateAsync(gameState).Wait();
    return Results.Ok("Video upload confirmed.");
});

app.MapPost("/video/loop", (HttpContext ctx) =>
{
    var device = GetDeviceId(ctx);
    if (device != "1") return Results.BadRequest("Only device 3 can request loop.");
    if (!gameState.VideoUploaded) return Results.BadRequest("Video not uploaded yet.");
    gameState.LoopStarted = true;
    persistence.SaveGameStateAsync(gameState).Wait();
    return Results.Ok("Loop started.");
});

// 2. LLM relay
app.MapGet("/llm/isupdated", (HttpContext ctx) =>
{
    var device = GetDeviceId(ctx);
    if (device != "1") return Results.BadRequest("Only device 2 can query LLM.");
    if (!gameState.PasswordUpdated) return Results.BadRequest("Password not updated yet.");
    return Results.Ok("Password is updated.");
});


app.MapPost("/llm/password", (HttpContext ctx) =>
{
    var device = GetDeviceId(ctx);
    if (device != "1") return Results.BadRequest("Only device 1 can set password.");
    gameState.PasswordUpdated = true;
    persistence.SaveGameStateAsync(gameState).Wait();
    return Results.Ok("Password updated.");
});

// 3. Disco endpoints
app.MapPost("/disco/lights", (HttpContext ctx) =>
{
    var device = GetDeviceId(ctx);
    if (device != "1") return Results.BadRequest("Only device 2 can confirm lights.");
    gameState.DiscoState["lights"] = DateTime.UtcNow;
    var discoCompleted = gameState.CheckDiscoCompletion(); // Check if disco is now completed
    if (discoCompleted)
    {
        persistence.SaveGameStateAsync(gameState).Wait();
    }
    return Results.Ok((int)gameState.DiscoWindow.TotalSeconds);
});

app.MapPost("/disco/music", (HttpContext ctx) =>
{
    var device = GetDeviceId(ctx);
    if (device != "1") return Results.BadRequest("Only device 3 can confirm music.");
    gameState.DiscoState["music"] = DateTime.UtcNow;
    var discoCompleted = gameState.CheckDiscoCompletion(); // Check if disco is now completed
    if (discoCompleted)
    {
        persistence.SaveGameStateAsync(gameState).Wait();
    }
    return Results.Ok((int)gameState.DiscoWindow.TotalSeconds);
});

app.MapGet("/disco/status", () =>
{
    return Results.Ok(new
    {
        gameState.DiscoCompleted
    });
});

// Status endpoint for debugging
app.MapGet("/status", () => new
{
    gameState.VideoUploaded,
    gameState.LoopStarted,
    gameState.PasswordUpdated,
    gameState.DiscoState,
    gameState.DiscoCompleted
});

// Admin endpoints for managing saved state
app.MapPost("/admin/reset", async () =>
{
    gameState.VideoUploaded = false;
    gameState.LoopStarted = false;
    gameState.PasswordUpdated = false;
    gameState.DiscoState.Clear();
    gameState.DiscoCompleted = false;
    await persistence.SaveGameStateAsync(gameState);
    return Results.Ok("Game state reset and saved.");
});

app.MapPost("/admin/save", async () =>
{
    await persistence.SaveGameStateAsync(gameState);
    return Results.Ok("Game state manually saved.");
});

app.MapDelete("/admin/savedstate", () =>
{
    persistence.DeleteSavedState();
    return Results.Ok("Saved state file deleted.");
});

app.Run();
