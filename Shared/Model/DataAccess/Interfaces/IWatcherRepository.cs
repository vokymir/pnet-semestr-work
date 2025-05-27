namespace BirdWatching.Shared.Model;

public interface IWatcherRepository
{
    Task AddAsync(Watcher watcher);
    Task DeleteAsync(int id);
    Task UpdateAsync(Watcher watcher);
    Task<Watcher[]> GetByUserAsync(User user);
    Task<Watcher?> GetByIdAsync(int id);
    Task<Watcher?> GetByPublicIdAsync(string publicId);
    Task<Watcher[]> GetAllAsync();
    Task<Dictionary<string, bool>> GetAllPublicIdentifiersAsync();
}
