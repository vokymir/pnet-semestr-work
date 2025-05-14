namespace BirdWatching.Shared.Model;

public interface IUserRepository
{
    public void Add(User user);
    public void Delete(int id);
    public void Update(User user);
    public User? GetById(int id);
    public IEnumerable<User> GetAll();
}
