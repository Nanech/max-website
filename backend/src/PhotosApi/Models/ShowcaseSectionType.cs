namespace PhotosApi.Models;

public class ShowcaseSectionType
{
    public int Id { get; init; }
    public string Name { get; set; } = string.Empty;
    public bool Removable { get; set; } = true;
}