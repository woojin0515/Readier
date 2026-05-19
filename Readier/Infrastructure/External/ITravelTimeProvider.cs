using Readier.Models;

namespace Readier.Interfaces;

public interface ITravelTimeProvider
{
    Task<TravelTimeEstimate?> EstimateAsync(Place origin, Place destination, TransportationMode mode, CancellationToken cancellationToken = default);
}
