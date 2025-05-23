namespace BirdWatching.Shared.Model;

public interface IEventRepository
{
    public void Add(Event @event);
    public void Delete(int id);
    public void Update(Event @event);
    public Event? GetById(int id);
    public Event? GetByPublicId(string publicId);
    public IEnumerable<Event> GetAll();
    public Dictionary<string, bool> GetAllPublicIdentifiers();
    public IEnumerable<Watcher>? GetParticipants(int id);
}
