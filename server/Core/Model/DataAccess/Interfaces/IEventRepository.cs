namespace BirdWatching;

public interface IEventRepository
{
    void Add(Event @event);
    void Delete(int id);
    void Update(Event @event);
    Event? GetById(int id);
    IEnumerable<Event> GetAll();
}
