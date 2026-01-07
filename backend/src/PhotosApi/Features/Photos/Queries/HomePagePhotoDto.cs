namespace PhotosApi.Features.Photos.Queries;

public record HomePagePhotoDto
{
    public int PhotoId { get; init; }
    public required string PresignedUrl { get; set; }
}