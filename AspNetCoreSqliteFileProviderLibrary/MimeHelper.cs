using Microsoft.AspNetCore.StaticFiles;

namespace AspNetCoreSqliteFileProviderLibrary;

public static class MimeHelper
{
    private static readonly FileExtensionContentTypeProvider _provider =
        new FileExtensionContentTypeProvider();

    public static string GetContentType(string fileName)
    {
        if (_provider.TryGetContentType(fileName, out var contentType)) return contentType;

        return "application/octet-stream";
    }
}