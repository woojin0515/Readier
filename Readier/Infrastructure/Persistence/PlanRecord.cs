using Readier.Models;

namespace Readier.Infrastructure.Persistence;

public class PlanRecord
{
    public long Id { get; set; }

    public string UserId { get; set; } = string.Empty;

    public Guid PlanId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? OriginJson { get; set; }

    public string? DestinationJson { get; set; }

    public DateTime StartTime { get; set; }

    public int EstimatedTravelMinutes { get; set; }

    public int EstimatedPrepMinutes { get; set; }

    public TransportationMode Transportation { get; set; } = TransportationMode.PublicTransit;

    public DateTime UpdatedUtc { get; set; } = DateTime.UtcNow;
}
