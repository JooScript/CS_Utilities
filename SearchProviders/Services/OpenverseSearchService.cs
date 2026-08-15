using System.Text.Json;
using Utils.General;
using Utils.SearchProvider.Models;
using Utils.SearchProviders.Contracts;

namespace ImageRecovery.Services;

/// <summary>
/// Openverse image search API (https://openverse.org). A genuinely free, key-less
/// JSON API. Results are filtered to commercially usable licenses.
/// It mostly aggregates Flickr/Wikimedia photos, so it is used as a fallback
/// provider; the validation layer keeps such low-trust sources from being chosen
/// unless nothing better exists.
/// </summary>
public sealed class OpenverseSearchService(HttpClient http, int maxRetries) : IImageSearchService
{
    public string Name => "Openverse API";

    public async Task<IReadOnlyList<ImageCandidate>> SearchImageAsync(string query, CancellationToken cancellationToken)
    {
        var url =
            "https://api.openverse.org/v1/images/?q=" + Uri.EscapeDataString(query) +
            "&page_size=10&license_type=commercial";

        var json = await RetryPolicy.ExecuteAsync(maxRetries, TimeSpan.FromSeconds(2), async () =>
        {
            using var response = await http.GetAsync(url, cancellationToken).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException(
                    $"Openverse returned {(int)response.StatusCode} for \"{query}\"");
            }
            return await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        }, cancellationToken);

        return ParseResults(json, query);
    }

    private static IReadOnlyList<ImageCandidate> ParseResults(string json, string query)
    {
        using var document = JsonDocument.Parse(json);
        if (!document.RootElement.TryGetProperty("results", out var results) ||
            results.ValueKind != JsonValueKind.Array)
        {
            return Array.Empty<ImageCandidate>();
        }

        var candidates = new List<ImageCandidate>();
        foreach (var item in results.EnumerateArray())
        {
            if (item.ValueKind != JsonValueKind.Object) continue;

            var imageUrl = GetString(item, "url");
            if (string.IsNullOrWhiteSpace(imageUrl)) continue;

            candidates.Add(new ImageCandidate
            {
                Title = GetString(item, "title") ?? "",
                ImageUrl = imageUrl,
                SourceUrl = GetString(item, "foreign_landing_url") ?? "",
                Query = query,
                Width = GetInt(item, "width"),
                Height = GetInt(item, "height"),
                Provider = "Openverse"
            });
        }

        return candidates;
    }

    private static string? GetString(JsonElement element, string name) =>
        element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;

    private static int? GetInt(JsonElement element, string name) =>
        element.TryGetProperty(name, out var value) &&
        (value.ValueKind == JsonValueKind.Number || value.ValueKind == JsonValueKind.String) &&
        int.TryParse(value.ToString(), out var parsed)
            ? parsed
            : null;
}
