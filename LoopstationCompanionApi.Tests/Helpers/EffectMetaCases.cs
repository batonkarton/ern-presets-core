namespace LoopstationCompanionApi.Tests.Helpers;

public static class EffectMetaCases
{
    public static IEnumerable<object[]> KnownParams() =>
        [
            ["AA_LPF", "A", "Rate", 0, 114, 0],
            [ "AA_CHORUS", "B", "Depth", 0, 100, 50 ],
        ];
}
