using Microsoft.Extensions.Logging;
using AppProject.Modules.FileUpload;
using AppProject.Modules.PathUtils;
using System.IO;


namespace AppProject.Modules.FileExplorer
{
    public interface IFileExplorerService
    {
        Task<FileExplorerResult> BrowseDirectory(string path);
        Task<FileDownloadResult> GetFileForDownload(string path);
    }

    public class FileExplorerService : IFileExplorerService
    {
        private readonly ILogger<FileExplorerService> _logger;
        private readonly PathUtilsService _pathUtilsService;

        public FileExplorerService(ILogger<FileExplorerService> logger, PathUtilsService pathUtilsService)
        {
            _logger = logger;
            _pathUtilsService = pathUtilsService;
        }

        public Task<FileExplorerResult> BrowseDirectory(string path)
        {
            try
            {
                var fullPath = _pathUtilsService.GetSafePath(path ?? "");

                if (!Directory.Exists(fullPath))
                {
                    return Task.FromResult(FileExplorerResult.NotFound($"Directory not found: {path}"));
                }

                var directoryInfo = new DirectoryInfo(fullPath);

                var directories = GetDirectoriesFromDirectoryInfo(directoryInfo);

                var files = GetFilesFromDirectoryInfo(directoryInfo);

                var result = new DirectoryContents
                {
                    CurrentPath = path ?? "",
                    ParentPath = _pathUtilsService.GetParentPath(path ?? ""),
                    Directories = directories,
                    Files = files
                };

                return Task.FromResult(FileExplorerResult.Success(result));
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Access denied to directory: {Path}", path);
                return Task.FromResult(FileExplorerResult.Unauthorized("Access denied to the specified directory"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error browsing directory: {Path}", path);
                return Task.FromResult(FileExplorerResult.Error("Error accessing directory"));
            }
        }

        public Task<FileDownloadResult> GetFileForDownload(string path)
        {
            try
            {
                _logger.LogInformation("Downloading file: '{Path}'", path);
                var fullPath = _pathUtilsService.GetSafePath(path);

                if (!System.IO.File.Exists(fullPath))
                {
                    _logger.LogWarning("File not found: '{FullPath}'", fullPath);
                    return Task.FromResult(FileDownloadResult.NotFound($"File not found: {path}"));
                }

                var fileInfo = new FileInfo(fullPath);
                var contentType = GetContentType(fileInfo.Extension);
                var fileStream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read);

                return Task.FromResult(FileDownloadResult.Success(fileStream, contentType, fileInfo.Name));
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Access denied to file: {Path}", path);
                return Task.FromResult(FileDownloadResult.Unauthorized("Access denied to the specified file"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading file: {Path}", path);
                return Task.FromResult(FileDownloadResult.Error("Error accessing file"));
            }
        }


        private List<FileSystemItem> GetFilesFromDirectoryInfo(DirectoryInfo directoryInfo)
        {
            return directoryInfo.GetFiles()
                .Select(f => new FileSystemItem
                {
                    Name = f.Name,
                    Path = f.FullName,
                    Size = f.Length,
                    LastModified = f.LastWriteTime,
                    Extension = f.Extension,
                    Type = "file"
                }).ToList();
        }

        private List<FileSystemItem> GetDirectoriesFromDirectoryInfo(DirectoryInfo directoryInfo)
        {
            return directoryInfo.GetDirectories()
                .Select(d => new FileSystemItem
                {
                    Name = d.Name,
                    Path = d.FullName,
                    LastModified = d.LastWriteTime,
                    Type = "directory"
                }).ToList();
        }

        private static string GetContentType(string extension)
        {
            return extension.ToLowerInvariant() switch
            {
                ".txt" => "text/plain",
                ".pdf" => "application/pdf",
                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".xls" => "application/vnd.ms-excel",
                ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                ".png" => "image/png",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".gif" => "image/gif",
                ".csv" => "text/csv",
                ".json" => "application/json",
                ".xml" => "application/xml",
                ".zip" => "application/zip",
                _ => "application/octet-stream"
            };
        }
    }
}