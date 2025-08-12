using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AppProject.Modules.FileExplorer
{
    public interface IFileExplorerService
    {
        Task<FileExplorerResult> BrowseDirectory(string path);
        Task<SearchResult> SearchByQuery(string query, string path = "");
        Task<FileDownloadResult> GetFileForDownload(string path);
    }

    public class FileExplorerService : IFileExplorerService
    {
        private readonly ILogger<FileExplorerService> _logger;
        private readonly IConfiguration _configuration;
        private readonly string _homeDirectory;

        public FileExplorerService(ILogger<FileExplorerService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            _homeDirectory = _configuration["HomeDirectory"] ?? @"C:\";
        }

        public Task<FileExplorerResult> BrowseDirectory(string path)
        {
            try
            {
                var fullPath = GetSafePath(path ?? "");

                if (!Directory.Exists(fullPath))
                {
                    return Task.FromResult(FileExplorerResult.NotFound($"Directory not found: {path}"));
                }

                var directoryInfo = new DirectoryInfo(fullPath);

                var directories = directoryInfo.GetDirectories()
                    .Select(d => new FileSystemItem
                    {
                        Name = d.Name,
                        Path = CombinePaths(path ?? "", d.Name),
                        LastModified = d.LastWriteTime,
                        Type = "directory"
                    }).ToList();

                var files = directoryInfo.GetFiles()
                    .Select(f => new FileSystemItem
                    {
                        Name = f.Name,
                        Path = CombinePaths(path ?? "", f.Name),
                        Size = f.Length,
                        LastModified = f.LastWriteTime,
                        Extension = f.Extension,
                        Type = "file"
                    }).ToList();

                var result = new DirectoryContents
                {
                    CurrentPath = path ?? "",
                    ParentPath = GetParentPath(path ?? ""),
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

        public Task<SearchResult> SearchByQuery(string query, string path = "")
        {
            try
            {
                if (string.IsNullOrWhiteSpace(query))
                {
                    return Task.FromResult(SearchResult.BadRequest("Search query cannot be empty"));
                }

                var searchResults = PerformSearch(query, path);

                return Task.FromResult(SearchResult.Success(new SearchData
                {
                    Query = query,
                    SearchPath = path,
                    Results = searchResults
                }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error performing search: {Query} in path: {Path}", query, path);
                return Task.FromResult(SearchResult.Error("Error performing search"));
            }
        }

        public Task<FileDownloadResult> GetFileForDownload(string path)
        {
            try
            {
                _logger.LogInformation("Downloading file: '{Path}'", path);
                var fullPath = GetSafePath(path);

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

        private List<FileSystemItem> PerformSearch(string query, string searchPath = "")
        {
            var results = new List<FileSystemItem>();
            try
            {
                // Determine the search directory
                var searchDirectory = string.IsNullOrEmpty(searchPath) ? _homeDirectory : GetSafePath(searchPath);

                _logger.LogInformation("Searching for '{Query}' in directory: '{SearchDirectory}'", query, searchDirectory);

                if (!Directory.Exists(searchDirectory))
                {
                    _logger.LogWarning("Search directory does not exist: '{SearchDirectory}'", searchDirectory);
                    return results;
                }

                var searchPattern = $"*{query}*";

                // Search for files
                var foundFiles = Directory.GetFiles(searchDirectory, searchPattern, SearchOption.AllDirectories);

                // Search for directories
                var foundDirectories = Directory.GetDirectories(searchDirectory, searchPattern, SearchOption.AllDirectories);

                // Process found files
                foreach (var file in foundFiles.Take(50)) // Limit file results
                {
                    var fileInfo = new FileInfo(file);
                    var relativePath = Path.GetRelativePath(_homeDirectory, file);

                    results.Add(new FileSystemItem
                    {
                        Name = fileInfo.Name,
                        Path = relativePath.Replace('\\', '/'),
                        Size = fileInfo.Length,
                        LastModified = fileInfo.LastWriteTime,
                        Extension = fileInfo.Extension,
                        Type = "file"
                    });
                }

                // Process found directories
                foreach (var directory in foundDirectories.Take(50)) // Limit directory results
                {
                    var dirInfo = new DirectoryInfo(directory);
                    var relativePath = Path.GetRelativePath(_homeDirectory, directory);

                    results.Add(new FileSystemItem
                    {
                        Name = dirInfo.Name,
                        Path = relativePath.Replace('\\', '/'),
                        LastModified = dirInfo.LastWriteTime,
                        Type = "directory"
                    });
                }

                _logger.LogInformation("Search found {Count} results", results.Count);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Access denied during search for query: {Query} in path: {Path}", query, searchPath);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error during search for query: {Query} in path: {Path}", query, searchPath);
            }

            return results.OrderBy(r => r.Type).ThenBy(r => r.Name).ToList();
        }

        private string GetSafePath(string relativePath)
        {
            if (string.IsNullOrEmpty(relativePath))
                return _homeDirectory;

            var fullPath = Path.Combine(_homeDirectory, relativePath);
            var normalizedPath = Path.GetFullPath(fullPath);

            // Ensure the path is within the home directory
            if (!normalizedPath.StartsWith(_homeDirectory, StringComparison.OrdinalIgnoreCase))
            {
                throw new UnauthorizedAccessException("Access outside home directory is not allowed");
            }

            return normalizedPath;
        }

        private static string? GetParentPath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return null;

            var parent = Path.GetDirectoryName(path);
            return parent?.Replace('\\', '/');
        }

        private static string CombinePaths(string basePath, string name)
        {
            if (string.IsNullOrEmpty(basePath))
                return name;

            return $"{basePath}/{name}".Replace('\\', '/');
        }
    }

    // Data models
    public class FileSystemItem
    {
        public string Name { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public long? Size { get; set; }
        public DateTime LastModified { get; set; }
        public string? Extension { get; set; }
        public string Type { get; set; } = string.Empty;
    }

    public class DirectoryContents
    {
        public string CurrentPath { get; set; } = string.Empty;
        public string? ParentPath { get; set; }
        public List<FileSystemItem> Directories { get; set; } = new();
        public List<FileSystemItem> Files { get; set; } = new();
    }

    public class SearchData
    {
        public string Query { get; set; } = string.Empty;
        public string SearchPath { get; set; } = string.Empty;
        public List<FileSystemItem> Results { get; set; } = new();
    }

    // Result classes
    public class FileExplorerResult
    {
        public bool IsSuccess { get; private set; }
        public string? ErrorMessage { get; private set; }
        public DirectoryContents? Data { get; private set; }
        public ResultType Type { get; private set; }

        public static FileExplorerResult Success(DirectoryContents data) =>
            new() { IsSuccess = true, Data = data, Type = ResultType.Success };

        public static FileExplorerResult NotFound(string message) =>
            new() { IsSuccess = false, ErrorMessage = message, Type = ResultType.NotFound };

        public static FileExplorerResult Unauthorized(string message) =>
            new() { IsSuccess = false, ErrorMessage = message, Type = ResultType.Unauthorized };

        public static FileExplorerResult Error(string message) =>
            new() { IsSuccess = false, ErrorMessage = message, Type = ResultType.Error };
    }

    public class SearchResult
    {
        public bool IsSuccess { get; private set; }
        public string? ErrorMessage { get; private set; }
        public SearchData? Data { get; private set; }
        public ResultType Type { get; private set; }

        public static SearchResult Success(SearchData data) =>
            new() { IsSuccess = true, Data = data, Type = ResultType.Success };

        public static SearchResult BadRequest(string message) =>
            new() { IsSuccess = false, ErrorMessage = message, Type = ResultType.BadRequest };

        public static SearchResult Error(string message) =>
            new() { IsSuccess = false, ErrorMessage = message, Type = ResultType.Error };
    }

    public class FileDownloadResult
    {
        public bool IsSuccess { get; private set; }
        public string? ErrorMessage { get; private set; }
        public Stream? FileStream { get; private set; }
        public string? ContentType { get; private set; }
        public string? FileName { get; private set; }
        public ResultType Type { get; private set; }

        public static FileDownloadResult Success(Stream fileStream, string contentType, string fileName) =>
            new() { IsSuccess = true, FileStream = fileStream, ContentType = contentType, FileName = fileName, Type = ResultType.Success };

        public static FileDownloadResult NotFound(string message) =>
            new() { IsSuccess = false, ErrorMessage = message, Type = ResultType.NotFound };

        public static FileDownloadResult Unauthorized(string message) =>
            new() { IsSuccess = false, ErrorMessage = message, Type = ResultType.Unauthorized };

        public static FileDownloadResult Error(string message) =>
            new() { IsSuccess = false, ErrorMessage = message, Type = ResultType.Error };
    }

    public enum ResultType
    {
        Success,
        NotFound,
        Unauthorized,
        BadRequest,
        Error
    }
}