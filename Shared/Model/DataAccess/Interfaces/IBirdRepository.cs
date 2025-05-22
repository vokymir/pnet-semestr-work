namespace BirdWatching.Shared.Model;

public interface IBirdRepository
{
    public void Add(Bird bird);
    public void Delete(int id);
    public void Update(Bird bird);
    public Bird? GetById(int id);
    public IEnumerable<Bird> GetAll();
    public IEnumerable<Bird> GetByPrefix(string prefix);
    public IEnumerable<Bird> GetByPrefixFast(string prefix);
}
