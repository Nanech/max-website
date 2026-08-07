using Microsoft.AspNetCore.Mvc;

namespace PhotosApi.Features.Photos;

[ApiController]
[Route("api/photos")]
public class PhotosController(
    PhotosCommandService commands,
    PhotosQueryService queries
) : ControllerBase
{
    [HttpGet("{id:guid}/url")]
    public async Task<IActionResult> GetPhotoUrlById(Guid id, CancellationToken ct)
    {
        var photoUrls = await queries.GetPhotoUrlByIdAsync(id, ct);
        return Ok(new {photoUrls});
    }

    [HttpGet("/albumId/{albumId:guid}")]
    public async Task<IActionResult> GetPhotosByAlbumAsync(Guid albumId, CancellationToken ct)
    {
        var photosUrls = await queries.GetPhotosUrlsByAlbumAsync(albumId, ct);
        return Ok(new {photosUrls});
    }
    
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeletePhotoById(Guid id, CancellationToken ct)
    {
        await commands.DeletePhotoByIdAsync(id, ct);
        return NoContent();
    }
    
    [HttpPost("upload")]
    public async Task<IActionResult> UploadPhoto([FromForm] UploadPhotoRequest request, CancellationToken ct)
    {
        var photoId = await commands.UploadPhotoAsync(request, ct);
        return Created($"api/photos{photoId}", new { id = photoId });
    }

}