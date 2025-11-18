namespace AspNetCoreSqliteFileProviderLibrary;

public record FileRecord(
    long Id,
    string Name,
    string Path,
    string ContentType,
    byte[] Content,
    long Length,
    DateTime CreatedAt,
    DateTime LastModifiedAt);
