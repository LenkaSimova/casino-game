
using System.Collections.Concurrent;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// In-memory state
var videoUploaded = false;
var loopStarted = false;
var llmPassword = string.Empty;
var discoState = new ConcurrentDictionary<string, DateTime>();
var discoWindow = TimeSpan.FromSeconds(5); // 5s window for disco

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
    videoUploaded = true;
    return Results.Ok("Video upload confirmed.");
});

app.MapPost("/video/loop", (HttpContext ctx) =>
{
    var device = GetDeviceId(ctx);
    if (device != "3") return Results.BadRequest("Only device 3 can request loop.");
    if (!videoUploaded) return Results.BadRequest("Video not uploaded yet.");
    loopStarted = true;
    return Results.Ok("Loop started.");
});

// 2. LLM relay
app.MapPost("/llm/query", async (HttpContext ctx) =>
{
    var device = GetDeviceId(ctx);
    if (device != "2") return Results.BadRequest("Only device 2 can query LLM.");
    using var reader = new StreamReader(ctx.Request.Body);
    var prompt = await reader.ReadToEndAsync();
    if (!string.IsNullOrEmpty(llmPassword))
        prompt = $"[password:{llmPassword}] {prompt}";
    // Simulate LLM relay (replace with real call)
    var llmResponse = $"LLM response to: {prompt}";
    return Results.Ok(llmResponse);
});

app.MapPost("/llm/password", async (HttpContext ctx) =>
{
    var device = GetDeviceId(ctx);
    if (device != "1") return Results.BadRequest("Only device 1 can set password.");
    using var reader = new StreamReader(ctx.Request.Body);
    llmPassword = await reader.ReadToEndAsync();
    return Results.Ok("Password set.");
});

// 3. Disco endpoints
app.MapPost("/disco/lights", (HttpContext ctx) =>
{
    var device = GetDeviceId(ctx);
    if (device != "2") return Results.BadRequest("Only device 2 can confirm lights.");
    discoState["lights"] = DateTime.UtcNow;
    return Results.Ok("Lights confirmed.");
});

app.MapPost("/disco/music", (HttpContext ctx) =>
{
    var device = GetDeviceId(ctx);
    if (device != "3") return Results.BadRequest("Only device 3 can confirm music.");
    discoState["music"] = DateTime.UtcNow;
    return Results.Ok("Music confirmed.");
});

app.MapGet("/disco/status", () =>
{
    if (discoState.TryGetValue("lights", out var t1) && discoState.TryGetValue("music", out var t2))
    {
        if (Math.Abs((t1 - t2).TotalSeconds) <= discoWindow.TotalSeconds)
            return Results.Ok("Disco task complete!");
    }
    return Results.Ok("Disco not complete.");
});

// Status endpoint for debugging
app.MapGet("/status", () => new
{
    videoUploaded,
    loopStarted,
    llmPasswordSet = !string.IsNullOrEmpty(llmPassword),
    discoState
});

app.Run();
