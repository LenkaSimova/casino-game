using Refit;
using System.Threading.Tasks;

namespace CasinoClient.Services.Apis;

public interface ILLMApi
{
    [Post("/llm/query")]
    Task<ApiResponse<string>> Query([AliasAs("device")] int device, [Body] string query);

    [Post("/llm/password")]
    Task<ApiResponse<string>> UpdatePassword([AliasAs("device")] int device);
}
