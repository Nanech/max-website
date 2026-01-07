using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using PhotosApi.Helpers;
using PhotosApi.Infrastructure.Data;
using PhotosApi.Models;

namespace PhotosApi.Services;

public class CategoryService(
    PhotosDbContext dbContext,
    IMemoryCache cache
    )
{
    private const string CategoriesCacheKey = "CategoriesCacheKey";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(24);

    public async Task<List<Category>> GetCategoriesByTypeAsync(
        List<CategoryType> categoryTypes,
        CancellationToken cancellationToken
    )
    {
        var allCategories = await GetAllCategories(cancellationToken);
        
        var foundCategories = allCategories
            .Where(c => categoryTypes.Contains(c.CategoryType))
            .ToList();

        if (foundCategories.Count == categoryTypes.Count) return foundCategories;
        
        var foundTypes = foundCategories.Select(c => c.CategoryType).ToHashSet();
        var missing = categoryTypes.Except(foundTypes).ToList();
            
        throw new InvalidOperationException(
            $"Не найдены категории для типов: {string.Join(", ", missing)}"
        );
    }
    
    /// <summary>
    /// Получаем все категории из кэша или БД
    /// </summary>
    /// <param name="token">CancellationToken</param>
    /// <returns></returns>
    private async Task<List<Category>> GetAllCategories(CancellationToken token)
    {
        var allCategories = await cache.GetOrCreateAsync(
                CategoriesCacheKey,
                async entry =>
                {
                    entry.AbsoluteExpirationRelativeToNow = CacheDuration;
                    return await dbContext.Categories.ToListAsync(token);
                }
            );
        
        return allCategories ?? [];
    }
    
    public void InvalidateCachce() => cache.Remove(CategoriesCacheKey);
}