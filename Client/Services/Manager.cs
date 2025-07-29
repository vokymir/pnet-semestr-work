namespace Client;

using System.Threading.Tasks;
using Blazored.LocalStorage;
using BirdWatching.Shared.Api;
using BirdWatching.Shared.Model;

public class Manager
{
    public static async Task ReloadUserDto(ILocalStorageService storage, BirdApiClient api)
    {
        var user = await storage.GetItemAsync<UserDto>("userDto");
        if (user is null)
            throw new Exception("Logged out.");

        var updatedUser = await api.User_GetUserAsync(user.Id);
        await storage.SetItemAsync("userDto", updatedUser);
    }
}
