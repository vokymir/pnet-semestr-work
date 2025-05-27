using Microsoft.EntityFrameworkCore;

namespace BirdWatching.Shared.Model;

public class EFAuthTokenRepository : IAuthTokenRepository
{
    private readonly AppDbContext _context;

    public IQueryable<AuthToken> AuthTokensWithDetails => _context.AuthTokens.Include(a => a.User);

    public EFAuthTokenRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(AuthToken authtoken)
    {
        await _context.AuthTokens.AddAsync(authtoken);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(AuthToken authtoken)
    {
        var dbAuthToken = await AuthTokensWithDetails
            .FirstOrDefaultAsync(a => a.Token == authtoken.Token);

        if (dbAuthToken is null)
            throw new InvalidOperationException($"AuthToken {authtoken.Token} is not in the database and cannot be updated.");

        _context.Entry(dbAuthToken).CurrentValues.SetValues(authtoken);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(string token)
    {
        var authtoken = await GetByStringAsync(token);

        if (authtoken is null)
            throw new InvalidOperationException($"AuthToken {token} is not in the database and cannot be deleted.");

        _context.AuthTokens.Remove(authtoken);
        await _context.SaveChangesAsync();
    }

    public async Task<AuthToken?> GetByStringAsync(string token)
    {
        var tkn = await AuthTokensWithDetails.FirstOrDefaultAsync(a => a.Token == token);
        return tkn;
    }

    public async Task<AuthToken[]> GetAllAsync()
    {
        return await AuthTokensWithDetails.ToArrayAsync();
    }
}
