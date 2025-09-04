using Refit;
using System.Threading.Tasks;

namespace CasinoClient.Services.Apis;

/// <summary>
/// Refit interface for interacting with the LLM API.
/// </summary>
public interface ILLMApi
{
    [Get("/llm/isupdated")]
    Task<ApiResponse<string>> IsUpdated([AliasAs("device")] int device);

    [Post("/llm/password")]
    Task<ApiResponse<string>> UpdatePassword([AliasAs("device")] int device);
}
