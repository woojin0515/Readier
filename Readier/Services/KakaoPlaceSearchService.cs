using System.Globalization;
using System.Text.Json;
using Readier.Interfaces;
using Readier.Models;

namespace Readier.Services;

public class KakaoPlaceSearchService : IPlaceSearchService
{
    private const string KeywordEndpoint = "https://dapi.kakao.com/v2/local/search/keyword.json";
    private const string AddressEndpoint = "https://dapi.kakao.com/v2/local/search/address.json";

    private readonly HttpClient _http;

    public KakaoPlaceSearchService(HttpClient http)
    {
        _http = http;
    }

    public async Task<IReadOnlyList<PlaceSearchResult>> SearchAsync(string query, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query)) return Array.Empty<PlaceSearchResult>();
        if (!ApiKeys.HasKakaoKey) return Array.Empty<PlaceSearchResult>();

        var url = $"{KeywordEndpoint}?query={Uri.EscapeDataString(query)}&size=10";
        using var request = BuildRequest(url);

        try
        {
            using var response = await _http.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode) return Array.Empty<PlaceSearchResult>();

            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            using var document = JsonDocument.Parse(json);
            if (!document.RootElement.TryGetProperty("documents", out var documents)) return Array.Empty<PlaceSearchResult>();

            var list = new List<PlaceSearchResult>();
            foreach (var item in documents.EnumerateArray())
            {
                var name = item.TryGetProperty("place_name", out var n) ? n.GetString() ?? "" : "";
                var address = item.TryGetProperty("address_name", out var a) ? a.GetString() ?? "" : "";
                var roadAddress = item.TryGetProperty("road_address_name", out var r) ? r.GetString() ?? "" : "";
                TryParseCoord(item, "y", out var lat);
                TryParseCoord(item, "x", out var lng);

                if (!string.IsNullOrWhiteSpace(name) || !string.IsNullOrWhiteSpace(address))
                    list.Add(new PlaceSearchResult(name, address, roadAddress, lat, lng));
            }
            return list;
        }
        catch
        {
            return Array.Empty<PlaceSearchResult>();
        }
    }

    public async Task<Place?> GeocodeAsync(string address, string? displayName = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(address)) return null;
        if (!ApiKeys.HasKakaoKey) return null;

        var url = $"{AddressEndpoint}?query={Uri.EscapeDataString(address)}&size=1";
        using var request = BuildRequest(url);

        try
        {
            using var response = await _http.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode) return null;

            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            using var document = JsonDocument.Parse(json);
            if (!document.RootElement.TryGetProperty("documents", out var documents)) return null;
            if (documents.GetArrayLength() == 0) return null;

            var first = documents[0];
            if (!TryParseCoord(first, "y", out var lat)) return null;
            if (!TryParseCoord(first, "x", out var lng)) return null;

            return new Place
            {
                Name = displayName ?? string.Empty,
                Address = address,
                Latitude = lat,
                Longitude = lng
            };
        }
        catch
        {
            return null;
        }
    }

    private static HttpRequestMessage BuildRequest(string url)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Add("Authorization", $"KakaoAK {ApiKeys.KakaoNativeAppKey}");
        return request;
    }

    private static bool TryParseCoord(JsonElement element, string property, out double value)
    {
        value = 0;
        if (!element.TryGetProperty(property, out var node)) return false;
        var text = node.ValueKind == JsonValueKind.String ? node.GetString() : node.ToString();
        return double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
    }
}
