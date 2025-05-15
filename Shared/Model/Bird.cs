namespace BirdWatching.Shared.Model;

public class Bird
{
    public int Id { get; set; }
    public string Genus { get; set; } = "Neurceny";
    public string Species { get; set; } = "Ptacek";

    public override string ToString() => $"{Genus} {Species}";
    public override int GetHashCode() => ToString().GetHashCode();
    public override bool Equals(object? obj) => obj is Bird b && b.Id == Id;
}
