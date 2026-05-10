namespace Readier.Models;

public class Schedule
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Title { get; set; } = string.Empty;

    public string Location { get; set; } = string.Empty;

    public DateTime StartTime { get; set; }

    public int EstimatedTravelMinutes { get; set; }

    public int EstimatedPrepMinutes { get; set; }
}
