using Refit;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Linq;

namespace CasinoClient.Services.Apis;

/// <summary>
/// Refit interface for interacting with the Music API and Mopidy JSON-RPC.
/// </summary>
public interface IMusicApi
{
    // Mopidy JSON-RPC endpoints (POST to /mopidy/rpc with a JSON-RPC body)

    [Post("/mopidy/rpc")]
    Task<ApiResponse<string>> ClearTracklist([Body] JsonRpcRequest body);

    [Post("/mopidy/rpc")]
    Task<ApiResponse<string>> AddTracks([Body] JsonRpcRequest body);

    [Post("/mopidy/rpc")]
    Task<ApiResponse<string>> Play([Body] JsonRpcRequest body);
}

/// <summary>
/// Simple JSON-RPC request model used for Mopidy calls.
/// Use MopidyRequests helpers to construct standard requests.
/// </summary>
public class JsonRpcRequest
{
    [JsonPropertyName("jsonrpc")]
    public string Jsonrpc { get; set; } = "2.0";

    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("method")]
    public string Method { get; set; } = string.Empty;

    [JsonPropertyName("params")]
    public object? Params { get; set; }
}

/// <summary>
/// Convenience factory helpers to build the three example requests.
/// </summary>
public static class MopidyRequests
{
    public static JsonRpcRequest Clear(int id = 1) =>
        new JsonRpcRequest { Id = id, Method = "core.tracklist.clear", Params = new { } };

    public static JsonRpcRequest AddUris(IEnumerable<string> uris, int id = 2) =>
        new JsonRpcRequest
        {
            Id = id,
            Method = "core.tracklist.add",
            Params = new
            {
                uris = uris
                    .Select(u => $"file:///var/lib/mopidy/iris/zaver/{u}")
                    .ToArray()
            }
        };

    public static JsonRpcRequest Play(int id = 3) =>
        new JsonRpcRequest { Id = id, Method = "core.playback.play", Params = new { } };
}