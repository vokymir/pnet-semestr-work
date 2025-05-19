namespace BirdWatching.Shared.Model;

public interface IUserRepository
{
    public IQueryable<User> UsersWithDetails { get; set; }
    public void Add(User user);
    public void Delete(int id);
    public void Update(User user);
    public User? GetById(int id);
    public User? GetByUsername(string username);
    public IEnumerable<User> GetAll();
}
