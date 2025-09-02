namespace LoopstationCompanionApi.Services
{
    public static class ErrorUtils
    {
        /// <summary>
        /// Joins error messages in a consistent, testable way.
        /// </summary>
        public static string JoinMessages(IEnumerable<string> messages, string separator = "; ")
            => string.Join(separator, messages ?? Array.Empty<string>());
    }
}
