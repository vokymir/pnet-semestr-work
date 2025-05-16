namespace BirdWatching.Shared.Model;

public class EFAuthTokenRepository : IAuthTokenRepository
{
    private AppDbContext _context;

    public EFAuthTokenRepository(AppDbContext context)
    {
        _context = context;
    }

    public void Add(AuthToken authtoken)
    {
        _context.AuthTokens.Add(authtoken);
        _context.SaveChanges();
    }

    public void Update(AuthToken authtoken)
    {
        var dbAuthToken = _context.AuthTokens.Find(authtoken.Token);

        if (dbAuthToken is null)
            throw new InvalidOperationException($"AuthToken {authtoken.Token} is not in the database and cannot be updated.");
        dbAuthToken = authtoken;

        _context.Update(dbAuthToken);
        _context.SaveChanges();
    }

    public void Delete(string token)
    {
        var authtoken = GetByString(token);

        if (authtoken is null)
            throw new InvalidOperationException($"AuthToken {token} is not in the database and cannot be deleted.");

        _context.AuthTokens.Remove(authtoken);
        _context.SaveChanges();
    }

    public AuthToken? GetByString(string token)
    {
        var tkn = _context.AuthTokens.Find(token);
        if (tkn is null) return null;

        _context.Entry(tkn).Reference("User").Load();
        return tkn;

    }

    public IEnumerable<AuthToken> GetAll() => _context.AuthTokens.ToList();
}
