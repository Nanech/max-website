using FluentValidation;

namespace PhotosApi.Features.Photos.Shared;

public static class PhotoFileValidator
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
    

    public static void ValidateFile(IFormFile file)
    {
        ValidateFileNotNull(file);
        ValidateFileSize(file);
        ValidateFileName(file);
        ValidateExtension(file);
        ValidateMimeType(file);
        
    }

    private static void ValidateFileNotNull(IFormFile? file)
    {
        if (file is null)
            throw new ValidationException("File is null");
    }

    private static void ValidateFileSize(IFormFile file)
    {
        switch (file.Length)
        {
            case < MinFileSize:
                throw new InvalidDataException(
                    $"File size is too small: {file.Length} bytes" +
                    $"Min: {MinFileSize} byte ({MinFileSize/1025} Kb) "
                );
            
            case > MaxFileSize:
                throw new InvalidDataException("File size is too big");
        }
    }

    private static void ValidateFileName(IFormFile file)
    {
        var fileName = file.FileName;
        
        if (string.IsNullOrEmpty(fileName))
            throw new InvalidDataException("File name is null or empty");
        
        if (fileName.Length > MaxFileName)
            throw new InvalidDataException($"File name is too long: {fileName.Length}, max is {MaxFileName}");
        
        var forbiddenCharIndex = fileName.AsSpan().IndexOfAny(SForbiddenChars);
        if (forbiddenCharIndex >= 0)
            throw new ValidationException("File name contains forbidden characters");
        
        var extensionCount = fileName.Count(c => c == '.');
        if (extensionCount > 1)
            throw new InvalidDataException($"File name contains more than one dot: {fileName}");
    }

    private static void ValidateExtension(IFormFile file)
    {
        var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
        
        if (string.IsNullOrEmpty(fileExtension))
            throw new ValidationException("File must contain an extension"); 
               
        if (!AllowedExtensions.Contains(fileExtension))
            throw new InvalidDataException($"Unsupported file extension: {fileExtension}," +
                                           $" allowed: {string.Join( ", ", AllowedExtensions)}");
    }

    private static void ValidateMimeType(IFormFile file)
    {
        var contentType = file.ContentType;
        
        if (string.IsNullOrEmpty(contentType))
            throw new ValidationException("File must contain a type");
        
        if (!AllowedMimeTypes.Contains(contentType))
            throw new ValidationException($"Unsupported file type: {contentType}," +
                                          $" allowed: {string.Join(',', AllowedMimeTypes)}");
    }
    
    
}