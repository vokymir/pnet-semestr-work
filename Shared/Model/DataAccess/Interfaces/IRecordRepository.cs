namespace BirdWatching.Shared.Model;

public interface IRecordRepository
{
    public void Add(Record record);
    public void Delete(int id);
    public void Update(Record record);
    public Record? GetById(int id);
    public IEnumerable<Record> GetAll();
}
