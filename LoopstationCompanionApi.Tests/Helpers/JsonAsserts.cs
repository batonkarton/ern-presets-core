using System.Text.Json;

namespace LoopstationCompanionApi.Tests.Helpers;

public static class JsonAsserts
{
    public static JsonElement Root(this string json)
        => JsonDocument.Parse(json).RootElement;

    public static string? GetStringAt(this string json, params string[] path)
    {
        var cur = json.Root();
        foreach (var p in path) cur = cur.GetProperty(p);
        return cur.GetString();
    }

    public static JsonElement Get(this string json, params string[] path)
    {
        var cur = json.Root();
        foreach (var p in path) cur = cur.GetProperty(p);
        return cur;
    }
}
