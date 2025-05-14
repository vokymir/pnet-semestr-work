namespace BirdWatching.Shared.Model;

public interface IBirdRepository
{
    public void Add(Bird bird);
    public void Delete(int id);
    public void Update(Bird bird);
    public Bird? GetById(int id);
    public IEnumerable<Bird> GetAll();
}
