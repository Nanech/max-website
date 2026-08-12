using Microsoft.AspNetCore.Mvc;

namespace PhotosApi.Features.Photos;

[ApiController]
[Route("api/photos")]
public class PhotosController(
    PhotosCommandService commands,
    PhotosQueryService queries
) : ControllerBase
{
    [HttpGet("{id:guid}/urls")]
    public async Task<IActionResult> GetUrls(Guid id, CancellationToken ct)
    {
        var urls = await queries.GetPhotoUrlByIdAsync(id, ct);
        return Ok(urls);
    }

    [HttpGet("/album/{albumId:guid}")]
    public async Task<IActionResult> GetByAlbum(Guid albumId, CancellationToken ct)
    {
        var photosUrls = await queries.GetPhotosUrlsByAlbumAsync(albumId, ct);
        return Ok(new {photosUrls});
    }
    
    [HttpDelete("{photoIds:guid}")]
    public async Task<IActionResult> Delete(List<Guid> photoIds, CancellationToken ct)
    {
        await commands.DeletePhotoByIdAsync(photoIds, ct);
        return NoContent();
    }
    
    [HttpPost("upload")]
    public async Task<IActionResult> Upload([FromForm] UploadPhotoRequest request, CancellationToken ct)
    {
        var photoIds = await commands.UploadPhotoAsync(request, ct);

        return CreatedAtAction(
            nameof(GetByAlbum),
            new { albumId = request.AlbumId },
            new { ods = photoIds }
        );
    }

}