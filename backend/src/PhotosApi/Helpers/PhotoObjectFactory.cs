
namespace PhotosApi.Helpers;

/// <summary>
/// Присваивание префиксов для пути
/// </summary>
public static class PhotoObjectFactory
{
    private const string OriginalPrefix = "original";
    private const string LargePrefix = "large";
    private const string PreviewPrefix = "preview";

    public static string BuildOriginalPath(Guid photoId) => Path.Combine(photoId.ToString(), $"{OriginalPrefix}.webp");
    public static string BuildLargePath(Guid photoId) => Path.Combine(photoId.ToString(), $"{LargePrefix}.webp");
    public static string BuildPreviewPath(Guid photoId) => Path.Combine(photoId.ToString(), $"{PreviewPrefix}.webp");

    public static string[] GetAllVersionsPaths(Guid photoId) =>
    [
        BuildOriginalPath(photoId),
        BuildLargePath(photoId),
        BuildPreviewPath(photoId)
    ];

    public static Dictionary<string, string> GetAllAsVersionDictionary(Guid photoId) => new()
    {
        { OriginalPrefix, BuildOriginalPath(photoId) },
        { LargePrefix, BuildLargePath(photoId) },
        { PreviewPrefix, BuildPreviewPath(photoId) }
    };
    
}