namespace BirdWatching;

public class EFUserRepository : IUserRepository
{
    private AppDbContext _context;

    public EFUserRepository(AppDbContext context)
    {
        _context = context;
    }

    void Add(User user)
    {
        _context.Users.Add(user);
        _context.SaveChanges();
    }

    void Update(User user)
    {
        var dbUser = _context.Users.Find(user.Id);

        if (dbUser is null)
            throw new InvalidOperationException($"User with ID = {user.Id} is not in the database and cannot be updated.");
        dbUser = user;
        _context.Update(dbUser);
        _context.SaveChanges();
    }

    void Delete(int id)
    {
        var user = GetById(id);

        if (user is null)
            throw new InvalidOperationException($"User with ID = {id} is not in the database and cannot be deleted.");
        _context.Users.Remove(user);
        _context.SaveChanges();
    }

    User? GetById(int id) => _context.Users.Find(id);

    IEnumerable<User> GetAll() => _context.Users.ToList();
}
