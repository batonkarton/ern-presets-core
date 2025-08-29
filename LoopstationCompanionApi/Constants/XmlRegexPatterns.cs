namespace LoopstationCompanionApi.Constants
{
    public static class XmlRegexPatterns
    {
        public const string OpenNumericTag = "<([0-9])>";
        public const string CloseNumericTag = "</([0-9])>";
        public const string OpenNumericReplacement = "<NUM_$1>";
        public const string CloseNumericReplacement = "</NUM_$1>";

        public const string OpenSymbolTag = "<([^\\w\\.-:])>";
        public const string CloseSymbolTag = "</([^\\w\\.-:])>";
        public const string OpenSymbolReplacement = "<SYMBOL_{0}>";
        public const string CloseSymbolReplacement = "</SYMBOL_{0}>";

        public const string CountTagPattern = "<count>.*?</count>";
    }
}
