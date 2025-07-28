namespace BirdWatching.Shared.Model;

public interface IEventRepository
{
    Task AddAsync(Event @event);
    Task DeleteAsync(int id);
    Task UpdateAsync(Event @event);
    Task<Event?> GetByIdAsync(int id);
    Task<Event?> GetByIdDetailedAsync(int id);
    Task<Event?> GetByPublicIdAsync(string publicId);
    Task<Event[]> GetAllAsync();
    Task<Event[]> GetByUserIdAsync(int userId);
    Task<Event[]> GetByWatcherIdAsync(int watcherId);
    Task<Dictionary<string, bool>> GetAllPublicIdentifiersAsync();
    Task<Watcher[]?> GetParticipantsAsync(int id);
}
