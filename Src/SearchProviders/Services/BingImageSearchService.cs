using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;
using Utils.General;
using Utils.SearchProviders.Contracts;
using Utils.SearchProviders.Models;

namespace Utils.SearchProviders.Services;

public sealed class BingImageSearchService : IImageSearchService
{
    private static readonly Regex IuscRegex = new(@"\bm=""(?<json>[^""]{20,})""", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private readonly HttpClient _http;
    private readonly int _maxRetries;

    public string Name => "Bing";

    public BingImageSearchService(HttpClient http, int maxRetries)
    {
        _http = http;
        _maxRetries = maxRetries;
    }

    public async Task<IReadOnlyList<ImageCandidate>> SearchImageAsync(string query, CancellationToken cancellationToken)
    {
        var url = "https://www.bing.com/images/search?q=" + Uri.EscapeDataString(query) + "&qft=+filterui:imagesize-large";

        var html = await RetryPolicy.ExecuteAsync(_maxRetries, TimeSpan.FromSeconds(2), async () =>
        {
            using var response = await _http.GetAsync(url, cancellationToken).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException($"Bing returned {(int)response.StatusCode} for \"{query}\"");

            return await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        }, cancellationToken);

        return ParseResults(html, query);
    }

    private static IReadOnlyList<ImageCandidate> ParseResults(string html, string query)
    {
        var results = new List<ImageCandidate>();

        foreach (Match match in IuscRegex.Matches(html))
        {
            try
            {
                var decoded = WebUtility.HtmlDecode(match.Groups["json"].Value);
                if (!decoded.Contains("\"murl\"", StringComparison.Ordinal)) continue;

                using var document = JsonDocument.Parse(decoded);
                var root = document.RootElement;

                var imageUrl = GetString(root, "murl") ?? GetString(root, "mu");
                var title = GetString(root, "t");
                var sourcePage = GetString(root, "purl");

                if (string.IsNullOrWhiteSpace(imageUrl)) continue;

                results.Add(new ImageCandidate
                {
                    Title = title ?? "",
                    ImageUrl = imageUrl,
                    SourceUrl = sourcePage ?? "",
                    Query = query,
                    Width = GetInt(root, "w"),
                    Height = GetInt(root, "h"),
                    Provider = "Bing"
                });
            }
            catch (JsonException)
            {
                throw;
            }
        }

        return results;
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
