using FluentValidation;

namespace PhotosApi.Features.Photos;

public static class PhotoValidators
{
     private static readonly System.Buffers.SearchValues<char> SForbiddenChars 
        = System.Buffers.SearchValues.Create("/\\:*?!\"<>|\0");
    
    private const long MinFileSize = 1024; // 1kb
    private const long MaxFileSize = 10 * 1024 * 1024; // 10 MB   
    private const int MaxFileName = 255;

    private static readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png", ".webp"];
    private static readonly string[] AllowedMimeTypes = ["image/jpeg", "image/png", "image/webp"];
    
    // todo: must to realize it asap
    private static readonly Dictionary<string, byte[][]> FileSignatures = new()
    {
        {
            "image/jpeg",
            [
                [0xFF, 0xD8, 0xFF, 0xE0], // JPEG JFIF
                [0xFF, 0xD8, 0xFF, 0xE1], // JPEG Exif
                [0xFF, 0xD8, 0xFF, 0xE2], // JPEG
                [0xFF, 0xD8, 0xFF, 0xE3]  // JPEG
            ]
        },
        {
            "image/png",
            [
                [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A] // PNG
            ]
        },
        {
            "image/webp",
            [
                [0x52, 0x49, 0x46, 0x46] // RIFF (WebP начинается с "RIFF")
            ]
        }
    };

    public static IRuleBuilderOptionsConditions<T, IFormFile> ValidPhotoFile<T>(this IRuleBuilder<T, IFormFile> ruleBuilder)
    {
        return ruleBuilder.Custom((file, context) =>
        {
            if (file is null)
            {
                context.AddFailure("File cannot be null");
                return;
            }
            
            switch (file.Length)
            {
                case < MinFileSize:
                    context.AddFailure($"File size is too small: {file.Length} bytes. Min: {MinFileSize} bytes.");
                    return;
                case > MaxFileSize:
                    context.AddFailure($"File size is too big. Max: {MaxFileSize / 1024 / 1024} MB.");
                    return;
            }
            
            var fileName = file.FileName;
            if (string.IsNullOrEmpty(fileName))
            {
                context.AddFailure("File name is null or empty");
                return;
            }

            if (fileName.Length > MaxFileName)
            {
                context.AddFailure($"File name is too long: {fileName.Length}, max is {MaxFileName}");
                return;
            }
                
            var forbiddenCharIndex = fileName.AsSpan().IndexOfAny(SForbiddenChars);
            if (forbiddenCharIndex >= 0)
            {
                context.AddFailure("File name contains forbidden characters");
                return;
            }
        
            var extensionCount = fileName.Count(c => c == '.');
            if (extensionCount > 1)
            {
                context.AddFailure($"File name contains more than one dot: {fileName}");
                return;
            }
                
            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (string.IsNullOrEmpty(fileExtension) || !AllowedExtensions.Contains(fileExtension))
            {
                context.AddFailure($"Unsupported file extension: {fileExtension}, allowed: {string.Join( ", ", AllowedExtensions)}");
                return;
            } 
               
            var contentType = file.ContentType;
            if (string.IsNullOrEmpty(contentType) || !AllowedMimeTypes.Contains(contentType))
            {
                context.AddFailure($"Unsupported file type: {contentType}. Allowed: {string.Join(", ", AllowedMimeTypes)}");
                return;
            }
                
            if (FileSignatures.TryGetValue(contentType, out var signatures))
            {
                using var stream = file.OpenReadStream();
                // Нам нужно прочитать максимум 8 байт (для PNG)
                var maxSignatureLength = signatures.Max(s => s.Length);
                var headerBytes = new byte[maxSignatureLength];
                
                var bytesRead = stream.Read(headerBytes, 0, maxSignatureLength);

                if (bytesRead < 4)
                {
                    context.AddFailure("File is corrupted or too short to verify its signature.");
                    return;
                }
                
                ReadOnlySpan<byte> readSpan = headerBytes.AsSpan(0, bytesRead);
                var isValidSignature = false;

                if (contentType == "image/webp")
                {
                    if (readSpan.Length >= 12 &&
                        readSpan.StartsWith("RIFF"u8) && // RIFF
                        readSpan.Slice(8,4).SequenceEqual("WEBP"u8)
                        )
                        isValidSignature = true;
                }
                else
                {
                    foreach (var signature in signatures)
                    {
                        if (!readSpan.SequenceEqual(signature)) continue;
                    
                        isValidSignature = true;
                        break;
                    }
                }
                
                if (!isValidSignature)
                    context.AddFailure("File content does not match its extension (invalid file signature).");
            }
            
        });
    }
    
    
}