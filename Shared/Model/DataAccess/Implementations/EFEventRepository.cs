namespace BirdWatching.Shared.Model;

using Microsoft.EntityFrameworkCore;

public class EFEventRepository : IEventRepository
{
    private AppDbContext _context;

    public EFEventRepository(AppDbContext context)
    {
        _context = context;
    }

    public void Add(Event @event)
    {
        _context.Events.Add(@event);
        _context.SaveChanges();
    }

    public void Update(Event @event)
    {
        var dbEvent = _context.Events.Find(@event.Id);

        if (dbEvent is null)
            throw new InvalidOperationException($"Event with ID = {@event.Id} is not in the database and cannot be updated.");
        dbEvent = @event;
        _context.Update(dbEvent);
        _context.SaveChanges();
    }

    public void Delete(int id)
    {
        var @event = GetById(id);

        if (@event is null)
            throw new InvalidOperationException($"Event with ID = {id} is not in the database and cannot be deleted.");
        _context.Events.Remove(@event);
        _context.SaveChanges();
    }

    public Event? GetById(int id) => _context.Events.Find(id);

    public IEnumerable<Event> GetAll() => _context.Events.ToList();

    public Dictionary<string, bool> GetAllPublicIdentifiers() => _context.Events.ToDictionary(e => e.PublicIdentifier, e => true);

    public IEnumerable<Watcher>? GetParticipants(int id) =>
        _context.Events.Include(e => e.Participants).First(e => e.Id == id)?
        .Participants ?? null; // if event doesn't exist, return null
}
