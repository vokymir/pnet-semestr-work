namespace BirdWatching;

public class Bird
{
    public int Id { get; set; }
    // rodove jmeno, napr.: sykora
    public string Genus { get; set; } = "Neurceny";
    // druhove jmeno, napr.: konadra
    public string Species { get; set; } = "Ptacek";

    public override string ToString() => $"{Genus} {Species}";

    public override int GetHashCode() => string.GetHashCode(this.ToString());

    public override bool Equals(object? obj)
    {
        if (obj is not Bird) return false;

        return ((Bird)obj).Id == this.Id;
    }

}
