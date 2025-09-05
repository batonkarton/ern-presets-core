using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;

namespace LoopstationCompanionApi.Tests.Helpers;

public sealed class TempHostEnvironment : IHostEnvironment, IDisposable
{
    public TempHostEnvironment()
    {
        ContentRootPath = Path.Combine(Path.GetTempPath(), "lcp-tests-" + Guid.NewGuid());
        Directory.CreateDirectory(ContentRootPath);
        ContentRootFileProvider = new PhysicalFileProvider(ContentRootPath);
    }

    public string EnvironmentName { get; set; } = "Development";
    public string ApplicationName { get; set; } = "Tests";
    public string ContentRootPath { get; set; }
    public IFileProvider ContentRootFileProvider { get; set; }

    public void Dispose()
    {
        try { Directory.Delete(ContentRootPath, recursive: true); } catch { }
    }
}
