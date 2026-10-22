
using PhotosApi.Helpers;

namespace PhotosApi.Models;

public class ShowcaseItem
{
    public int Id { get; init; }
    
    public int SectionId { get; set; }
    public ShowcaseSection Section { get; set; } = null!;
    
    public ShowcaseItemType ShowcaseItemType { get; set; }
    
    public Guid? AlbumId { get; set; }
    public Album? Album { get; set; }

    public Guid? PhotoId { get; set; }
    public Photo? Photo { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    public int DisplayOrder { get; set; }
}