namespace BirdWatching.Shared.Model;

using Microsoft.EntityFrameworkCore;

public class EFWatcherRepository : IWatcherRepository
{
    private AppDbContext _context;

    public IQueryable<Watcher> WatcherWithDetails {
        get {
            return _context.Watchers
                .Include(w => w.Records)
                .Include(w => w.Curators)
                .Include(w => w.Participating)
                .Include(w => w.MainCurator);
        }
        set { }
    }

    public EFWatcherRepository(AppDbContext context)
    {
        _context = context;
    }

    public void Add(Watcher watcher)
    {
        _context.Watchers.Add(watcher);
        _context.SaveChanges();
    }

    public void Update(Watcher watcher)
    {
        var dbWatcher = WatcherWithDetails.First(w => w.Id == watcher.Id);

        if (dbWatcher is null)
            throw new InvalidOperationException($"Watcher with ID = {watcher.Id} is not in the database and cannot be updated.");

        dbWatcher = watcher;
        _context.Update(dbWatcher);
        _context.SaveChanges();
    }

    public void Delete(int id)
    {
        var watcher = GetById(id);

        if (watcher is null)
            throw new InvalidOperationException($"Watcher with ID = {id} is not in the database and cannot be deleted.");

        _context.Watchers.Remove(watcher);
        _context.SaveChanges();
    }

    public Watcher[] GetByUser(User user) => WatcherWithDetails.Where(w => w.Curators.Contains(user)).ToArray();

    public Watcher? GetById(int id) => WatcherWithDetails.FirstOrDefault(w => w.Id == id);

    public Watcher? GetByPublicId(string publicId) => WatcherWithDetails.FirstOrDefault(w => w.PublicIdentifier.Equals(publicId));

    public IEnumerable<Watcher> GetAll() => WatcherWithDetails.ToArray();

    public Dictionary<string, bool> GetAllPublicIdentifiers() => _context.Watchers.ToDictionary(w => w.PublicIdentifier, w => true);
}
