using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;

namespace AspNetCoreSqliteFileProviderLibrary;
public class FileService
{
    private readonly string _connectionString;
    private readonly ILogger<FileService>? _logger;

    public FileService(string connectionString, ILogger<FileService>? logger = null)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("Connection string cannot be null or empty.", nameof(connectionString));
        
        _connectionString = connectionString;
        _logger = logger;
        
        try
        {
            _logger?
            .LogInformation("Initializing SQLite file database with connection string: {ConnectionString}",
  connectionString);
            InitializeDatabase();
            _logger?
            .LogInformation("Database initialized successfully");
        }
        catch (Exception ex)
        {
            _logger?
            .LogError(ex, "Failed to initialize database");
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

        _logger?
        .LogDebug("Retrieving file: {FilePath}", filePath);
        try
        {
            using var connection = new SqliteConnection(_connectionString);
            var query = "SELECT Id, Name, Path, ContentType, Content, Length, CreatedAt, LastModifiedAt FROM Files WHERE Path = @Path";
            var result = connection.QueryFirstOrDefault<FileRecord?>(query, new { Path = filePath });
            if (result == null)
            {
                _logger?
                .LogDebug("File not found: {FilePath}", filePath);
           }
            else
            {
                _logger?
                .LogDebug("File retrieved successfully: {FilePath} ({Length} bytes)", filePath, result.Length);
            }
            return result;
        }
        catch (Exception ex)
        {
            _logger?
            .LogError(ex, "Failed to retrieve file: {FilePath}", filePath);
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

        _logger?
        .LogInformation("Saving file: {FilePath} ({Length} bytes)", file.Path, file.Length);
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
            _logger?
            .LogInformation("File saved successfully: {FilePath}", file.Path);
        }
        catch (Exception ex)
        {
            _logger?
            .LogError(ex, "Failed to save file: {FilePath}", file.Path);
            throw new InvalidOperationException($"Failed to save the file '{file.Path}' to database.", ex);
        }
    }
}
