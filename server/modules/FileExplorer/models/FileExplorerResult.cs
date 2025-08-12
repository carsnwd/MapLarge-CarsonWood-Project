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