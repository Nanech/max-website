using PhotosApi.Helpers;

namespace Photos.UnitTests.Helpers;

public class StoragePathsTests
{
    [Theory]
    [InlineData(PhotoStatus.Draft, "uploads")]
    [InlineData(PhotoStatus.Published, "gallery")]
    [InlineData(PhotoStatus.Archived, "archived")]
    public void GetPrefixForStatus_ReturnsCorrectPrefix(PhotoStatus status, string expectedPrefix)
    {
        var prefix = StoragePaths.GetPrefixForStatus(status);
        Assert.Equal(expectedPrefix, prefix);
    }

    [Fact]
    public void BuildPhotoPath_Draft_ReturnsUploadsPath()
    {
        var path = StoragePaths.BuildPhotoPath(PhotoStatus.Draft, "test.jpg");
        Assert.Equal("uploads/test.jpg", path);
    }

    [Fact]
    public void BuildPhotoPath_Published_ReturnsGalleryPath()
    {
        var path = StoragePaths.BuildPhotoPath(PhotoStatus.Published, "test.jpg");
        Assert.Equal("gallery/test.jpg", path);
    }

    [Fact]
    public void BuildThumbnailPath_WithDot_ReturnsCorrectPath()
    {
        var guid = Guid.Parse("abc12345-1234-1234-1234-123456789abc");
        var path = StoragePaths.BuildThumbnailPath(guid, ".jpg");
        Assert.Equal("thumbnails/abc12345-1234-1234-1234-123456789abc.jpg", path);
    }

    [Fact]
    public void BuildThumbnailPath_WithoutDot_ReturnsCorrectPath()
    {
        var guid = Guid.Parse("abc12345-1234-1234-1234-123456789abc");
        var path = StoragePaths.BuildThumbnailPath(guid, "jpg");
        Assert.Equal("thumbnails/abc12345-1234-1234-1234-123456789abc.jpg", path);
    }

    [Theory]
    [InlineData("uploads/test.jpg", PhotoStatus.Draft)]
    [InlineData("gallery/test.jpg", PhotoStatus.Published)]
    [InlineData("archived/test.jpg", PhotoStatus.Archived)]
    public void ParseStatus_ReturnsCorrectStatus(string s3Path, PhotoStatus expectedStatus)
    {
        var status = StoragePaths.ParseStatus(s3Path);
        Assert.Equal(expectedStatus, status);
    }

    [Fact]
    public void ParseStatus_InvalidPath_ThrowsException()
    {
        Assert.Throws<InvalidDataException>(() => 
            StoragePaths.ParseStatus("invalid/test.jpg"));
    }

    [Fact]
    public void GetFileName_ReturnsCorrectFileName()
    {
        var fileName = StoragePaths.GetFileName("uploads/test.jpg");
        Assert.Equal("test.jpg", fileName);
    }
}