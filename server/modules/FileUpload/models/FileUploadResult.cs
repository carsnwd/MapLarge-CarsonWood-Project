public class FileUploadResult
{
    public bool IsSuccess { get; private set; }
    public string? ErrorMessage { get; private set; }
    public FileSystemItem? Data { get; private set; }
    public ResultType Type { get; private set; }

    public static FileUploadResult Success(FileSystemItem data) =>
        new() { IsSuccess = true, Data = data, Type = ResultType.Success };

    public static FileUploadResult NotFound(string message) =>
        new() { IsSuccess = false, ErrorMessage = message, Type = ResultType.NotFound };

    public static FileUploadResult Unauthorized(string message) =>
        new() { IsSuccess = false, ErrorMessage = message, Type = ResultType.Unauthorized };

    public static FileUploadResult BadRequest(string message) =>
        new() { IsSuccess = false, ErrorMessage = message, Type = ResultType.BadRequest };

    public static FileUploadResult Error(string message) =>
        new() { IsSuccess = false, ErrorMessage = message, Type = ResultType.Error };
}