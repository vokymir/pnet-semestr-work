using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

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

    public IQueryable<Record> RecordsWithBird {
        get {
            return _context.Records
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

    public async Task<Record[]> GetValidEventsWatcherRecordsAsync(int watcherId, string eventPubId)
    {
        // 1. Najdi event
        var e = await _context.Events
            .FirstOrDefaultAsync(ev => ev.PublicIdentifier == eventPubId);

        if (e is null)
            return Array.Empty<Record>();

        // 2. Načti záznamy pro daného uživatele a čas
        var records = await _context.Records
            .Include(r => r.Bird)
            .Where(r =>
                r.WatcherId == watcherId &&
                r.DateSeen >= e.Start &&
                r.DateSeen <= e.End)
            .ToListAsync();

        // 3. Připrav regexy
        var ordoRegex = new Regex(SanitizeRegex(e.OrdoRegex), RegexOptions.IgnoreCase);
        var familiaRegex = new Regex(SanitizeRegex(e.FamiliaRegex), RegexOptions.IgnoreCase);
        var genusRegex = new Regex(SanitizeRegex(e.GenusRegex), RegexOptions.IgnoreCase);
        var speciesRegex = new Regex(SanitizeRegex(e.SpeciesRegex), RegexOptions.IgnoreCase);

        // 4. Filtrování podle regexů
        var validRecords = records
            .Where(r =>
                r.Bird is not null &&
                ordoRegex.IsMatch(r.Bird.Ordo ?? "") &&
                familiaRegex.IsMatch(r.Bird.Familia ?? "") &&
                genusRegex.IsMatch(r.Bird.Genus ?? "") &&
                speciesRegex.IsMatch(r.Bird.Species ?? ""))
            .ToList();

        // 5. Pokud nejsou povolené duplicity, omezíme na první výskyt každého ptáka
        if (!e.AllowDuplicates)
        {
            validRecords = validRecords
                .GroupBy(r => r.Bird.Id)
                .Select(g => g.OrderBy(r => r.DateSeen).First()) // první výskyt
                .ToList();
        }

        return validRecords.ToArray();
    }

    private string SanitizeRegex(string? pattern)
    {
        if (string.IsNullOrWhiteSpace(pattern) || !IsValidRegex(pattern))
            return ".*"; // match anything

        return pattern;
    }

    private bool IsValidRegex(string pattern)
    {
        try
        {
            _ = Regex.Match("", pattern);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
