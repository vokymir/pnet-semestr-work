namespace BirdWatching.Shared.Model;

public class Bird : IHaveDto<BirdDto>
{
    public int Id { get; set; }
    public string Ordo { get; set; } = "řád, např.: pěvci";
    public string Familia { get; set; } = "čeleď, např.: sýkorovití";
    public string Genus { get; set; } = "rod, např.: sýkora";
    public string Species { get; set; } = "druh, např.: koňadra";
    public string FullName {
        get => $"{Genus} {Species}";
    }

    public string Comment { get; set; } = string.Empty;

    public BirdDto ToDto()
    {
        var b = new BirdDto() {
            Id = Id,
            Ordo = Ordo,
            Familia = Familia,
            Genus = Genus,
            Species = Species,
            Comment = Comment,
        };

        return b;
    }

    public BirdDto ToFullDto() => ToDto();
}
