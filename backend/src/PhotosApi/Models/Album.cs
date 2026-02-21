namespace PhotosApi.Models;

public class Album
{
    public Guid AlbumId { get; init; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; init; } = DateTime.Now;
    public short ShootYear { get; set; }
    
    public short? CategoryId { get; set; }
    public Category? Category { get; init; }
    
    public ICollection<Photo> Photos { get; set; } = [];
    
}