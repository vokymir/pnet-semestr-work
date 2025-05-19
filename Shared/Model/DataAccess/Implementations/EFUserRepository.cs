namespace BirdWatching.Shared.Model;
using Microsoft.EntityFrameworkCore;

public class EFUserRepository : IUserRepository
{
    private AppDbContext _context;

    public IQueryable<User> UsersWithDetails {
        get {
            return _context.Users
                .Include(u => u.CuratedWatchers)
                .Include(u => u.Watchers)
                .Include(u => u.Events)
                .Include(u => u.AdministeredEvents)
                .Include(u => u.AuthTokens);
        }
        set { }
    }

    public EFUserRepository(AppDbContext context)
    {
        _context = context;
    }

    public void Add(User user)
    {
        _context.Users.Add(user);
        _context.SaveChanges();
    }

    public void Update(User user)
    {
        var dbUser = _context.Users.Find(user.Id);

        if (dbUser is null)
            throw new InvalidOperationException($"User with ID = {user.Id} is not in the database and cannot be updated.");
        dbUser = user;
        _context.Update(dbUser);
        _context.SaveChanges();
    }

    public void Delete(int id)
    {
        var user = GetById(id);

        if (user is null)
            throw new InvalidOperationException($"User with ID = {id} is not in the database and cannot be deleted.");
        _context.Users.Remove(user);
        _context.SaveChanges();
    }

    public User? GetById(int id) => UsersWithDetails.First(u => u.Id == id);

    public User? GetByUsername(string username) => UsersWithDetails.First(u => u.UserName == username);

    public IEnumerable<User> GetAll() => UsersWithDetails.ToList();
}
