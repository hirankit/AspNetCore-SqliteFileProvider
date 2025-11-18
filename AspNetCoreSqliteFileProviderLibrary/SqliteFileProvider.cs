using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

namespace AspNetCoreSqliteFileProviderLibrary;

public class DatabaseFileProvider : IFileProvider
{
    private readonly FileService _fileService;
    private readonly ILogger<DatabaseFileProvider>? _logger;

    public DatabaseFileProvider(FileService fileService, ILogger<DatabaseFileProvider>? logger = null)
    {
        _fileService = fileService;
        _logger = logger;
        _logger?.LogInformation("Database file provider initialized");
    }

    public IDirectoryContents GetDirectoryContents(string path)
    {
        _logger?
        .LogWarning("GetDirectoryContents called for path: {Path} - Operation not supported", path);
        throw new NotImplementedException(); // Directory listing is not supported
    }

    public IFileInfo GetFileInfo(string path)
    {
        if (string.IsNullOrEmpty(path))
            throw new ArgumentException("Path cannot be null or empty.", nameof(path));

        path = path.Replace("\\", "/");
        if (path.Contains(".."))
        {
            _logger?
            .LogWarning("Path traversal attempt detected: {Path}", path);
            throw new ArgumentException("Path traversal is not allowed.", nameof(path));
        }

        _logger?.LogDebug("Getting file info for: {Path}", path);
        var fileRecord = _fileService.GetFile(path);
        return new SqliteDatabaseFileInfo(fileRecord);
    }

    public IChangeToken Watch(string filter)
    {
        _logger?
        .LogDebug("Watch called with filter: {Filter} - Returning NullChangeToken", filter);
        return NullChangeToken.Singleton;
    }
}