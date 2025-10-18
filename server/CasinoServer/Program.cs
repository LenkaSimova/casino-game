
var builder = WebApplication.CreateBuilder(args);

// Configure logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Information);

var app = builder.Build();

// Add request logging middleware
app.Use(async (context, next) =>
{
    var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
    var deviceId = context.Request.Query.TryGetValue("device", out var device) ? device.ToString() : "none";

    logger.LogInformation(
        "Incoming request: {Method} {Path} | Device: {DeviceId} | RemoteIP: {RemoteIp}",
        context.Request.Method,
        context.Request.Path,
        deviceId,
        context.Connection.RemoteIpAddress
    );

    await next();

    logger.LogInformation(
        "Response: {Method} {Path} | Status: {StatusCode} | Device: {DeviceId}",
        context.Request.Method,
        context.Request.Path,
        context.Response.StatusCode,
        deviceId
    );
});

// Initialize persistence service
var persistence = new GameStatePersistence();

// Load game state from file (or create new if file doesn't exist)
var gameState = await persistence.LoadGameStateAsync();

var devicePermissions = await DevicePermissionsLoader.LoadDevicePermissionsAsync();

// Helper: check if device is allowed for endpoint
bool IsDeviceAllowed(string endpoint, string? deviceId)
{
    if (deviceId == null) return false;
    if (devicePermissions.TryGetValue(endpoint, out var allowed))
        return allowed.Contains(deviceId);
    return false;
}

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
    if (!IsDeviceAllowed("/video/upload", device))
        return Results.BadRequest("Device not allowed to confirm upload.");
    gameState.VideoUploaded = true;
    persistence.SaveGameStateAsync(gameState).Wait();
    return Results.Ok("Video upload confirmed.");
});


app.MapPost("/video/loop", (HttpContext ctx) =>
{
    var device = GetDeviceId(ctx);
    if (!IsDeviceAllowed("/video/loop", device))
        return Results.BadRequest("Device not allowed to request loop.");
    if (!gameState.VideoUploaded) return Results.BadRequest("Video not uploaded yet.");
    gameState.LoopStarted = true;
    persistence.SaveGameStateAsync(gameState).Wait();
    return Results.Ok("Loop started.");
});

// 2. LLM relay

app.MapGet("/llm/isupdated", (HttpContext ctx) =>
{
    var device = GetDeviceId(ctx);
    if (!IsDeviceAllowed("/llm/isupdated", device))
        return Results.BadRequest("Device not allowed to query LLM.");
    if (!gameState.PasswordUpdated) return Results.BadRequest("Password not updated yet.");
    return Results.Ok("Password is updated.");
});



app.MapPost("/llm/password", (HttpContext ctx) =>
{
    var device = GetDeviceId(ctx);
    if (!IsDeviceAllowed("/llm/password", device))
        return Results.BadRequest("Device not allowed to set password.");
    gameState.PasswordUpdated = true;
    persistence.SaveGameStateAsync(gameState).Wait();
    return Results.Ok("Password updated.");
});

// 3. Disco endpoints

app.MapPost("/disco/lights", (HttpContext ctx) =>
{
    var device = GetDeviceId(ctx);
    if (!IsDeviceAllowed("/disco/lights", device))
        return Results.BadRequest("Device not allowed to confirm lights.");
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
    if (!IsDeviceAllowed("/disco/music", device))
        return Results.BadRequest("Device not allowed to confirm music.");
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

// Make Program class accessible for testing
public partial class Program { }
