namespace BirdWatching.Shared.Model;

using Microsoft.EntityFrameworkCore;

[PrimaryKey("Token")]
public class AuthToken
{
    public string Token { get; set; } = string.Empty;
    public DateTime Created { get; set; } = DateTime.Now;
    public User User { get; set; } = null!;
}
