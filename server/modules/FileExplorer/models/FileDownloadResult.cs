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