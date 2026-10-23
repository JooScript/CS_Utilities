using System.Text.Json;
using Utils.General;
using Utils.SearchProviders.Contracts;
using Utils.SearchProviders.Models;

namespace Utils.SearchProviders.Services;

/// <summary>
/// Google Programmable Search (Custom Search Engine) image provider.
/// Official and stable, but requires credentials via environment variables:
///   GOOGLE_CSE_API_KEY, GOOGLE_CSE_ID.
/// Used automatically when both are present.
/// </summary>
public sealed class GoogleCseSearchService(HttpClient http, int maxRetries, string googleCseApiKey, string googleCseId) : IImageSearchService
{
    public string Name => "Google Custom Search";

    public async Task<IReadOnlyList<ImageCandidate>> SearchImageAsync(string query, CancellationToken cancellationToken)
    {
        var url =
            "https://www.googleapis.com/customsearch/v1?key=" +
            Uri.EscapeDataString(googleCseApiKey!) +
            "&cx=" + Uri.EscapeDataString(googleCseId!) +
            "&searchType=image&num=10&q=" + Uri.EscapeDataString(query);

        var json = await RetryPolicy.ExecuteAsync(maxRetries, TimeSpan.FromSeconds(2), async () =>
        {
            using var response = await http.GetAsync(url, cancellationToken).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException(
                    $"Google CSE returned {(int)response.StatusCode} for \"{query}\"");
            }
            return await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        }, cancellationToken);

        using var document = JsonDocument.Parse(json);
        if (!document.RootElement.TryGetProperty("items", out var items) ||
            items.ValueKind != JsonValueKind.Array)
        {
            return Array.Empty<ImageCandidate>();
        }

        var results = new List<ImageCandidate>();
        foreach (var item in items.EnumerateArray())
        {
            var imageUrl = item.TryGetProperty("link", out var link) ? link.GetString() : null;
            var sourcePage =
                item.TryGetProperty("image", out var image) &&
                image.TryGetProperty("contextLink", out var contextLink)
                    ? contextLink.GetString()
                    : null;
            var title = item.TryGetProperty("title", out var titleProp) ? titleProp.GetString() : null;

            if (string.IsNullOrWhiteSpace(imageUrl)) continue;

            results.Add(new ImageCandidate
            {
                Title = title ?? "",
                ImageUrl = imageUrl,
                SourceUrl = sourcePage ?? "",
                Query = query,
                Width = null,
                Height = null,
                Provider = "GoogleCSE"
            });
        }

        return results;
    }
}
