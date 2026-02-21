using PhotosApi.Helpers;

namespace PhotosApi.Models;

public class Photo
{
    public Guid PhotoId { get; init; }
    public string S3Path { get; set; } = null!;
    public DateTime UploadedAt { get; init; }
    public PhotoStatus Status { get; set; } = PhotoStatus.Draft;
    public Guid AlbumId { get; init; }
    public Album Album { get; set; } = null!;
}