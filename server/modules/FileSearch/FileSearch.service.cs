using Microsoft.Extensions.Logging;
using AppProject.Modules.PathUtils;

namespace AppProject.Modules.FileSearch
{
    public interface IFileSearchService
    {
        Task<SearchResult> SearchFilesByQuery(string query, string path = "");
    }

    public class FileSearchService : IFileSearchService
    {
        private readonly ILogger<FileSearchService> _logger;
        private readonly PathUtilsService _pathUtilsService;

        public FileSearchService(ILogger<FileSearchService> logger, PathUtilsService pathUtilsService)
        {
            _logger = logger;
            _pathUtilsService = pathUtilsService;
        }

        public async Task<SearchResult> SearchFilesByQuery(string query, string path = "")
        {
            try
            {
                if (string.IsNullOrWhiteSpace(query))
                {
                    return SearchResult.BadRequest("Search query cannot be empty");
                }

                var searchResults = await PerformSearchAsync(query, path);

                return SearchResult.Success(new SearchData
                {
                    Query = query,
                    SearchPath = path,
                    Results = searchResults
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error performing search: {Query} in path: {Path}", query, path);
                return SearchResult.Error("Error performing search");
            }
        }

        private async Task<List<FileSystemItem>> PerformSearchAsync(string query, string searchPath = "")
        {
            var results = new List<FileSystemItem>();

            await Task.Run(() =>
            {
                try
                {
                    var searchDirectory = _pathUtilsService.GetSafePath(searchPath);

                    _logger.LogInformation("Searching for '{Query}' in directory: '{SearchDirectory}'", query, searchDirectory);

                    if (!Directory.Exists(searchDirectory))
                    {
                        _logger.LogWarning("Search directory does not exist: '{SearchDirectory}'", searchDirectory);
                        return;
                    }

                    var searchPattern = $"*{query}*";

                    var foundFiles = Directory.GetFiles(searchDirectory, searchPattern, SearchOption.AllDirectories);
                    var foundDirectories = Directory.GetDirectories(searchDirectory, searchPattern, SearchOption.AllDirectories);

                    foreach (var file in foundFiles.Take(50))
                    {
                        var fileInfo = new FileInfo(file);
                        var relativePath = Path.GetRelativePath(_pathUtilsService.GetHomeDirectory(), file);

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

                    foreach (var directory in foundDirectories.Take(50))
                    {
                        var dirInfo = new DirectoryInfo(directory);
                        var relativePath = Path.GetRelativePath(_pathUtilsService.GetHomeDirectory(), directory);

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
            });

            return results.OrderBy(r => r.Type).ThenBy(r => r.Name).ToList();
        }
    }
}