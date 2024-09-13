using Microsoft.Extensions.FileProviders;

namespace AspNetCoreSqliteFileProviderLibrary;

public class SqliteDatabaseFileInfo(FileRecord? fileRecord) : IFileInfo
{
    private readonly FileRecord? _fileRecord = fileRecord;

    public bool Exists => _fileRecord != null;
    public long Length => _fileRecord == null ? 0 : _fileRecord.Content.Length;
    public string Name => _fileRecord == null ? string.Empty : _fileRecord.Name;
    public DateTimeOffset LastModified => DateTimeOffset.Now;
    public bool IsDirectory => false;

    public string PhysicalPath => null!;

    public Stream CreateReadStream()
    {
        if (_fileRecord == null) throw new InvalidOperationException();

        return new MemoryStream(_fileRecord.Content);
    }

}
