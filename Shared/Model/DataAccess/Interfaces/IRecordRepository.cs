namespace BirdWatching;

public interface IRecordRepository
{
    void Add(Record record);
    void Delete(int id);
    void Update(Record record);
    Record? GetById(int id);
    IEnumerable<Record> GetAll();
}
