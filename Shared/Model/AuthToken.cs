namespace BirdWatching.Shared.Model;

using Microsoft.EntityFrameworkCore;

[PrimaryKey("Token")]
public class AuthToken : IHaveDto<AuthTokenDto>
{
    public string Token { get; set; } = string.Empty;
    public DateTime Created { get; set; } = DateTime.Now;

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public AuthTokenDto ToDto()
    {
        var a = new AuthTokenDto() {
            Token = Token,
            Created = Created,
            UserId = UserId
        };

        return a;
    }

    public AuthTokenDto ToFullDto()
    {
        var a = new AuthTokenDto() {
            Token = Token,
            Created = Created,
            UserId = UserId,
            User = User.ToDto()
        };

        return a;
    }
}
