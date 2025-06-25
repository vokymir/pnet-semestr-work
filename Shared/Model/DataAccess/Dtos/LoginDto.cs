namespace BirdWatching.Shared.Model;

public class LoginDto()
{
    public string username { get; set; } = "";
    public string passwordhash { get; set; } = "";
    public int WantedMinutes { get; set; } = 0;
}
