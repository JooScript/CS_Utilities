namespace Utils.SearchProvider.Models;

public sealed class ImageCandidate
{
    public string Title { get; init; } = "";
    public string SourceUrl { get; init; } = "";
    public string ImageUrl { get; init; } = "";
    public string Query { get; init; } = "";
    public int? Width { get; init; }
    public int? Height { get; init; }
    public string Provider { get; init; } = "";
}