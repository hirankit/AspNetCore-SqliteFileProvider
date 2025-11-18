namespace AspNetCoreSqliteFileProviderLibrary;

public static class Utils
{
    public static void StoreFilesFromDirectoryAsync(FileService fileService, string directoryPath)
    {
        StoreFilesFromDirectoryRecursively(fileService, directoryPath, directoryPath);
    }

    private static void StoreFilesFromDirectoryRecursively(FileService fileService, string path, string root)
    {
        var filePaths = Directory.GetFiles(path);

        foreach (var filePath in filePaths)
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

        var directories = Directory.GetDirectories(path);
        foreach (var subDirectory in directories)
        {
            StoreFilesFromDirectoryRecursively(fileService, subDirectory, root);
        }
    }
}