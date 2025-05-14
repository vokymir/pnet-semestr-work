namespace BirdWatching;

public interface IWatcherRepository
{
    void Add(Watcher watcher);
    void Delete(int id);
    void Update(Watcher watcher);
    Watcher? GetById(int id);
    IEnumerable<Watcher> GetAll();
}
