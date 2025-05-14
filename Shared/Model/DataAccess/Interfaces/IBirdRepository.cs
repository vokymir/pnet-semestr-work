namespace BirdWatching;

public interface IBirdRepository
{
    void Add(Bird bird);
    void Delete(int id);
    void Update(Bird bird);
    Bird? Get(int id);
    IEnumerable<Bird> GetAll();
}
