using Readier.Models;

namespace Readier.Interfaces;

public interface IPlaceSearchService
{
    Task<IReadOnlyList<PlaceSearchResult>> SearchAsync(string query, CancellationToken cancellationToken = default);

    Task<Place?> GeocodeAsync(string address, string? displayName = null, CancellationToken cancellationToken = default);
}
