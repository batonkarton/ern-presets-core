namespace LoopstationCompanionApi.Services
{
    public interface IRc0Importer
    {
        Task<string> ImportAndSanitizeAsync(IFormFile file, CancellationToken ct = default);
    }
}
