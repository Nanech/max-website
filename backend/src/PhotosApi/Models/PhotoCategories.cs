namespace PhotosApi.Models;

public class PhotoCategories
{
    public Guid PhotoId { get; init; }
    public short CategoryId { get; set; }
    
    public Photo Photo { get; set; } = null!;
    public Category Category { get; set; } = null!;
}