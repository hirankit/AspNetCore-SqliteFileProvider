using Dapper;
using Microsoft.Data.Sqlite;

namespace AspNetCoreSqliteFileProviderLibrary;
public class FileService
{
    private readonly string _connectionString;

    public FileService(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("Connection string cannot be null or empty.", nameof(connectionString));
        
        _connectionString = connectionString;
        
        try
        {
            InitializeDatabase();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to initialize the database.", ex);
        }
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
        Content BLOB NOT NULL,
        Length INTEGER NOT NULL,
        CreatedAt DATETIME NOT NULL,
        LastModifiedAt DATETIME NOT NULL
        );
        ";
        connection.Execute(query);
    }

    public FileRecord? GetFile(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("File path cannot be null or empty.", nameof(filePath));

        try
        {
            using var connection = new SqliteConnection(_connectionString);
            var query = "SELECT Id, Name, Path, ContentType, Content, Length, CreatedAt, LastModifiedAt FROM Files WHERE Path = @Path";
            return connection.QueryFirstOrDefault<FileRecord?>(query, new { Path = filePath });
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to retrieve the file '{filePath}' from database.", ex);
        }
    }

    public void SaveFile(FileRecord file)
    {
        if (file == null)
            throw new ArgumentNullException(nameof(file));

        if (string.IsNullOrWhiteSpace(file.Path))
            throw new ArgumentException("File path cannot be null or empty.", nameof(file.Path));

        if (file.Content == null)
            throw new ArgumentException("File content cannot be null.", nameof(file.Content));

        try
        {
            using var connection = new SqliteConnection(_connectionString);
            var query = @"
            INSERT INTO Files (Name, Path, ContentType, Content, Length, CreatedAt, LastModifiedAt)
            VALUES (@Name, @Path, @ContentType, @Content, @Length, @CreatedAt, @LastModifiedAt)
            ON CONFLICT(Path) DO
                UPDATE SET ContentType = excluded.ContentType,
                Content = excluded.Content,
                Length = excluded.Length,
                LastModifiedAt = excluded.LastModifiedAt
            ;
            ";
            connection.Execute(query, file);   
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to save the file '{file.Path}' to database.", ex);
        }
    }
}
