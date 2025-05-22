namespace BirdWatching.Shared.Model;

public class Bird : IHaveDto<BirdDto>
{
    public int Id { get; set; }
    public string Genus { get; set; } = "Neurceny";
    public string Species { get; set; } = "Ptacek";

    public string Comment { get; set; } = string.Empty;

    public BirdDto ToDto()
    {
        var b = new BirdDto() {
            Id = Id,
            Genus = Genus,
            Species = Species,
            Comment = Comment,
        };

        return b;
    }

    public BirdDto ToFullDto() => ToDto();
}
