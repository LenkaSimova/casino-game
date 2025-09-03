using Refit;
using System.Threading.Tasks;

namespace CasinoClient.Services.Apis;

public interface ILLMApi
{
    [Post("/llm/isupdated")]
    Task<ApiResponse<string>> IsUpdated([AliasAs("device")] int device);

    [Post("/llm/password")]
    Task<ApiResponse<string>> UpdatePassword([AliasAs("device")] int device);
}
