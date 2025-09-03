using Refit;
using System.Text.Json;
using System.Threading.Tasks;
public class ContentRequest
{
    public Content[] contents { get; set; }
}

public class Content
{
    public Part[] parts { get; set; }
}

public class Part
{
    public string text { get; set; }
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
