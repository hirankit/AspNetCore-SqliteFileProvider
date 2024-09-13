namespace AspNetCoreSqliteFileProviderLibrary;

public static class Utils
{
    public static async Task StoreFilesFromDirectoryAsync(FileService fileService, string directoryPath)
    {
        await StoreFileFromDirectoryRecursiveAsync(fileService, directoryPath, directoryPath);
    }

    private static async Task StoreFileFromDirectoryRecursiveAsync(FileService fileService, string path, string root)
    {
        var filePaths = Directory.GetFiles(path);

        foreach (var filePath in filePaths)
        {
            var fileName = Path.GetFileName(filePath);
            var contentType = MimeHelper.GetContentType(fileName);
            var content = await File.ReadAllBytesAsync(filePath);
            var relativePath = Path.GetRelativePath(root, filePath).Replace("\\", "/");
            var fileRecord = new FileRecord
            (
                Id: 0,
                Name: fileName,
                Path: $"/{relativePath}",
                ContentType: contentType,
                Content: content
            );

            await fileService.SaveFileAsync(fileRecord);
        }

        var directories = Directory.GetDirectories(path);
        foreach (var subDirectory in directories)
        {
            await StoreFileFromDirectoryRecursiveAsync(fileService, subDirectory, root);
        }
    }
}