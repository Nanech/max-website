namespace PhotosApi.Helpers;

/// <summary>
/// Присваивание префиксов для пути
/// </summary>
public static class StoragePaths
{
    private const string UploadsPrefix = "uploads";
    private const string GalleryPrefix = "gallery";
    private const string HomepagePrefix = "homepage";
    private const string ThumbnailsPrefix = "thumbnails";
    private const string ArchivedPrefix = "archived";

    public static string [] GetPublicPrefixes => [ GalleryPrefix, HomepagePrefix, ThumbnailsPrefix];
    public static string [] GetPrivatePrefixes => [ UploadsPrefix, ArchivedPrefix ];
    
    public static string GetPrefixForStatus(PhotoStatus status)
    {
        return status switch
        {
            PhotoStatus.Draft => UploadsPrefix,
            PhotoStatus.Published => GalleryPrefix,
            PhotoStatus.Archived => ArchivedPrefix,
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
        };
    }

    private static string GetExtension(string extension) => $".{extension}";
    
    public static string BuildPhotoPath(PhotoStatus status, string fileName) =>
        $"{GetPrefixForStatus(status)}/{fileName}";

    public static string BuildThumbnailPath(Guid photoId, string extension)
    {
        if (string.IsNullOrEmpty(extension))
            throw new ArgumentNullException(nameof(extension), "Extension cannot be null or empty");
      
        if (!extension.StartsWith('.')) extension = GetExtension(extension);
        
        return $"{ThumbnailsPrefix}/{photoId}{extension}";
    }

    public static string BuildHomepagePath(Guid photoId, string extension)
    {
        if (string.IsNullOrEmpty(extension))
            throw new ArgumentNullException(nameof(extension), "Extension cannot be null or empty");
        
        if (!extension.StartsWith('.')) extension = GetExtension(extension);
        
        return $"{HomepagePrefix}/{photoId}{extension}";
    }
    
    public static PhotoStatus ParseStatus(string s3Path)
    {
        if (s3Path.StartsWith(UploadsPrefix, StringComparison.OrdinalIgnoreCase))
            return PhotoStatus.Draft;
        
        if (s3Path.StartsWith(ArchivedPrefix, StringComparison.OrdinalIgnoreCase))
            return PhotoStatus.Archived;
        
        if (s3Path.StartsWith(GalleryPrefix, StringComparison.OrdinalIgnoreCase))
            return PhotoStatus.Published;
        
        throw new InvalidDataException(
                $"Cannot parse status from '{s3Path}'" +
                $"Expected prefix: {UploadsPrefix}, {GalleryPrefix}, or {ArchivedPrefix}");
    }
    
    public static string GetFileName(string s3Path) => Path.GetFileName(s3Path);
}