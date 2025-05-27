using Microsoft.EntityFrameworkCore;

namespace BirdWatching.Shared.Model;

public class EFRecordRepository : IRecordRepository
{
    private readonly AppDbContext _context;

    public IQueryable<Record> RecordsWithDetails {
        get {
            return _context.Records
                .Include(r => r.Watcher)
                .Include(r => r.Bird);
        }
        set { }
    }

    public EFRecordRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Record record)
    {
        await _context.Records.AddAsync(record);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Record record)
    {
        var dbRecord = await RecordsWithDetails.FirstOrDefaultAsync(r => r.Id == record.Id);
        if (dbRecord == null)
            throw new InvalidOperationException($"Record with ID = {record.Id} is not in the database and cannot be updated.");

        _context.Entry(dbRecord).CurrentValues.SetValues(record);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var record = await GetByIdAsync(id);
        if (record == null)
            throw new InvalidOperationException($"Record with ID = {id} is not in the database and cannot be deleted.");

        _context.Records.Remove(record);
        await _context.SaveChangesAsync();
    }

    public async Task<Record?> GetByIdAsync(int id)
    {
        return await RecordsWithDetails.FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<Record[]> GetAllAsync()
    {
        return await RecordsWithDetails.ToArrayAsync();
    }

    public async Task<Record[]> GetWatcherRecordsAsync(int watcherId)
    {
        return await RecordsWithDetails
            .Where(r => r.WatcherId == watcherId)
            .ToArrayAsync();
    }
}
