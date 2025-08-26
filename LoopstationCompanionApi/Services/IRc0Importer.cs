namespace LoopstationCompanionApi.Services
{
    public interface IRc0Importer
    {
        Task<string> ImportAndSanitizeAsync(Stream rc0Stream, CancellationToken ct = default);
    }
}
