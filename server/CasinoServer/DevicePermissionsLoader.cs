// load device permissions from config file
using System.Text.Json;
using System.Text.Json.Nodes;

class DevicePermissionsLoader
{
    public static async Task<Dictionary<string, HashSet<string>>> LoadDevicePermissionsAsync(string path = "device_permissions.json")
    {
        try
        {
            var permissionsJson = await File.ReadAllTextAsync(path);
            var root = JsonNode.Parse(permissionsJson)?["DevicePermissions"];
            if (root is JsonObject obj)
            {
                return obj.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value is JsonArray arr ? arr.Select(x => x?.ToString() ?? "").ToHashSet() : new HashSet<string>()
                );
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to load device permissions: {ex.Message}");
        }
        return new Dictionary<string, HashSet<string>>();
    }
}