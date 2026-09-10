using Microsoft.EntityFrameworkCore;
using PhotosApi.Helpers;
using PhotosApi.Infrastructure.Data;
using PhotosApi.Models;

namespace PhotosApi.Features.AdminShowcase;

public class AdminCommandService(
    PhotosDbContext dbContext,
    ILogger<AdminCommandService> logger
    )
{

    public async Task<int> CreateShowcaseItem(CreateShowcaseItemCommand request, CancellationToken ct)
    {
        logger.LogInformation("Создание section: {Request}", request);
        
        var sectionExists = await dbContext.ShowcaseSections
            .AnyAsync(si => si.Id == request.SectionId, ct);
        if (sectionExists)
            throw new ArgumentException($"Section with id: {request.SectionId} not found");
        
        ShowcaseItemType itemType;
        if (request.AlbumId.HasValue && request.AlbumId.Value != Guid.Empty)
        {
            var albumExists = await dbContext.Albums.AnyAsync(a => a.AlbumId == request.AlbumId.Value, ct);
            if (!albumExists)
                throw new ArgumentException($"Album with id: {request.AlbumId} not found");
            itemType = ShowcaseItemType.Album;
        }
        else if (request.PhotoId.HasValue && request.PhotoId.Value != Guid.Empty)
        {
            var photoExists = await dbContext.Photos.AnyAsync(p => p.PhotoId == request.PhotoId.Value, ct);
            if (!photoExists)
                throw new ArgumentException($"Photo with id: {request.PhotoId} not found");
            itemType = ShowcaseItemType.Photo;
        }
        else 
            throw new ArgumentException("Either Album or PhotoId must be provided");
        
        var showcaseItem = new ShowcaseItem
        {
            SectionId = request.SectionId,
            ShowcaseItemType =  itemType,
            AlbumId = itemType == ShowcaseItemType.Album ? request.AlbumId : null,
            PhotoId = itemType == ShowcaseItemType.Photo ? request.PhotoId : null,
            CreatedAt = DateTime.UtcNow
        };
        await dbContext.AddAsync(showcaseItem, ct);
        await dbContext.SaveChangesAsync(ct);
        
        logger.LogInformation("Создан ShowcaseItem с ID: {Id} для секции {SectionId}", showcaseItem.Id, request.SectionId);
        
        return showcaseItem.Id;
    }
    
    public async Task<int> CreateShowcaseSection(CreateShowcaseSectionCommand request, CancellationToken ct)
    {
        logger.LogInformation("Создание section: {Request}", request);

        var page = await dbContext.ShowcasePages.FirstOrDefaultAsync(sp => sp.Id == request.PageId, ct);
        if (page == null)
            throw new ArgumentException($"PageId: {request.PageId} not found");

        var showcaseSectionType = await dbContext.ShowcaseSectionTypes
            .FirstOrDefaultAsync(si => si.Id == request.ShowcaseSectionTypeId, ct);
        if (showcaseSectionType == null)
            throw new ArgumentException($"ShowcaseSectionType with id: {request.ShowcaseSectionTypeId} not found");

        var keyExists = await dbContext.ShowcaseSections
            .AnyAsync(sc => sc.PageId == request.PageId && sc.SectionKey == request.SectionKey , ct);
        if (keyExists)
            throw new ArgumentException($"Section key: {request.SectionKey} is already exists on this page");
        
        var maxOrder = await dbContext.ShowcaseSections.Where(sc => sc.PageId == request.PageId)
            .MaxAsync(sc => (int?)sc.DisplayOrder, ct) ?? 0;
        var displayOrder = maxOrder + 10;
        
        var showcaseSection = new ShowcaseSection
        {
            PageId = page.Id,
            ShowcaseSectionTypeId = showcaseSectionType.Id,
            DisplayOrder = displayOrder,
            Name = request.Name,
            SectionKey =  request.SectionKey
        };
        await dbContext.AddAsync(showcaseSection, ct);
        await dbContext.SaveChangesAsync(ct);
        
        return showcaseSection.Id;
    }
    
    
}