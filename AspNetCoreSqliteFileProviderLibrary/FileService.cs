using Dapper;
using Microsoft.Data.Sqlite;

namespace AspNetCoreSqliteFileProviderLibrary;
public class FileService
{
    private readonly string _connectionString;

    public FileService(string connectionString)
    {
        _connectionString = connectionString;
        InitializeDatabase().Wait();
    }

    private async Task InitializeDatabase()
    {
        using var connection = new SqliteConnection(_connectionString);
        var query = @"
        CREATE TABLE IF NOT EXISTS Files (
        Id INTEGER PRIMARY KEY AUTOINCREMENT,
        Name TEXT NOT NULL,
        Path TEXT NOT NULL UNIQUE,
        ContentType TEXT NOT NULL,
        Content BLOB NOT NULL
        );
        ";
        await connection.ExecuteAsync(query);
    }

    public async Task<FileRecord?> GetFileAsync(string filePath)
    {
        using var connection = new SqliteConnection(_connectionString);
        var query = "SELECT Id, Name, Path, ContentType, Content FROM Files WHERE Path = @Path";
        return await connection.QueryFirstOrDefaultAsync<FileRecord?>(query, new { Path = filePath });
    }

    public async Task SaveFileAsync(FileRecord file)
    {
        using var connection = new SqliteConnection(_connectionString);
        var query = @"
        INSERT INTO Files (Name, Path, ContentType, Content)
        VALUES (@Name, @Path, @ContentType, @Content)
        ON CONFLICT(Path) DO
            UPDATE SET ContentType = excluded.ContentType,
            Content = excluded.Content
        ;
        ";
        await connection.ExecuteAsync(query, file);
    }
}
