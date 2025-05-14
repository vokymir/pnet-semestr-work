namespace BirdWatching;

public interface IUserRepository
{
    void Add(User user);
    void Delete(int id);
    void Update(User user);
    User? GetById(int id);
    IEnumerable<User> GetAll();
}
