using PhotosApi.Helpers;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;

namespace PhotosApi.Services;

public record ProcessedPhotoVersion(string Path, Stream Data, string ContentType = "image/webp");

public record ProcessedPhotoGroup(
    ProcessedPhotoVersion Original,
    ProcessedPhotoVersion Large,
    ProcessedPhotoVersion Preview
)
{
    public ProcessedPhotoVersion[] GetAllVersions() => [Original, Large, Preview];
}
    

public class ImageSharpPhotoProcessor
{

    public async Task<ProcessedPhotoGroup> ProcessPhotoAsync(
        Guid photoId,
        Stream originalStream, 
        CancellationToken ct = default
    )
    {
        using var originalImage = await Image.LoadAsync(originalStream, ct);
    
        
        if (originalImage == null)
            throw new ArgumentException("Failed to deserialize photo", nameof(originalStream));
        
        // 2. Генерируем 3 версии (Original, Large, Preview)
        var originalStreamOutput = await EncodeToWebpAsync(originalImage, quality: 90, maxDimension: null, ct);
        var largeStreamOutput = await EncodeToWebpAsync(originalImage, quality: 80, maxDimension: 1920, ct);
        var previewStreamOutput = await EncodeToWebpAsync(originalImage, quality: 75, maxDimension: 400, ct);

        // 3. Собираем группу версий
        return new ProcessedPhotoGroup(
            Original: new ProcessedPhotoVersion(PhotoObjectFactory.BuildOriginalPath(photoId), originalStreamOutput),
            Large: new ProcessedPhotoVersion(PhotoObjectFactory.BuildLargePath(photoId), largeStreamOutput),
            Preview: new ProcessedPhotoVersion(PhotoObjectFactory.BuildPreviewPath(photoId), previewStreamOutput)
        );
    }

    private async Task<MemoryStream> EncodeToWebpAsync(Image sourceImage, int quality, int? maxDimension , CancellationToken ct)
    {
        // Делаем клон оригинального изображения для потокобезопасной мутации
        using var clone = sourceImage.Clone(ctx =>
        {
            if (maxDimension.HasValue)
            {
                // ResizeMode.Max пропорционально уменьшит фото, 
                // только если одна из сторон превышает maxDimension
                ctx.Resize(new ResizeOptions
                {
                    Size = new Size(maxDimension.Value, maxDimension.Value),
                    Mode = ResizeMode.Max
                });
            }
        });

        var memoryStream = new MemoryStream();
        var encoder = new WebpEncoder
        {
            Quality = quality,
            FileFormat = WebpFileFormatType.Lossy
        };

        await clone.SaveAsync(memoryStream, encoder, ct);
        memoryStream.Position = 0; // Сбрасываем каретку в начало потока для дальнейшего чтения

        return memoryStream;
    }

}