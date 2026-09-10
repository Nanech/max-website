using FluentValidation;

namespace PhotosApi.Features.AdminShowcase;

public record CreateShowcaseSectionCommand(
    int PageId,
    string Name,
    string SectionKey,
    int ShowcaseSectionTypeId
);

public class CreateShowcaseSectionValidator : AbstractValidator<CreateShowcaseSectionCommand>
{
    public CreateShowcaseSectionValidator()
    {
        RuleFor(x => x.PageId).NotNull()
            .GreaterThan(0).WithMessage($"PageId is key between 1 and {int.MaxValue}");
        
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name cannot be empty")
            .Length(3, 255).WithName("Name must be between 3 and 255 characters long");
        
        RuleFor(x => x.SectionKey).NotEmpty().WithMessage("Section key cannot be empty");
        
        RuleFor(x => x.ShowcaseSectionTypeId)
            .NotNull().WithMessage("Showcase section type id cannot be empty")
            .GreaterThan(0).WithMessage($"ShowcaseSectionTypeId is key between 1 and {int.MaxValue}");
    }
}

public record CreateShowcaseItemCommand(
    int SectionId,
    Guid? AlbumId,
    Guid? PhotoId
);

public class CreateShowcaseItemValidator : AbstractValidator<CreateShowcaseItemCommand>
{
    public CreateShowcaseItemValidator()
    {
        RuleFor(x => x.SectionId).GreaterThan(0)
            .WithMessage($"SectionId is key between 1 and {int.MaxValue}");
        
        RuleFor(x => x)
            .Must(HasExactlyOneTarget)
            .WithMessage("There must be exactly one target: either AlbumId or PhotoId.")
            .WithName("ItemTarget");
    }

    private static bool HasExactlyOneTarget(CreateShowcaseItemCommand command)
    {
        var hasAlbum = command.AlbumId.HasValue && command.AlbumId.Value != Guid.Empty;
        var hasPhoto = command.PhotoId.HasValue && command.PhotoId.Value != Guid.Empty;

        return hasAlbum ^ hasPhoto; 
    }
}
