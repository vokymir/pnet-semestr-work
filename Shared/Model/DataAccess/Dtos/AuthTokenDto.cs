namespace BirdWatching.Shared.Model;

public class AuthTokenDto : IAmDto<AuthToken>
{
    public string Token { get; set; } = string.Empty;
    public DateTime Created { get; set; } = DateTime.Now;

    public int UserId { get; set; }
    public UserDto? User { get; set; }

    public AuthToken ToEntity()
    {
        var a = new AuthToken() {
            Token = Token,
            Created = Created,
        };

        if (User is not null)
            a.User = User.ToEntity();

        return a;
    }
}
