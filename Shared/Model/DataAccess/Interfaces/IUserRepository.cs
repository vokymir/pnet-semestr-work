namespace BirdWatching.Shared.Model;

public interface IUserRepository
{
    IQueryable<User> UsersWithDetails { get; set; }

    Task AddAsync(User user);
    Task<bool> DeleteAsync(int id);
    Task UpdateAsync(User user);
    Task<User?> GetByIdAsync(int id);
    Task<User?> GetByUsernameAsync(string username);
    Task<User[]> GetAllAsync();
}
