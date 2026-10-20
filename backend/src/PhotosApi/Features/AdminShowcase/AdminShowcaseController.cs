using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace PhotosApi.Features.AdminShowcase;

[ApiController]
[Route("api/admin")]
public class AdminShowcaseController(
    AdminShowcaseCommandService showcaseCommandService
    ) : ControllerBase
{
    
    [HttpPost("showcaseItem")]
    public async Task<IActionResult> CreateShowcaseItem(
        [FromBody] CreateShowcaseItemCommand command,
        [FromServices] IValidator<CreateShowcaseItemCommand> validator,
        CancellationToken ct
    )
    {
        var validationResult = await validator.ValidateAsync(command, ct);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);
        
        var showcaseItem = await showcaseCommandService.CreateShowcaseItem(command, ct);
        return Ok(showcaseItem);
    }

    [HttpPost("showcaseSection")]
    public async Task<IActionResult> CreateShowcaseSection(
        [FromBody] CreateShowcaseSectionCommand command,
        [FromServices] IValidator<CreateShowcaseSectionCommand> validator,
        CancellationToken ct
        )
    {
        var validationResult = await validator.ValidateAsync(command, ct);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);
        
        var showcaseSection = await showcaseCommandService.CreateShowcaseSection(command, ct);
        return Ok(showcaseSection);
    }
    
}