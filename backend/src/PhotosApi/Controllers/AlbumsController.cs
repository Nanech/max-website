using MediatR;
using Microsoft.AspNetCore.Mvc;
using PhotosApi.Features.Albums.Commands;
using PhotosApi.Features.Albums.Queries;

namespace PhotosApi.Controllers;

[ApiController]
[Route("api/albums")]
public class AlbumsController(IMediator mediator) : Controller
{
    [HttpGet]
    public async Task<IResult> GetAlbums(
        [FromQuery] short? categoryId,
        CancellationToken ct
        )
    {
        try
        {
            var albums = await mediator.Send(new GetAlbumsQuery(categoryId), ct);
            return Results.Ok(albums);
        }
        catch (Exception e)
        {
            return Results.Problem(e.Message);
        }
    }

    [HttpPost]
    public async Task<IResult> CreateAlbum(
        [FromBody] CreateAlbumCommand command,
        CancellationToken ct
        )
    {
        try
        {
            var albumId = await mediator.Send(command, ct);
            return Results.Created($"api/albums/{albumId}", new { albumId });
        }
        catch (InvalidOperationException e)
        {
            return Results.BadRequest(new {error = e.Message});
        }
        catch (Exception e)
        {
            return Results.Problem(e.Message);
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IResult> DeleteAlbum(Guid id, CancellationToken ct)
    {
        try
        {
            await mediator.Send(new DeleteAlbumCommand(id), ct);
            return Results.Ok(new { message = "Album deleted successfully" });
        }
        catch (KeyNotFoundException)
        {
            return Results.NotFound(new { error = $"Album with id {id} not found" });
        }
        catch (Exception e)
        {
            return Results.Problem(e.Message);
        }
    }
    
        
    
}