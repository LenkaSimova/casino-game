using Refit;
using System.Text.Json;
using System.Threading.Tasks;

// Class structure for content generation request
public class ContentRequest
{
    public SystemInstruction? system_instruction { get; set; }
    public Content[]? contents { get; set; }
    public GenerationConfig? generationConfig { get; set; }
}

public class Content
{
    public Part[]? parts { get; set; }
}

public class Part
{
    public string? text { get; set; }
}
public class GenerationConfig
{
    public ThinkingConfig? thinkingConfig { get; set; }
}

public class ThinkingConfig
{
    public string? thinkingBudget { get; set; }
}

public class SystemInstruction
{
    public Part[]? parts { get; set; }
}

/// <summary>
/// Refit interface for Gemini API content generation endpoint.
/// </summary>
public interface IGeminiApi
{
    /// <summary>
    /// Calls Gemini API to generate content using the specified model and request payload.
    /// </summary>
    /// <param name="model">Model name or ID.</param>
    /// <param name="request">Content generation request payload.</param>
    /// <param name="key">API key for authentication.</param>
    /// <returns>API response containing generated content as a JsonElement.</returns>
    [Post("/v1beta/models/{model}:generateContent")]
    Task<ApiResponse<JsonElement>> GenerateContentAsync(
        [AliasAs("model")] string model,
        [Body] ContentRequest request,
        [Query] string key // for API key auth
    );
}
