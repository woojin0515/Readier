using System.Globalization;
using System.Text.Json;
using Readier.Interfaces;
using Readier.Models;

namespace Readier.Services;

public class KakaoTravelTimeProvider : ITravelTimeProvider
{
    private const string DirectionEndpoint = "https://apis-navi.kakaomobility.com/v1/directions";

    private readonly HttpClient _http;

    public KakaoTravelTimeProvider(HttpClient http)
    {
        _http = http;
    }

    public async Task<TravelTimeEstimate?> EstimateAsync(
        Place origin,
        Place destination,
        TransportationMode mode,
        CancellationToken cancellationToken = default)
    {
        if (origin is null || destination is null) return null;
        if (!origin.HasCoordinates || !destination.HasCoordinates) return null;
        if (!KakaoApiKeyResolver.HasKey) return null;

        var (drivingMinutes, distanceKm) = await CallDirectionAsync(origin, destination, cancellationToken);
        if (drivingMinutes is null) return null;

        var minutes = mode switch
        {
            TransportationMode.Car => drivingMinutes.Value,
            TransportationMode.Taxi => drivingMinutes.Value,
            TransportationMode.PublicTransit => (int)Math.Round(drivingMinutes.Value * 1.3),
            TransportationMode.Walking => (int)Math.Round(distanceKm * 12),
            TransportationMode.Bicycle => (int)Math.Round(distanceKm * 4),
            _ => drivingMinutes.Value
        };

        var note = mode is TransportationMode.PublicTransit or TransportationMode.Walking or TransportationMode.Bicycle
            ? "근사치"
            : string.Empty;

        return new TravelTimeEstimate(Math.Max(1, minutes), distanceKm, note);
    }

    private async Task<(int? Minutes, double DistanceKm)> CallDirectionAsync(
        Place origin,
        Place destination,
        CancellationToken cancellationToken)
    {
        var originParam = FormatCoord(origin);
        var destParam = FormatCoord(destination);
        var url = $"{DirectionEndpoint}?origin={originParam}&destination={destParam}&priority=RECOMMEND";

        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Add("Authorization", $"KakaoAK {KakaoApiKeyResolver.GetKey()}");

        try
        {
            using var response = await _http.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode) return (null, 0);

            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            using var document = JsonDocument.Parse(json);
            if (!document.RootElement.TryGetProperty("routes", out var routes)) return (null, 0);
            if (routes.GetArrayLength() == 0) return (null, 0);

            var route = routes[0];
            if (route.TryGetProperty("result_code", out var code) && code.GetInt32() != 0) return (null, 0);
            if (!route.TryGetProperty("summary", out var summary)) return (null, 0);

            var durationSec = summary.GetProperty("duration").GetInt64();
            var distanceM = summary.GetProperty("distance").GetInt64();
            var minutes = (int)Math.Round(durationSec / 60.0);
            var distanceKm = distanceM / 1000.0;
            return (minutes, distanceKm);
        }
        catch
        {
            return (null, 0);
        }
    }

    private static string FormatCoord(Place place)
        => string.Format(
            CultureInfo.InvariantCulture,
            "{0},{1}",
            place.Longitude,
            place.Latitude);
}
