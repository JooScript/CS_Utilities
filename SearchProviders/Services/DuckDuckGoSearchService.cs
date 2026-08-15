using System.Text.Json;
using System.Text.RegularExpressions;
using Utils.General;
using Utils.SearchProvider.Models;
using Utils.SearchProviders.Contracts;

namespace Utils.SearchProviders.Services;


public sealed class DuckDuckGoSearchService : IImageSearchService
{
    private static readonly string[] VqdPatterns =
    {
        "\"vqd\"\\s*:\\s*\"([0-9a-zA-Z\\-]+)\"",
        "vqd=\"([0-9a-zA-Z\\-]+)\"",
        "vqd='([0-9a-zA-Z\\-]+)'",
        "vqd=([0-9a-zA-Z\\-]+)[,;\\s]"
    };

    private readonly HttpClient _http;

    private readonly int _maxRetries;

    public string Name => "DuckDuckGo";

    public DuckDuckGoSearchService(HttpClient http, int maxRetries)
    {
        _http = http;
        _maxRetries = maxRetries;
    }

    public async Task<IReadOnlyList<ImageCandidate>> SearchImageAsync(string query, CancellationToken cancellationToken)
    {
        var vqd = await GetVqdAsync(query, cancellationToken);

        var searchUrl =
            "https://duckduckgo.com/i.js?l=us-en&o=json&f=,,,,&p=1&q=" +
            Uri.EscapeDataString(query) +
            "&vqd=" + Uri.EscapeDataString(vqd);

        var json = await RetryPolicy.ExecuteAsync(_maxRetries, TimeSpan.FromSeconds(2), async () =>
        {
            using var response = await _http.GetAsync(searchUrl, cancellationToken).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException(
                    $"DuckDuckGo search returned {(int)response.StatusCode} for \"{query}\"");
            }
            return await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        }, cancellationToken);

        return ParseResults(json, query);
    }

    private async Task<string> GetVqdAsync(string query, CancellationToken cancellationToken)
    {
        var pageUrl =
            "https://duckduckgo.com/?q=" + Uri.EscapeDataString(query) +
            "&iax=images&ia=images";

        var html = await RetryPolicy.ExecuteAsync(_maxRetries, TimeSpan.FromSeconds(2), async () =>
        {
            using var response = await _http.GetAsync(pageUrl, cancellationToken).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException(
                    $"DuckDuckGo page returned {(int)response.StatusCode} for \"{query}\"");
            }
            return await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        }, cancellationToken);

        foreach (var pattern in VqdPatterns)
        {
            var match = Regex.Match(html, pattern, RegexOptions.IgnoreCase);
            if (match.Success) return match.Groups[1].Value;
        }

        throw new InvalidOperationException(
            "Could not extract the DuckDuckGo vqd token. The provider layout may have changed.");
    }

    private static IReadOnlyList<ImageCandidate> ParseResults(string json, string query)
    {
        var results = new List<ImageCandidate>();
        using var document = JsonDocument.Parse(json);

        var root = document.RootElement;
        IEnumerable<JsonElement> items;

        if (root.ValueKind == JsonValueKind.Array)
        {
            items = root.EnumerateArray();
        }
        else if (root.TryGetProperty("results", out var resultsProp) && resultsProp.ValueKind == JsonValueKind.Array)
        {
            items = resultsProp.EnumerateArray();
        }
        else
        {
            return Array.Empty<ImageCandidate>();
        }

        foreach (var item in items)
        {
            if (item.ValueKind != JsonValueKind.Object) continue;

            var imageUrl = GetString(item, "image", "m", "thumbnail");
            var title = GetString(item, "title", "t", "an");
            var sourcePage = GetString(item, "url", "u", "source");

            if (string.IsNullOrWhiteSpace(imageUrl)) continue;

            results.Add(new ImageCandidate
            {
                Title = title ?? "",
                ImageUrl = CleanRedirect(imageUrl),
                SourceUrl = CleanRedirect(sourcePage ?? ""),
                Query = query,
                Width = GetInt(item, "width", "w"),
                Height = GetInt(item, "height", "h"),
                Provider = "DuckDuckGo"
            });
        }

        return results;
    }

    /// <summary>
    /// Normalizes protocol-relative URLs and unwraps DuckDuckGo's internal
    /// "/l/?uddg=..." redirect links back to the real destination.
    /// </summary>
    private static string CleanRedirect(string url)
    {
        var value = url;
        if (value.StartsWith("//", StringComparison.Ordinal)) value = "https:" + value;

        var index = value.IndexOf("uddg=", StringComparison.OrdinalIgnoreCase);
        if (index < 0) return value;

        var start = index + 5;
        var end = value.IndexOf('&', start);
        var encoded = end < 0 ? value[start..] : value[start..end];

        try
        {
            return Uri.UnescapeDataString(encoded);
        }
        catch
        {
            return value;
        }
    }

    private static string? GetString(JsonElement element, params string[] names)
    {
        foreach (var name in names)
        {
            if (element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String)
            {
                return value.GetString();
            }
        }
        return null;
    }

    private static int? GetInt(JsonElement element, params string[] names)
    {
        foreach (var name in names)
        {
            if (element.TryGetProperty(name, out var value) &&
                (value.ValueKind == JsonValueKind.Number || value.ValueKind == JsonValueKind.String) &&
                int.TryParse(value.ToString(), out var parsed))
            {
                return parsed;
            }
        }
        return null;
    }
}
