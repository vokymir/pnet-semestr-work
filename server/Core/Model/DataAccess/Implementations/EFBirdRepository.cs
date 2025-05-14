namespace BirdWatching;

public class EFBirdRepository : IBirdRepository
{
    private BirdDbContext _context;

    public EFBirdRepository(BirdDbContext context)
    {
        _context = context;
    }

    void Add(Bird bird)
    {
        _context.Birds.Add(bird);
        _context.SaveChanges();
    }

    void Update(Bird bird)
    {
        var dbBird = _context.Birds.Find(bird.Id);

        if (dbBird is null)
            throw new InvalidOperationException($"Bird with ID = {bird.Id} is not in the database and cannot be updated.");
        dbBird = bird;
        _context.Update(dbBird);
        _context.SaveChanges();
    }

    void Delete(int id)
    {
        var bird = GetById(id);

        if (bird is null)
            throw new InvalidOperationException($"Bird with ID = {id} is not in the database and cannot be deleted.");
        _context.Birds.Remove(bird);
        _context.SaveChanges();
    }

    Bird? GetById(int id) => _context.Birds.Find(id);

    IEnumerable<Bird> GetAll() => _context.Birds.ToList();
}
