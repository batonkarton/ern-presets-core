namespace LoopstationCompanionApi.Services
{
    public interface IRc0Validator
    {
        Task<IReadOnlyList<string>> ValidateAsync(string cleanedXml, CancellationToken ct = default);
    }
}
