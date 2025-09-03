
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();


// In-memory state
var videoUploaded = false;
var loopStarted = false;
var passwordUpdated = false;
var discoState = new ConcurrentDictionary<string, DateTime>();
var discoWindow = TimeSpan.FromSeconds(5); // 5s window for disco
var discoCompleted = false;

// Helper: get device id from query
string? GetDeviceId(HttpContext ctx)
{
    if (ctx.Request.Query.TryGetValue("device", out var query))
        return query.ToString();
    return null;
}

// Helper: check if disco is completed and update flag
void CheckDiscoCompletion()
{
    if (discoState.TryGetValue("lights", out var lightsTime) &&
        discoState.TryGetValue("music", out var musicTime))
    {
        // Check if both lights and music were activated within the time window
        if (Math.Abs((lightsTime - musicTime).TotalSeconds) <= discoWindow.TotalSeconds)
        {
            discoCompleted = true;
        }
    }
}

// 1. Video upload/loop endpoints
app.MapPost("/video/upload", (HttpContext ctx) =>
{
    var device = GetDeviceId(ctx);
    if (device != "1") return Results.BadRequest("Only device 1 can confirm upload.");
    videoUploaded = true;
    return Results.Ok("Video upload confirmed.");
});

app.MapPost("/video/loop", (HttpContext ctx) =>
{
    var device = GetDeviceId(ctx);
    if (device != "1") return Results.BadRequest("Only device 3 can request loop.");
    if (!videoUploaded) return Results.BadRequest("Video not uploaded yet.");
    loopStarted = true;
    return Results.Ok("Loop started.");
});

// 2. LLM relay
app.MapPost("/llm/isupdated", (HttpContext ctx) =>
{
    var device = GetDeviceId(ctx);
    if (device != "1") return Results.BadRequest("Only device 2 can query LLM.");
    if (!passwordUpdated) return Results.BadRequest("Password not updated yet.");
    return Results.Ok("Password is updated.");
});


// 2. LLM relay
// app.MapPost("/llm/query", async (HttpContext ctx) =>
// {
//     var device = GetDeviceId(ctx);
//     if (device != "1") return Results.BadRequest("Only device 2 can query LLM.");
//     using var reader = new StreamReader(ctx.Request.Body);
//     var prompt = await reader.ReadToEndAsync();
//     if (!string.IsNullOrEmpty(llmPassword))
//         prompt = $"[password:{llmPassword}] {prompt}";
//     // Simulate LLM relay (replace with real call)
//     var llmResponse = $"LLM response to: {prompt}";
//     return Results.Ok(llmResponse);
// });

app.MapPost("/llm/password", (HttpContext ctx) =>
{
    var device = GetDeviceId(ctx);
    if (device != "1") return Results.BadRequest("Only device 1 can set password.");
    passwordUpdated = true;
    return Results.Ok("Password updated.");
});

// 3. Disco endpoints
app.MapPost("/disco/lights", (HttpContext ctx) =>
{
    var device = GetDeviceId(ctx);
    if (device != "1") return Results.BadRequest("Only device 2 can confirm lights.");
    discoState["lights"] = DateTime.UtcNow;
    CheckDiscoCompletion(); // Check if disco is now completed
    return Results.Ok("Lights confirmed.");
});

app.MapPost("/disco/music", (HttpContext ctx) =>
{
    var device = GetDeviceId(ctx);
    if (device != "1") return Results.BadRequest("Only device 3 can confirm music.");
    discoState["music"] = DateTime.UtcNow;
    CheckDiscoCompletion(); // Check if disco is now completed
    return Results.Ok("Music confirmed.");
});

// Status endpoint for debugging
app.MapGet("/status", () => new
{
    videoUploaded,
    loopStarted,
    passwordUpdated,
    discoState,
    discoCompleted
});

app.Run();
