namespace BirdWatching.Shared.Model;

public class AuthTokenDto : IAmDto<AuthToken>
{
    public string Token { get; set; } = string.Empty;
    public DateTime Created { get; set; } = DateTime.Now;

    public int UserId { get; set; }

    public AuthToken ToEntity()
    {
        var a = new AuthToken() {
            Token = Token,
            Created = Created,
        };

        return a;
    }
}
