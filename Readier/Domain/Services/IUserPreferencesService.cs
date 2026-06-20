using Readier.Models;

namespace Readier.Interfaces;

public interface IUserPreferencesService
{
    event EventHandler? PreferencesChanged;

    Task<UserPreferences> GetAsync();

    Task SaveAsync(UserPreferences preferences);
}
