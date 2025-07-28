using Microsoft.EntityFrameworkCore;

namespace BirdWatching.Shared.Model;

public class EFEventRepository : IEventRepository
{
    private readonly AppDbContext _context;

    public IQueryable<Event> EventsWithDetails {
        get {
            return _context.Events
                .Include(e => e.Participants)
                .Include(e => e.MainAdmin)
                .Include(e => e.NotValidRecords)
                ;
        }
        set { }
    }

    public EFEventRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Event @event)
    {
        await _context.Events.AddAsync(@event);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Event @event)
    {
        var dbEvent = await EventsWithDetails.FirstOrDefaultAsync(e => e.Id == @event.Id);

        if (dbEvent == null)
            throw new InvalidOperationException($"Event with ID = {@event.Id} is not in the database and cannot be updated.");

        _context.Entry(dbEvent).CurrentValues.SetValues(@event);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var @event = await GetByIdAsync(id);

        if (@event == null)
            throw new InvalidOperationException($"Event with ID = {id} is not in the database and cannot be deleted.");

        _context.Events.Remove(@event);
        await _context.SaveChangesAsync();
    }

    public async Task<Event?> GetByIdAsync(int id)
    {
        return await EventsWithDetails.FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<Event?> GetByIdDetailedAsync(int id)
    {
        return await EventsWithDetails.FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<Event?> GetByPublicIdAsync(string publicId)
    {
        return await EventsWithDetails.FirstOrDefaultAsync(e => e.PublicIdentifier == publicId);
    }

    public async Task<Event[]> GetAllAsync()
    {
        return await EventsWithDetails.ToArrayAsync();
    }

    public async Task<Event[]> GetByUserIdAsync(int userId)
    {
        return await EventsWithDetails.Where(e => e.MainAdminId == userId).ToArrayAsync();
    }

    public async Task<Event[]> GetByWatcherIdAsync(int watcherId)
    {
        var watcher = await _context.Watchers
            .Include(w => w.Participating)
                .ThenInclude(e => e.Participants)
            .Include(w => w.Participating)
                .ThenInclude(e => e.MainAdmin)
            .FirstOrDefaultAsync(w => w.Id == watcherId);

        return watcher?.Participating?.ToArray() ?? Array.Empty<Event>();
    }

    public async Task<Dictionary<string, bool>> GetAllPublicIdentifiersAsync()
    {
        return await _context.Events
            .Select(e => e.PublicIdentifier)
            .ToDictionaryAsync(id => id, id => true);
    }

    public async Task<Watcher[]?> GetParticipantsAsync(int id)
    {
        var @event = await _context.Events
            .Include(e => e.Participants)
            .FirstOrDefaultAsync(e => e.Id == id);

        return @event?.Participants?.ToArray();
    }
}
