namespace BirdWatching;

public class BirdUser
{
    public int Id { get; set; }
    public string UserName { get; set; } = "username";
    public string PasswordHash { get; set; } = "password";

    public ICollection<BirdWatcher> Watchers { get; set; } = new List<BirdWatcher>();
    public ICollection<BirdEvent> Events { get; set; } = new List<BirdEvent>();

    public BirdUser() { }
    public BirdUser(string username, string passwordHash)
    {
        UserName = username;
        PasswordHash = passwordHash;
    }
}
