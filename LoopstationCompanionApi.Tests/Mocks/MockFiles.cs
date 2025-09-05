using Microsoft.AspNetCore.Http;
using System.Text;

namespace LoopstationCompanionApi.Tests.Mocks;

public static class MockFiles
{
    public static IFormFile FromString(string fileName, string content, string contentType = "application/octet-stream")
    {
        var bytes = Encoding.UTF8.GetBytes(content);
        var stream = new MemoryStream(bytes);
        return new FormFile(stream, 0, bytes.Length, "file", fileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = contentType
        };
    }
}
