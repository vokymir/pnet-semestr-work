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

    public async Task AddAsync(Watcher watcher)
    {
        await _context.Watchers.AddAsync(watcher);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Watcher watcher)
    {
        var dbWatcher = await WatcherWithDetails.FirstOrDefaultAsync(w => w.Id == watcher.Id);
        if (dbWatcher == null)
            throw new InvalidOperationException($"Watcher with ID = {watcher.Id} is not in the database and cannot be updated.");

        // Update individual properties or attach and set state
        _context.Entry(dbWatcher).CurrentValues.SetValues(watcher);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var watcher = await GetByIdAsync(id);
        if (watcher == null)
            throw new InvalidOperationException($"Watcher with ID = {id} is not in the database and cannot be deleted.");

        _context.Watchers.Remove(watcher);
        await _context.SaveChangesAsync();
    }

    public async Task<Watcher[]> GetByUserAsync(User user)
    {
        return await WatcherWithDetails
            .Where(w => w.Curators.Contains(user))
            .ToArrayAsync();
    }

    public async Task<Watcher?> GetByIdAsync(int id)
    {
        return await WatcherWithDetails.FirstOrDefaultAsync(w => w.Id == id);
    }

    public async Task<Watcher?> GetByPublicIdAsync(string publicId)
    {
        return await WatcherWithDetails.FirstOrDefaultAsync(w => w.PublicIdentifier == publicId);
    }

    public async Task<Watcher[]> GetAllAsync()
    {
        return await WatcherWithDetails.ToArrayAsync();
    }

    public async Task<Dictionary<string, bool>> GetAllPublicIdentifiersAsync()
    {
        return await _context.Watchers
            .ToDictionaryAsync(w => w.PublicIdentifier, w => true);
    }
}
