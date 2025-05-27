using Microsoft.EntityFrameworkCore;

namespace BirdWatching.Shared.Model;

public class EFUserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public IQueryable<User> UsersWithDetails {
        get {
            return _context.Users
                .Include(u => u.CuratedWatchers)
                .Include(u => u.Watchers)
                .Include(u => u.Events)
                .Include(u => u.AdministeredEvents);
        }
        set { }
    }

    public EFUserRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(User user)
    {
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(User user)
    {
        var dbUser = await UsersWithDetails.FirstOrDefaultAsync(u => u.Id == user.Id);
        if (dbUser == null)
            throw new InvalidOperationException($"User with ID = {user.Id} is not in the database and cannot be updated.");

        _context.Entry(dbUser).CurrentValues.SetValues(user);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var user = await GetByIdAsync(id);
        if (user == null)
            return false;

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<User?> GetByIdAsync(int id)
    {
        return await UsersWithDetails.FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await UsersWithDetails.FirstOrDefaultAsync(u => u.UserName == username);
    }

    public async Task<User[]> GetAllAsync()
    {
        return await UsersWithDetails.ToArrayAsync();
    }
}
