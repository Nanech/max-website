using FluentValidation;
using PhotosApi.Features.Photos;
using PhotosApi.Helpers;

namespace PhotosApi.Features.Albums;

public record AlbumDto(
    Guid AlbumId,
    string Name,
    short? CategoryId,
    string? CategoryName,
    short ShootYear,
    int PhotosCount,
    DateTime CreatedAt
);

public record AlbumDetailDto(
    Guid AlbumId,
    string Name,
    short? CategoryId,
    string? CategoryName,
    short ShootYear,
    DateTime CreateAt,
    List<PhotoDto> Photos
);


public record CreateAlbumCommand(
    string Name,
    short CategoryId,
    short ShootYear,
    AlbumStatus Status
);

public class CreateAlbumDtoValidator : AbstractValidator<CreateAlbumCommand>
{
    public CreateAlbumDtoValidator()
    {
        RuleFor(c => c.Name)
            .NotEmpty().WithMessage("Album name cannot be empty")
            .Length(3, 255).WithMessage("Album name must  be between 3 and 255 characters");
        
        RuleFor(c => c.CategoryId)
            .InclusiveBetween((short)1, (short)4)
            .WithMessage("Category id cannot be less than 4)");
        
        RuleFor(c => c.ShootYear)
            .InclusiveBetween((short)1900, (short)DateTime.UtcNow.Year)
            .WithMessage("Shot year cannot be less than 1900");
    }
}

public record GetAlbumsQuery(
    string? SearchTerm,
    short? CategoryId,
    short? Year
);

public class GetAlbumsQueryValidator : AbstractValidator<GetAlbumsQuery>
{
    public GetAlbumsQueryValidator()
    {
        When(q => !string.IsNullOrEmpty(q.SearchTerm), () =>
        {
            RuleFor(c => c.SearchTerm)
                .MinimumLength(3).WithMessage("Search term is required")
                .MaximumLength(100).WithMessage("Search term cannot exceed 100 characters");
        });

        When(q => q.CategoryId.HasValue, () =>
        {
            RuleFor(c => c.CategoryId)
                .InclusiveBetween((short)1, (short)4).WithMessage("Category id cannot be less than 4)")
                .When(q => !q.CategoryId.HasValue);
        });

        When(q => q.Year.HasValue, () =>
        {
            RuleFor(c => c.Year)
                .InclusiveBetween((short)1900, (short)DateTime.UtcNow.Year)
                .WithMessage("Shot year cannot be less than 1900")
                .When(q => !q.Year.HasValue);

        });
    }
}
