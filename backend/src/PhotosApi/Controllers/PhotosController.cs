using MediatR;
using Microsoft.AspNetCore.Mvc;
using PhotosApi.Features.Photos.Commands;
using PhotosApi.Features.Photos.Queries;

namespace PhotosApi.Controllers;

[ApiController]
[Route("api/photos")]
public class PhotosController(IMediator mediator) : ControllerBase
{
    [HttpDelete("{id:guid}")]
    public async Task<IResult> DeletePhotoById(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            await mediator.Send(new DeletePhotoCommand(id), cancellationToken);
            return Results.Ok();
        }
        catch (FileNotFoundException) { return Results.NotFound(); }
        catch (Exception e) { return Results.Problem(e.Message); }
    }

    [HttpGet("{id:guid}/url")]
    public async Task<IResult> GetPhotoUrlById(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var url = await mediator.Send(new GetPhotoUrlByIdQuery(id), cancellationToken);
            return Results.Ok(url);
        }
        catch (FileNotFoundException) { return Results.NotFound(); }
        catch (Exception e) { return Results.Problem(e.Message); }
    }
    
    [HttpPost("upload")]
    public async Task<IResult> UploadPhoto([FromForm] UploadPhotoCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var photoId = await mediator.Send(request, cancellationToken);
            return Results.Created($"api/photos{photoId}", photoId);
        }
        catch (Exception e) { return Results.Problem(e.Message); }
    }

    [HttpPatch("change-category")]
    public async Task<IResult> ChangePhotoCategory([FromBody] ChangePhotoCategoryCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            await mediator.Send(request, cancellationToken);
            return Results.Ok();
        }
        catch (Exception e)
        {
            return Results.Problem(e.Message);
        }
    }
}