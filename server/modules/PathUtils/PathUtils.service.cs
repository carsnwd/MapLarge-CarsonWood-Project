using Microsoft.Extensions.Configuration;

namespace AppProject.Modules.PathUtils
{

    public class PathUtilsService(IConfiguration configuration)
    {
        private readonly string _homeDirectory = configuration["HomeDirectory"] ?? @"C:\";

        public string GetSafePath(string relativePath)
        {
            if (string.IsNullOrEmpty(relativePath))
                return _homeDirectory;

            var fullPath = Path.Combine(_homeDirectory, relativePath);
            var normalizedPath = Path.GetFullPath(fullPath);

            if (!normalizedPath.StartsWith(_homeDirectory, StringComparison.OrdinalIgnoreCase))
            {
                throw new UnauthorizedAccessException("Access outside home directory is not allowed");
            }

            return normalizedPath;
        }

        public string? GetParentPath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return null;

            var parent = Path.GetDirectoryName(path);
            return parent?.Replace('\\', '/');
        }

        public string GetHomeDirectory()
        {
            return _homeDirectory;
        }
    }
}