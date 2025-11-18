using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;

namespace AspNetCoreSqliteFileProviderLibrary;

public class DatabaseFileProvider : IFileProvider
{
    private readonly FileService _fileService;

    public DatabaseFileProvider(FileService fileService)
    {
        _fileService = fileService;
    }

    public IDirectoryContents GetDirectoryContents(string path)
    {
        throw new NotImplementedException(); // Directory listing is not supported
    }

    public IFileInfo GetFileInfo(string path)
    {
        if (string.IsNullOrEmpty(path))
            throw new ArgumentException("Path cannot be null or empty.", nameof(path));

        path = path.Replace("\\", "/");
        if (path.Contains(".."))
            throw new ArgumentException("Path traversal is not allowed.", nameof(path));
        
        var fileRecord = _fileService.GetFile(path);
        return new SqliteDatabaseFileInfo(fileRecord);
    }

    public IChangeToken Watch(string filter)
    {
        return NullChangeToken.Singleton;
    }
}