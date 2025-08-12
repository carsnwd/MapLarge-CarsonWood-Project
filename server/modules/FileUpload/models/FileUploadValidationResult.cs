public class FileUploadValidationResult
{
    public bool IsValid { get; private set; }
    public string? ErrorMessage { get; private set; }
    public FileUploadValidationType ResultType { get; private set; }

    public static FileUploadValidationResult Valid() =>
        new() { IsValid = true };

    public static FileUploadValidationResult BadRequest(string errorMessage) =>
        new() { IsValid = false, ErrorMessage = errorMessage, ResultType = FileUploadValidationType.BadRequest };

    public static FileUploadValidationResult Unauthorized(string errorMessage) =>
        new() { IsValid = false, ErrorMessage = errorMessage, ResultType = FileUploadValidationType.Unauthorized };

    public static FileUploadValidationResult Error(string errorMessage) =>
        new() { IsValid = false, ErrorMessage = errorMessage, ResultType = FileUploadValidationType.Error };
}