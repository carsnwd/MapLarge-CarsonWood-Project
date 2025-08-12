public class DirectoryContents
{
    public string CurrentPath { get; set; } = string.Empty;
    public string? ParentPath { get; set; }
    public List<FileSystemItem> Directories { get; set; } = new();
    public List<FileSystemItem> Files { get; set; } = new();
}