namespace BirdWatching;

public interface IRepository<T>
{
    public void Add(T item);
    public void Update(T item);
    public void Delete(T item);
    public T? GetByID(int id);
    public IReadOnlyCollection<T> GetAll();
}
