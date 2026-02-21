using PhotosApi.Helpers;

namespace PhotosApi.Models;

public class Category
{
    public short CategoryId { get; init; }
    public string Name { get; init; } = string.Empty;

    public CategoryType CategoryType => Name switch
    {
        "Персональная съемка" => CategoryType.PersonalPhoto,
        "Репортаж" => CategoryType.Reportage,
        "Love Story" => CategoryType.LoveStory,
        "Свадебная съемка" => CategoryType.WeddingPhoto,
        _ => CategoryType.None
    };

    public ICollection<Album> Albums { get; set; } = [];
}