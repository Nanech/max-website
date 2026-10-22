using Microsoft.EntityFrameworkCore;
using PhotosApi.Features.Albums;
using PhotosApi.Features.Photos;
using PhotosApi.Helpers;
using PhotosApi.Infrastructure.Data;
using PhotosApi.Services;

namespace PhotosApi.Features.Pages;

public class PagesQueryService(
    PhotosDbContext dbContext,
    PhotoService photoService,
    AlbumsQueryService albumsQueryService,
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
                p.PageKey,
                p.Name,
                Sections = p.Sections.OrderBy(s => s.DisplayOrder).Select(s => new
                {
                    s.SectionKey,
                    s.SectionType,
                    
                    Items = s.Items.Select(i => new
                    {
                        ItemType = i.ShowcaseItemType,
                        i.AlbumId,
                        i.PhotoId
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

        var albumsInfo = await albumsQueryService.GetAlbumsByIdsAsync(albumIds, ct);
        var albumDict = albumsInfo.ToDictionary(a => a.AlbumId);
        
        var photosIds = pageData.Sections.
            SelectMany(s => 
                s.Items.Where(i => i.ItemType == ShowcaseItemType.Photo && i.PhotoId.HasValue))
            .Select(i => i.PhotoId!.Value).Distinct().ToList();

        var coverPhotosIds = albumsInfo
            .Where(a => a.CoverPhoto.HasValue)
            .Select(a => a.CoverPhoto!.Value)
            .Distinct().ToArray();
        
        if (coverPhotosIds.Length != 0) photosIds.AddRange(coverPhotosIds);
        
        // todo: may it bee too expensive for network to take all variants of photos here 
        var photoUrls = await photoService.GetPhotosByIdsAsync(photosIds, ct);
        var photoDict = photosIds.Zip(photoUrls)
            .ToDictionary(p => p.First,  p => p.Second);
        
        var sectionDtos = new List<SectionDto>();

        foreach (var section in pageData.Sections)
        {
            var itemDtos = new List<ShowcaseItemDto>();

            foreach (var item in section.Items)
            {
                if (item.ItemType == ShowcaseItemType.Album && item.AlbumId.HasValue &&
                    albumDict.TryGetValue(item.AlbumId.Value, out var albumCard)
                    )
                {
                    PhotoUrlDto? coverPhotoUrlDto = null;
                    if (albumCard.CoverPhoto.HasValue
                        && photoDict.TryGetValue(albumCard.CoverPhoto.Value, out var coverPhoto))
                    {
                        coverPhotoUrlDto = coverPhoto;
                    }
                    
                    itemDtos.Add(new AlbumShowcaseItemDto(albumCard, coverPhotoUrlDto));
                }
                else if (item.ItemType == ShowcaseItemType.Photo && item.PhotoId.HasValue &&
                         photoDict.TryGetValue(item.PhotoId.Value, out var photoUrl)
                        )
                {
                    itemDtos.Add(new PhotoShowcaseItemDto(photoUrl));
                }
            }
            
            sectionDtos.Add(new SectionDto(section.SectionKey, section.SectionType.Name, [.. itemDtos]));
        }
        
        return new PageLayoutDto(pageData.PageKey, pageData.Name, sectionDtos);
    }
    
    
}