namespace BirdWatching.Shared.Model;

using Microsoft.EntityFrameworkCore;

public class EFBirdRepository : IBirdRepository
{
    private AppDbContext _context;

    public EFBirdRepository(AppDbContext context)
    {
        _context = context;
    }

    public void Add(Bird bird)
    {
        _context.Birds.Add(bird);
        _context.SaveChanges();
    }

    public void Update(Bird bird)
    {
        var dbBird = _context.Birds.Find(bird.Id);

        if (dbBird is null)
            throw new InvalidOperationException($"Bird with ID = {bird.Id} is not in the database and cannot be updated.");
        dbBird = bird;
        _context.Update(dbBird);
        _context.SaveChanges();
    }

    public void Delete(int id)
    {
        var bird = GetById(id);

        if (bird is null)
            throw new InvalidOperationException($"Bird with ID = {id} is not in the database and cannot be deleted.");
        _context.Birds.Remove(bird);
        _context.SaveChanges();
    }

    public Bird? GetById(int id) => _context.Birds.Find(id);

    public IEnumerable<Bird> GetAll() => _context.Birds.ToList();
}
