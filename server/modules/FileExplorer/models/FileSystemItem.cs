public class FileSystemItem
{
    public string Name { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public long? Size { get; set; }
    public DateTime LastModified { get; set; }
    public string? Extension { get; set; }
    public string Type { get; set; } = string.Empty;
}