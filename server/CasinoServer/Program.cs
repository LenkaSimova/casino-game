
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// Initialize game state
var gameState = new GameState();

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
    return Results.Ok("Video upload confirmed.");
});

app.MapPost("/video/loop", (HttpContext ctx) =>
{
    var device = GetDeviceId(ctx);
    if (device != "1") return Results.BadRequest("Only device 3 can request loop.");
    if (!gameState.VideoUploaded) return Results.BadRequest("Video not uploaded yet.");
    gameState.LoopStarted = true;
    return Results.Ok("Loop started.");
});

// 2. LLM relay
app.MapPost("/llm/isupdated", (HttpContext ctx) =>
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
    return Results.Ok("Password updated.");
});

// 3. Disco endpoints
app.MapPost("/disco/lights", (HttpContext ctx) =>
{
    var device = GetDeviceId(ctx);
    if (device != "1") return Results.BadRequest("Only device 2 can confirm lights.");
    gameState.DiscoState["lights"] = DateTime.UtcNow;
    gameState.CheckDiscoCompletion(); // Check if disco is now completed
    return Results.Ok("Lights confirmed.");
});

app.MapPost("/disco/music", (HttpContext ctx) =>
{
    var device = GetDeviceId(ctx);
    if (device != "1") return Results.BadRequest("Only device 3 can confirm music.");
    gameState.DiscoState["music"] = DateTime.UtcNow;
    gameState.CheckDiscoCompletion(); // Check if disco is now completed
    return Results.Ok("Music confirmed.");
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

app.Run();
