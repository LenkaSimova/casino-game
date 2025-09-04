using Refit;
using System.Threading.Tasks;

namespace CasinoClient.Services.Apis;

/// <summary>
/// API interface for controlling disco features.
/// </summary>
public interface IDiscoApi
{
    [Post("/disco/music")]
    Task<ApiResponse<string>> MusicOn([AliasAs("device")] int device);

    [Post("/disco/lights")]
    Task<ApiResponse<string>> LightsOn([AliasAs("device")] int device);
}
