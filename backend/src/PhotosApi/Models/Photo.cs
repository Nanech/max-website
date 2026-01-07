namespace PhotosApi.Models;

public class Photo
{
    public Guid PhotoId { get; init; }
    public string S3FilePath { get; set; } = null!;
    public DateTime UploadedAt { get; init; }
    public short ShootYear { get; set; }
    public ICollection<PhotoCategories> PhotosToCategory { get; set; } = [];
}