using Microsoft.AspNetCore.Mvc;

namespace PhotosApi.Features.Pages;

[ApiController]
[Route("api/pages")]
public class PagesController(
    PagesQueryService queryService
    ) : ControllerBase
{
    [HttpGet("{pageKey}")]
    public async Task<IActionResult> GetPageLayout(string pageKey, CancellationToken ct)
    {
        var page = await queryService.GetPageLayout(pageKey, ct);
        return Ok(page);
    }
}