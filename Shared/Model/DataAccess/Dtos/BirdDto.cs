namespace BirdWatching.Shared.Model;

public class BirdDto : IAmDto<Bird>
{
    public int Id { get; set; }
    public string Genus { get; set; } = "Neurceny";
    public string Species { get; set; } = "Ptacek";

    public Bird ToEntity()
    {
        var b = new Bird() {
            Id = Id,
            Genus = Genus,
            Species = Species
        };

        return b;
    }
}
