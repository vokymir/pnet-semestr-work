namespace BirdWatching.Shared.Model;

public class AuthTokenDto : IAmDto<AuthToken>
{
    public string Token { get; set; } = string.Empty;
    public DateTime Created { get; set; } = DateTime.Now;

    public UserDto User { get; set; } = null!;

    public AuthToken ToEntity()
    {
        var a = new AuthToken() {
            Token = Token,
            Created = Created,
            User = User.ToEntity()
        };

        return a;
    }
}
