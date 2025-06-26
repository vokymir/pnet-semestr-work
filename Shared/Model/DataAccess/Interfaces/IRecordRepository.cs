namespace BirdWatching.Shared.Model;

public interface IRecordRepository
{
    Task AddAsync(Record record);
    Task DeleteAsync(int id);
    Task UpdateAsync(Record record);
    Task<Record?> GetByIdAsync(int id);
    Task<Record[]> GetAllAsync();
    Task<Record[]> GetWatcherRecordsAsync(int watcherId);
    Task<Record[]> GetValidEventsWatcherRecordsAsync(int watcherId, string eventPubId);
}
