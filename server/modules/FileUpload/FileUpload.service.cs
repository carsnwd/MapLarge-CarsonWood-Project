using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AppProject.Modules.FileUpload
{
    public interface IFileUploadService
    {
        Task<FileUploadResult> UploadFile(string path, Stream fileStream, string fileName);
    }

    public class FileUploadService : IFileUploadService
    {
        private readonly ILogger<FileUploadService> _logger;
        private readonly IConfiguration _configuration;
        private readonly string _homeDirectory;
        private readonly long _maxFileSize;

        public FileUploadService(ILogger<FileUploadService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            _homeDirectory = _configuration["HomeDirectory"] ?? @"C:\";
            _maxFileSize = _configuration.GetValue<long>("MaxUploadFileSize", 10 * 1024 * 1024); // 10MB default
        }

        public async Task<FileUploadResult> UploadFile(string path, Stream fileStream, string fileName)
        {
            try
            {
                _logger.LogInformation("Uploading file: '{FileName}' to path: '{Path}'", fileName, path);

                // Validate file name
                var validationResult = ValidateFileName(fileName);
                if (!validationResult.IsValid)
                {
                    return FileUploadResult.BadRequest(validationResult.ErrorMessage!);
                }

                // Validate file size
                var sizeValidationResult = ValidateFileSize(fileStream.Length);
                if (!sizeValidationResult.IsValid)
                {
                    return FileUploadResult.BadRequest(sizeValidationResult.ErrorMessage!);
                }

                // Get and validate target directory
                var targetDirectory = GetSafePath(path ?? "");
                if (!Directory.Exists(targetDirectory))
                {
                    _logger.LogWarning("Target directory does not exist: '{TargetDirectory}'", targetDirectory);
                    return FileUploadResult.NotFound($"Directory not found: {path}");
                }

                // Create and validate target file path
                var targetFilePath = Path.Combine(targetDirectory, fileName);
                var pathValidationResult = ValidateTargetPath(targetFilePath, fileName);
                if (!pathValidationResult.IsValid)
                {
                    return pathValidationResult.ResultType switch
                    {
                        FileUploadValidationType.BadRequest => FileUploadResult.BadRequest(pathValidationResult.ErrorMessage!),
                        FileUploadValidationType.Unauthorized => FileUploadResult.Unauthorized(pathValidationResult.ErrorMessage!),
                        _ => FileUploadResult.Error(pathValidationResult.ErrorMessage!)
                    };
                }

                // Write file to disk
                await WriteFileAsync(fileStream, targetFilePath);

                _logger.LogInformation("File uploaded successfully: '{TargetFilePath}'", targetFilePath);

                // Return the created file info
                var uploadedFile = CreateFileSystemItem(targetFilePath);
                return FileUploadResult.Success(uploadedFile);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Access denied during file upload: {FileName} to {Path}", fileName, path);
                return FileUploadResult.Unauthorized("Access denied to the specified directory");
            }
            catch (IOException ex)
            {
                _logger.LogError(ex, "IO error during file upload: {FileName} to {Path}", fileName, path);
                return FileUploadResult.Error("Error writing file to disk");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading file: {FileName} to {Path}", fileName, path);
                return FileUploadResult.Error("Error uploading file");
            }
        }

        private ValidationResult ValidateFileName(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return ValidationResult.Invalid("File name cannot be empty");
            }

            if (HasInvalidFileNameCharacters(fileName))
            {
                return ValidationResult.Invalid("File name contains invalid characters");
            }

            return ValidationResult.Valid();
        }

        private ValidationResult ValidateFileSize(long fileSize)
        {
            if (fileSize > _maxFileSize)
            {
                return ValidationResult.Invalid($"File size exceeds maximum allowed size of {_maxFileSize / (1024 * 1024)}MB");
            }

            return ValidationResult.Valid();
        }

        private FileUploadValidationResult ValidateTargetPath(string targetFilePath, string fileName)
        {
            // Check if file already exists
            if (File.Exists(targetFilePath))
            {
                return FileUploadValidationResult.BadRequest($"File '{fileName}' already exists in this directory");
            }

            // Ensure the target file path is still within the home directory (security check)
            var normalizedTargetPath = Path.GetFullPath(targetFilePath);
            if (!normalizedTargetPath.StartsWith(_homeDirectory, StringComparison.OrdinalIgnoreCase))
            {
                return FileUploadValidationResult.Unauthorized("Invalid file path");
            }

            return FileUploadValidationResult.Valid();
        }

        private async Task WriteFileAsync(Stream sourceStream, string targetFilePath)
        {
            using var targetStream = new FileStream(targetFilePath, FileMode.Create, FileAccess.Write);
            await sourceStream.CopyToAsync(targetStream);
        }

        private FileSystemItem CreateFileSystemItem(string filePath)
        {
            var fileInfo = new FileInfo(filePath);
            var relativePath = Path.GetRelativePath(_homeDirectory, filePath);

            return new FileSystemItem
            {
                Name = fileInfo.Name,
                Path = relativePath.Replace('\\', '/'),
                Size = fileInfo.Length,
                LastModified = fileInfo.LastWriteTime,
                Extension = fileInfo.Extension,
                Type = "file"
            };
        }

        private static bool HasInvalidFileNameCharacters(string fileName)
        {
            var invalidChars = Path.GetInvalidFileNameChars();
            return fileName.IndexOfAny(invalidChars) >= 0;
        }

        private string GetSafePath(string relativePath)
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
    }
}