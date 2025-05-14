namespace BirdWatching.Shared.Model;

public class EFEventRepository : IEventRepository
{
    private AppDbContext _context;

    public EFEventRepository(AppDbContext context)
    {
        _context = context;
    }

    void Add(Event @event)
    {
        _context.Events.Add(@event);
        _context.SaveChanges();
    }

    void Update(Event @event)
    {
        var dbEvent = _context.Events.Find(@event.Id);

        if (dbEvent is null)
            throw new InvalidOperationException($"Event with ID = {@event.Id} is not in the database and cannot be updated.");
        dbEvent = @event;
        _context.Update(dbEvent);
        _context.SaveChanges();
    }

    void Delete(int id)
    {
        var @event = GetById(id);

        if (@event is null)
            throw new InvalidOperationException($"Event with ID = {id} is not in the database and cannot be deleted.");
        _context.Events.Remove(@event);
        _context.SaveChanges();
    }

    Event? GetById(int id) => _context.Events.Find(id);

    IEnumerable<Event> GetAll() => _context.Events.ToList();
}
