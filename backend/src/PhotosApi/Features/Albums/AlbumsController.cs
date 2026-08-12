using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace PhotosApi.Features.Albums;

[ApiController]
[Route("api/albums")]
public class AlbumsController(
    AlbumsQueryService queries,
    AlbumsCommandService commands
    ) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAlbumsByFiltersAsync(
        [FromQuery] GetAlbumsQuery query,
        [FromServices] IValidator<GetAlbumsQuery> validator,
        CancellationToken ct
    )
    {
        var validationResult = await validator.ValidateAsync(query, ct);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);
        
        var albums = await queries.GetAlbumsWithFiltersAsync(query, ct);
        return Ok(albums);
    }
    
    [HttpPost]
    public async Task<IActionResult> CreateAlbum(
        [FromBody] CreateAlbumCommand command,
        [FromServices] IValidator<CreateAlbumCommand> validator,
        CancellationToken ct
    )
    {
        var validationResult = await validator.ValidateAsync(command, ct);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);
        
        var albumId = await commands.CreateAlbumAsync(command, ct);
        return Created($"api/albums/{albumId}", new { albumId });
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAlbum(Guid id, CancellationToken ct)
    {
        await commands.DeleteAlbumByIdAsync(id, ct);
        return NoContent();
    }
    
}