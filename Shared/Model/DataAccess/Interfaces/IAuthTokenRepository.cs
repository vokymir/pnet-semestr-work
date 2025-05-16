namespace BirdWatching.Shared.Model;

public interface IAuthTokenRepository
{
    public void Add(AuthToken authtoken);
    public void Delete(string token);
    public void Update(AuthToken authtoken);
    public AuthToken? GetByString(string token);
    public IEnumerable<AuthToken> GetAll();
}
