namespace BirdWatching.Shared.Model;

using Microsoft.EntityFrameworkCore;

public class EFBirdRepository : IBirdRepository
{
    private AppDbContext _context;

    public IQueryable<Bird> BirdsWithDetails {
        get {
            return _context.Birds
                .Include(b => b.Id); // unnecessary, to avoid warning of not using EF
        }
        set { }
    }

    public EFBirdRepository(AppDbContext context)
    {
        _context = context;
    }

    public void Add(Bird bird)
    {
        if (bird is null) throw new ApplicationException("Invalid bird to add...");

        _context.Birds.Add((Bird) bird);
        _context.SaveChanges();
    }

    public void Update(Bird bird)
    {
        var dbBird = BirdsWithDetails.First(b => b.Id == bird.Id);

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

    public Bird? GetById(int id) => BirdsWithDetails.First(b => b.Id == id);

    public IEnumerable<Bird> GetByPrefix(string prefix) => BirdsWithDetails.Where(b => b.FullName.StartsWith(prefix)).ToArray();
    public IEnumerable<Bird> GetByPrefixFast(string prefix) => _context.Birds.Where(b => b.FullName.StartsWith(prefix)).ToArray();

    public IEnumerable<Bird> GetAll() => BirdsWithDetails.ToArray();
}
