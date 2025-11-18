using Dapper;
using Microsoft.Data.Sqlite;

namespace AspNetCoreSqliteFileProviderLibrary;
public class FileService
{
    private readonly string _connectionString;

    public FileService(string connectionString)
    {
        _connectionString = connectionString;
        InitializeDatabase();
    }

    private void InitializeDatabase()
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
        connection.Execute(query);
    }

    public FileRecord? GetFile(string filePath)
    {
        using var connection = new SqliteConnection(_connectionString);
        var query = "SELECT Id, Name, Path, ContentType, Content FROM Files WHERE Path = @Path";
        return connection.QueryFirstOrDefault<FileRecord?>(query, new { Path = filePath });
    }

    public void SaveFile(FileRecord file)
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
        connection.Execute(query, file);
    }
}
