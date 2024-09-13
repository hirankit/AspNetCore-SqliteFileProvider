namespace AspNetCoreSqliteFileProviderLibrary;

public record FileRecord(long Id, string Name, string Path, string ContentType, byte[] Content);
