namespace BirdWatching;

public class BirdWatcher
{
    public int Id { get; set; }

    public ICollection<BirdRecord> Records = new List<BirdRecord>();

    public BirdUser MainCurator { get; set; } = new BirdUser();
    public ICollection<BirdUser> Curators = new List<BirdUser>();

    public ICollection<BirdEvent> Participating = new List<BirdEvent>();

    public BirdWatcher() { }
    public BirdWatcher(BirdUser user)
    {
        MainCurator = user;
    }
}
