using Readier.Models;

namespace Readier.Helpers;

public record TransportationOption(TransportationMode Mode, string Label);

public static class TransportationCatalog
{
    public static IReadOnlyList<TransportationOption> All { get; } = new[]
    {
        new TransportationOption(TransportationMode.Walking, "도보"),
        new TransportationOption(TransportationMode.Bicycle, "자전거"),
        new TransportationOption(TransportationMode.PublicTransit, "대중교통"),
        new TransportationOption(TransportationMode.Car, "자동차"),
        new TransportationOption(TransportationMode.Taxi, "택시")
    };

    public static TransportationOption FromMode(TransportationMode mode)
        => All.FirstOrDefault(o => o.Mode == mode) ?? All[0];
}
