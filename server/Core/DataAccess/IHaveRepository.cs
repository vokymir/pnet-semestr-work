namespace BirdWatching;

public interface IHaveRepository
{
    public IRepository<Bird> Birds { get; set; }
    public IRepository<Event> Events { get; set; }
    public IRepository<User> Users { get; set; }
    public IRepository<Watcher> Watchers { get; set; }
    public IRepository<Record> Records { get; set; }
}
