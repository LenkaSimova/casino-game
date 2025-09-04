using Refit;
using System.Threading.Tasks;

namespace CasinoClient.Services.Apis;

/// <summary>
/// Refit interface for interacting with the Video API.
/// </summary>
public interface IVideoApi
{
    [Post("/video/upload")]
    Task<ApiResponse<string>> UploadVideo([AliasAs("device")] int device);

    [Post("/video/loop")]
    Task<ApiResponse<string>> LoopVideo([AliasAs("device")] int device);
}
