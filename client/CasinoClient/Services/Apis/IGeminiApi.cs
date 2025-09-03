using Refit;
using System.Text.Json;
using System.Threading.Tasks;
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

public interface IGeminiApi
{
    [Post("/v1beta/models/{model}:generateContent")]
    Task<ApiResponse<JsonElement>> GenerateContentAsync(
        [AliasAs("model")] string model,
        [Body] ContentRequest request,
        [Query] string key // for API key auth
    );
}
