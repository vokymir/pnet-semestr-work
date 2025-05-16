namespace BirdWatching.Shared.Model;

using Microsoft.EntityFrameworkCore;

public class EFWatcherRepository : IWatcherRepository
{
    private AppDbContext _context;

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
        var dbWatcher = _context.Watchers.Find(watcher.Id);

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

    public Watcher[] GetByUser(User user) => _context.Watchers.Where(w => w.Curators.Contains(user)).ToArray();

    public Watcher? GetById(int id) => _context.Watchers.Find(id);

    public IEnumerable<Watcher> GetAll() => _context.Watchers
        .Include(w => w.Curators)
        .ToArray();
}
