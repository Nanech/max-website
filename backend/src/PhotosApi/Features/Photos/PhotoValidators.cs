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
            
            using var stream = file.OpenReadStream();
            Span<byte> headerBytes = stackalloc byte[12];
            var bytesRead = stream.Read(headerBytes);

            if (bytesRead < 4) // for jpeg/png/webp min 4 bytes 
            {
                context.AddFailure($"Unexpected end of file: {file}");
                return;
            }
            
            var readSpan = headerBytes[..bytesRead];
            var isValidSignature = false;

            if (contentType is "image/jpeg" or "image/jpg")
            {
                isValidSignature = readSpan.StartsWith((ReadOnlySpan<byte>)[0xFF, 0xD8]);
            }
            else if (contentType == "image/png")
            {
                // Стандартная 8-байтная сигнатура PNG
                isValidSignature = readSpan.StartsWith((ReadOnlySpan<byte>)[0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A]);
            }
            else if (contentType == "image/webp")
            {
                // Теперь буфер точно 12 байт, проверяем "RIFF" в начале и "WEBP" на 8-й позиции
                isValidSignature = readSpan.Length >= 12 
                                   && readSpan.StartsWith("RIFF"u8) 
                                   && readSpan[8..12].SequenceEqual("WEBP"u8);
            }

            if (!isValidSignature)
            {
                context.AddFailure("File content does not match its extension (invalid file signature).");
            }
        });
    }
    
    
}