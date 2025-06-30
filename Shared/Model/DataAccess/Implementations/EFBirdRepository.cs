using Microsoft.EntityFrameworkCore;

namespace BirdWatching.Shared.Model;

public class EFBirdRepository : IBirdRepository
{
    private readonly AppDbContext _context;

    public IQueryable<Bird> BirdsWithDetails => _context.Birds;

    public EFBirdRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Bird bird)
    {
        if (bird is null)
            throw new ApplicationException("Invalid bird to add...");

        await _context.Birds.AddAsync(bird);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Bird bird)
    {
        var dbBird = await BirdsWithDetails.FirstOrDefaultAsync(b => b.Id == bird.Id);

        if (dbBird is null)
            throw new InvalidOperationException($"Bird with ID = {bird.Id} is not in the database and cannot be updated.");

        _context.Entry(dbBird).CurrentValues.SetValues(bird);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var bird = await GetByIdAsync(id);

        if (bird is null)
            throw new InvalidOperationException($"Bird with ID = {id} is not in the database and cannot be deleted.");

        _context.Birds.Remove(bird);
        await _context.SaveChangesAsync();
    }

    public async Task<Bird?> GetByIdAsync(int id)
    {
        return await BirdsWithDetails.FirstOrDefaultAsync(b => b.Id == id);
    }

    public async Task<Bird[]> GetAllAsync()
    {
        return await BirdsWithDetails.ToArrayAsync();
    }

    public async Task<Bird[]> GetByPrefixAsync(string prefix)
    {
        return await BirdsWithDetails
            .Where(b => EF.Functions.Like(
                (b.Genus ?? "") + " " +
                (b.Species ?? "") + " " +
                (b.Ordo ?? "") + " " +
                (b.Familia ?? ""),
                 prefix + "%"))
            .ToArrayAsync();
    }

    public async Task<Bird[]> GetByContainsAsync(string str)
    {
        return await BirdsWithDetails
            .Where(b => EF.Functions.Like(
                (b.Genus ?? "") + " " +
                (b.Species ?? "") + " " +
                (b.Ordo ?? "") + " " +
                (b.Familia ?? ""),
                "%" + str + "%"))
            .ToArrayAsync();
    }

    public async Task<Bird[]> GetByPrefixFastAsync(string prefix)
    {
        return await _context.Birds
            .Where(b => EF.Functions.Like(
                (b.Genus ?? "") + " " +
                (b.Species ?? "") + " " +
                (b.Ordo ?? "") + " " +
                (b.Familia ?? ""),
                 prefix + "%"))
            .ToArrayAsync();
    }
}
