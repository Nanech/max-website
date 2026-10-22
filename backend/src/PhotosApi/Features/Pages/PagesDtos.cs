using PhotosApi.Features.Albums;
using PhotosApi.Features.Photos;

namespace PhotosApi.Features.Pages;

public record PageLayoutDto(
    string PageKey,
    string Name,
    IReadOnlyCollection<SectionDto> Sections
);

public record SectionDto(
    string SectionKey,
    string SectionType,
    IReadOnlyCollection<ShowcaseItemDto> Items
);

public abstract record ShowcaseItemDto(
    string Type
);

public sealed record AlbumShowcaseItemDto(
    AlbumCardDto AlbumCard,
    PhotoUrlDto? CoverPhoto
) : ShowcaseItemDto("album");

public sealed record PhotoShowcaseItemDto(
    PhotoUrlDto Photo
) : ShowcaseItemDto("photo");