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
        throw new NotImplementedException();
    }

    public IFileInfo GetFileInfo(string path)
    {
        var fileRecord = _fileService.GetFileAsync(path).Result;
        return new SqliteDatabaseFileInfo(fileRecord);
    }

    public IChangeToken Watch(string filter)
    {
        return NullChangeToken.Singleton;
    }
}