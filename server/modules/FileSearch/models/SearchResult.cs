
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