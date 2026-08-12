using PhotosApi.Helpers;

namespace PhotosApi.Models;

public class Photo
{
    public Guid PhotoId { get; init; }
    public DateTime UploadedAt { get; init; }
    public Guid AlbumId { get; init; }
    public Album Album { get; set; } = null!;
}