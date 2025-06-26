namespace BirdWatching.Shared.Model;

public class BirdDto : IAmDto<Bird>
{
    public int Id { get; set; }
    public string Ordo { get; set; } = "řád, např.: pěvci";
    public string Familia { get; set; } = "čeleď, např.: sýkorovití";
    public string Genus { get; set; } = "rod, např.: sýkora";
    public string Species { get; set; } = "druh, např.: koňadra";
    public string BinomicName {
        get => $"{Genus} {Species}";
    }
    public string FullName {
        get => $"{Genus} {Species} (Řád {Ordo}, Čeleď {Familia})";
    }

    public string Comment { get; set; } = string.Empty;

    public Bird ToEntity()
    {
        var b = new Bird() {
            Id = Id,
            Ordo = Ordo,
            Familia = Familia,
            Genus = Genus,
            Species = Species,
            Comment = Comment,
        };

        return b;
    }
}
