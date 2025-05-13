namespace BirdWatching;

public interface IRepository
{
    public void CreateBird(Bird bird);
    public void UpdateBird(Bird bird);
    public Bird ReadBird(int id);
    public void DeleteBird(Bird bird);
    public ICollection<Bird> ReadAllBirds();
}
