namespace LoopstationCompanionApi.Tests.Helpers;

public static class TestConstants
{
    public static class ValidationMessages
    {
        public const string FileRequired = "File is required.";
        public const string OnlyRc0Supported = "Only .rc0 files are supported.";
        public const string NameRequired = "Name is required.";
        public const string Rc0ValidationFailedContains = "RC0 validation failed";
    }

    public static class JsonKeys
    {
        public const string Database = "database";
        public const string Ifx = "ifx";
        public const string Text = "_text";
    }

    public static class Effects
    {
        public const string AA_LPF = "AA_LPF";
        public const string AA_CHORUS = "AA_CHORUS";
        public const string MyUnknownEffect = "MY_UNKNOWN_EFFECT";
    }

    public static class Params
    {
        public const string Rate = "Rate";
        public const string Depth = "Depth";
        public const string Resonance = "Resonance";
        public const string Cutoff = "Cutoff";
        public const string StepRate = "Step Rate";
    }

    public static class Devices
    {
        public const string RC505mkII = "RC505mkII";
        public const string RC505mkI = "RC505mkI";
        public const string RC505mkI_Hyphen = "RC-505mkI"; // when DTO expects hyphenated form
    }
}
