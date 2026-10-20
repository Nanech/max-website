using Microsoft.EntityFrameworkCore;
using PhotosApi.Helpers;
using PhotosApi.Infrastructure.Data;
using PhotosApi.Services;

namespace PhotosApi.Features.Pages;

public class PagesQueryService(
    PhotosDbContext dbContext,
    PhotoService photoService,
    ILogger<PagesQueryService> logger
    )
{

    public async Task<PageLayoutDto?> GetPageLayout(string pageKey, CancellationToken ct)
    {
        var pageData = await dbContext.ShowcasePages
            .AsNoTracking()
            .Where(p => p.PageKey == pageKey)
            .Select(p => new
            {
                PageKey = p.PageKey,
                Name = p.Name,
                Sections = p.Sections.OrderBy(s => s.DisplayOrder).Select(s => new
                {
                    SectionKey = s.SectionKey,
                    SectionType = s.SectionType,
                    
                    Items = s.Items.Select(i => new
                    {
                        ItemType = i.ShowcaseItemType,
                        AlbumId = i.AlbumId,
                        PhotoId = i.PhotoId
                    })
                    
                })

            })
            .FirstOrDefaultAsync(ct);
        
        if (pageData == null)
            return null;

        var albumIds = pageData.Sections
            .SelectMany(s => 
                s.Items.Where(i => i.ItemType == ShowcaseItemType.Album && i.AlbumId.HasValue))
            .Select(i => i.AlbumId!.Value).Distinct().ToList();

        var photosIds = pageData.Sections.
            SelectMany(s => 
                s.Items.Where(i => i.ItemType == ShowcaseItemType.Photo && i.PhotoId.HasValue))
            .Select(i => i.PhotoId!.Value).Distinct().ToList();
       
        var urls = await photoService.GetPhotosByIdsAsync(photosIds, ct);
        

        



    }
    
    
}