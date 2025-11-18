namespace AspNetCoreSqliteFileProviderLibrary;

public static class Utils
{
    public static void StoreFilesFromDirectoryAsync(FileService fileService, string directoryPath)
    {
        if (fileService == null)
            throw new ArgumentNullException(nameof(fileService));

        if (string.IsNullOrWhiteSpace(directoryPath))
            throw new ArgumentException("Directory Path cannot be null or empty.", nameof(directoryPath));

        if (!Directory.Exists(directoryPath))
            throw new DirectoryNotFoundException($"The directory '{directoryPath}' does not exist.");

        StoreFilesFromDirectoryRecursively(fileService, directoryPath, directoryPath);
    }

    private static void StoreFilesFromDirectoryRecursively(FileService fileService, string path, string root)
    {
        string[] filePaths;
        try
        {
            filePaths = Directory.GetFiles(path);
        }
        catch (Exception ex)
        {
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
                throw new InvalidOperationException($"Failed to store file '{filePath}' in database.", ex);
            }
        }

        string[] directories;
        try
        {
            directories = Directory.GetDirectories(path);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to read subdirectories of '{path}'.", ex);
        }
        
        foreach (var subDirectory in directories)
        {
            StoreFilesFromDirectoryRecursively(fileService, subDirectory, root);
        }
    }
}