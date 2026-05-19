namespace Readier.Models;

public record PlaceSearchResult(
    string Name,
    string Address,
    string RoadAddress,
    double Latitude,
    double Longitude)
{
    public string DisplayLine => string.IsNullOrWhiteSpace(Name) ? Address : Name;

    public string SubLine => string.IsNullOrWhiteSpace(RoadAddress) ? Address : RoadAddress;

    public bool HasCoordinates => Latitude != 0 || Longitude != 0;
}
