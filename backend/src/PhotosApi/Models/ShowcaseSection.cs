namespace PhotosApi.Models;

public class ShowcaseSection
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string SectionKey { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    
    public int PageId { get; set; }
    public ShowcasePage Page { get; set; } = null!;
    
    public int ShowcaseSectionTypeId { get; set; }
    public ShowcaseSectionType SectionType { get; set; } = null!;

    public List<ShowcaseItem> Items { get; set; } = [];
}