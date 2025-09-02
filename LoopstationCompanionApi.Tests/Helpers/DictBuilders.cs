namespace LoopstationCompanionApi.UnitTests.Helpers;

public static class DictBuilders
{
    // Makes: { effect -> { label -> {"_text": value} } }
    public static IDictionary<string, object> PartialEffect(string effect, params (string label, string value)[] pairs)
    {
        var paramDict = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        foreach (var (label, value) in pairs)
        {
            paramDict[label] = new Dictionary<string, string> { ["_text"] = value };
        }

        return new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
        {
            [effect] = paramDict
        };
    }

    public static IDictionary<string, object> UnknownEffect(string effect, string paramLabel, string value)
    {
        return new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
        {
            [effect] = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
            {
                [paramLabel] = new Dictionary<string, string> { ["_text"] = value }
            }
        };
    }
}
