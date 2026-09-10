namespace PhotosApi.Models;

public class ShowcasePage
{
    public int Id { get; init; }
    public string Name { get; set; } = string.Empty;
    public string PageKey { get; set; } = string.Empty;

    public List<ShowcaseSection> Sections { get; set; } = [];
}