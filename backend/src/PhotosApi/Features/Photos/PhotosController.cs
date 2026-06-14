using Microsoft.AspNetCore.Mvc;
using PhotosApi.Helpers;

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
        var url = await queries.GetPhotoUrlByIdAsync(id, ct);
        return Ok(new {url});
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

    [HttpPatch("{photoId:guid}/status")]
    public async Task<IActionResult> ChangePhotoCategory(Guid photoId, PhotoStatus newStatus, CancellationToken ct)
    {
        await commands.ChangePhotoStatusAsync(photoId, newStatus, ct);
        return Ok();
    }
}