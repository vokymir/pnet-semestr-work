namespace BirdWatching.Shared.Model;

public interface IWatcherRepository
{
    public void Add(Watcher watcher);
    public void Delete(int id);
    public void Update(Watcher watcher);
    public Watcher[] GetByUser(User user);
    public Watcher? GetById(int id);
    public Watcher? GetByPublicId(string publicId);
    public IEnumerable<Watcher> GetAll();
    public Dictionary<string, bool> GetAllPublicIdentifiers();
}
