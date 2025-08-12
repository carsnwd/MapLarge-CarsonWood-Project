public class SearchData
{
    public string Query { get; set; } = string.Empty;
    public string SearchPath { get; set; } = string.Empty;
    public List<FileSystemItem> Results { get; set; } = new();
}