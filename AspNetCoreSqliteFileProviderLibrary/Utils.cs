using Microsoft.Extensions.Logging;

namespace AspNetCoreSqliteFileProviderLibrary;

public static class Utils
{
    public static void StoreFilesFromDirectoryAsync(FileService fileService, string directoryPath, ILogger? logger = null)
    {
        if (fileService == null)
            throw new ArgumentNullException(nameof(fileService));

        if (string.IsNullOrWhiteSpace(directoryPath))
            throw new ArgumentException("Directory Path cannot be null or empty.", nameof(directoryPath));

        if (!Directory.Exists(directoryPath))
            throw new DirectoryNotFoundException($"The directory '{directoryPath}' does not exist.");

        logger?
        .LogInformation("Starting to store files from directory: {DirectoryPath}", directoryPath);
        StoreFilesFromDirectoryRecursively(fileService, directoryPath, directoryPath, logger);
        logger?.LogInformation("Successfully stored all files from directory: {DirectoryPath}", directoryPath);
    }

    private static void StoreFilesFromDirectoryRecursively(FileService fileService, string path, string root, ILogger? logger = null)
    {
        string[] filePaths;
        try
        {
            filePaths = Directory.GetFiles(path);
            logger?.LogDebug("Found {FileCount} files in {Path}", filePaths.Length, path);
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Failed to read directory: {Path}", path);
            throw new InvalidOperationException($"Failed to read directory '{path}'.", ex);
        }

        foreach (var filePath in filePaths)
        {
            try
            {
                var fileName = Path.GetFileName(filePath);
                var contentType = MimeHelper.GetContentType(fileName);
                var fileInfo = new FileInfo(filePath);
                var content = File.ReadAllBytes(filePath);
                var relativePath = Path.GetRelativePath(root, filePath).Replace("\\", "/");
                logger?.LogDebug("Processing file: {FilePath} ({Length} bytes)", filePath, content.Length);
                var fileRecord = new FileRecord
                (
                    Id: 0,
                    Name: fileName,
                    Path: $"/{relativePath}",
                    ContentType: contentType,
                    Content: content,
                    Length: content.Length,
                    CreatedAt: fileInfo.CreationTimeUtc,
                    LastModifiedAt: fileInfo.LastWriteTimeUtc
                );

                fileService.SaveFile(fileRecord);
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Failed to store file: {FilePath}", filePath);
                throw new InvalidOperationException($"Failed to store file '{filePath}' in database.", ex);
            }
        }

        string[] directories;
        try
        {
            directories = Directory.GetDirectories(path);
            logger?.LogDebug("Found {DirectoryCount} subdirectories in {Path}", directories.Length, path);
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Failed to read subdirectories: {Path}", path);
            throw new InvalidOperationException($"Failed to read subdirectories of '{path}'.", ex);
        }

        foreach (var subDirectory in directories)
        {
            StoreFilesFromDirectoryRecursively(fileService, subDirectory, root, logger);
        }
    }
}