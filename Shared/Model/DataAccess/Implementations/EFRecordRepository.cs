namespace BirdWatching.Shared.Model;

// using Microsoft.EntityFrameworkCore;

public class EFRecordRepository : IRecordRepository
{
    private AppDbContext _context;

    public EFRecordRepository(AppDbContext context)
    {
        _context = context;
    }

    public void Add(Record record)
    {
        _context.Records.Add(record);
        _context.SaveChanges();
    }

    public void Update(Record record)
    {
        var dbRecord = _context.Records.Find(record.Id);

        if (dbRecord is null)
            throw new InvalidOperationException($"Record with ID = {record.Id} is not in the database and cannot be updated.");
        dbRecord = record;
        _context.Update(dbRecord);
        _context.SaveChanges();
    }

    public void Delete(int id)
    {
        var record = GetById(id);

        if (record is null)
            throw new InvalidOperationException($"Record with ID = {id} is not in the database and cannot be deleted.");
        _context.Records.Remove(record);
        _context.SaveChanges();
    }

    public Record? GetById(int id) => _context.Records.Find(id);

    public IEnumerable<Record> GetAll() => _context.Records.ToList();

    public IEnumerable<Record> GetWatcherRecords(int watcherId) => _context.Records.Where(r => r.WatcherId == watcherId).ToArray();
}
