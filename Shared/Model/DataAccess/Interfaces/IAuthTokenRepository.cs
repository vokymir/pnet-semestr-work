namespace BirdWatching.Shared.Model;

public interface IAuthTokenRepository
{
    Task AddAsync(AuthToken authtoken);
    Task DeleteAsync(string token);
    Task UpdateAsync(AuthToken authtoken);
    Task<AuthToken?> GetByStringAsync(string token);
    Task<AuthToken[]> GetAllAsync();
}
