namespace Client.Pages;

using BirdWatching.Shared.Model;

public partial class Watchers
{
    private ICollection<WatcherDto>? W = null;

    protected override async Task OnInitializedAsync()
    {
        W = await BAC.Watcher_GetUserWatchersAsync();
    }
}
