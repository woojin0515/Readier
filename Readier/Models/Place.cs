namespace Readier.Models;

public class Place
{
    public string Name { get; set; } = string.Empty;

    public string Address { get; set; } = string.Empty;

    public double Latitude { get; set; }

    public double Longitude { get; set; }

    public bool HasCoordinates => Latitude != 0 || Longitude != 0;

    public string DisplayLine => string.IsNullOrWhiteSpace(Name) ? Address : Name;
}
