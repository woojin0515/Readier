using Readier.Models;

namespace Readier.Interfaces;

public interface IUserPreferencesService
{
    Task<UserPreferences> GetAsync();

    Task SaveAsync(UserPreferences preferences);
}
