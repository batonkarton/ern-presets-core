namespace LoopstationCompanionApi.Tests.Helpers;

public static class DictBuilders
{
    // Builds: { effect -> { label -> { "_text": value } } }
    public static IDictionary<string, object> BuildPartialEffect(
        string effect,
        params (string label, string value)[] pairs)
    {
        var paramDict = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

        foreach (var (label, value) in pairs)
        {
            paramDict[label] = new Dictionary<string, string>
            {
                [TestConstants.JsonKeys.Text] = value
            };
        }

        return new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
        {
            [effect] = paramDict
        };
    }

    // Builds: { effect -> { paramLabel -> { "_text": value } } }
    public static IDictionary<string, object> BuildUnknownEffect(
        string effect,
        string paramLabel,
        string value)
    {
        return new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
        {
            [effect] = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
            {
                [paramLabel] = new Dictionary<string, string>
                {
                    [TestConstants.JsonKeys.Text] = value
                }
            }
        };
    }
}
