namespace BirdWatching.Shared.Model;

public interface IBirdRepository
{
    Task AddAsync(Bird bird);
    Task DeleteAsync(int id);
    Task UpdateAsync(Bird bird);
    Task<Bird?> GetByIdAsync(int id);
    Task<Bird[]> GetAllAsync();
    Task<Bird[]> GetByPrefixAsync(string prefix);
    Task<Bird[]> GetByPrefixFastAsync(string prefix);
    Task<Bird[]> GetByContainsAsync(string str);
}
