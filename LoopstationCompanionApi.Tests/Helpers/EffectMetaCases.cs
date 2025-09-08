namespace LoopstationCompanionApi.Tests.Helpers;

public static class EffectMetaCases
{
    public static IEnumerable<object[]> GetKnownParams() =>
    [
        [ TestConstants.Effects.AA_LPF,   "A", TestConstants.Params.Rate,  0, 114, 0 ],
        [ TestConstants.Effects.AA_CHORUS,"B", TestConstants.Params.Depth, 0, 100, 50 ],
    ];
}
