namespace Readier.Models;

public class Schedule
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Title { get; set; } = string.Empty;

    public Place? Origin { get; set; }

    public Place? Destination { get; set; }

    public DateTime StartTime { get; set; }

    public int EstimatedTravelMinutes { get; set; }

    public int EstimatedPrepMinutes { get; set; }

    public TransportationMode Transportation { get; set; } = TransportationMode.PublicTransit;
}
