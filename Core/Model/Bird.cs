namespace BirdWatching;

public class Bird
{
    // rodove jmeno, napr.: sykora
    public string Genus { get; set; } = "Neurceny";
    // druhove jmeno, napr.: konadra
    public string Species { get; set; } = "Ptacek";

    public Bird() { }

    /// <summary>
    /// Create a new bird, where genus is e.g. "sykora" and species e.g. "konadra".
    /// </summary>
    public Bird(string genus, string species)
    {
        Genus = genus;
        Species = species;
    }

    public override string ToString() => $"{Genus} {Species}";

    public override int GetHashCode() => string.GetHashCode(this.ToString());

    public override bool Equals(object? obj)
    {
        if (obj is not Bird) return false;

        return ((Bird)obj).ToString().Equals(this.ToString());
    }

}
