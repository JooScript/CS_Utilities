using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;
using Utils.General;
using Utils.SearchProviders.Contracts;
using Utils.SearchProviders.Models;

namespace Utils.SearchProviders.Services;

/// <summary>
/// Wikimedia Commons API (https://commons.wikimedia.org/wiki/Commons:API).
/// A free, key-less JSON API over the Commons media repository. Images are
/// mostly product/still-life photography with permissive licenses, so it is a
/// useful last-resort provider. Sometimes rate-limited (HTTP 429); the retry
/// policy and the composite fallback chain absorb that.
/// </summary>
public sealed class WikimediaSearchService(HttpClient http, int maxRetries) : IImageSearchService
{
    public string Name => "Wikimedia Commons API";

    public async Task<IReadOnlyList<ImageCandidate>> SearchImageAsync(string query, CancellationToken cancellationToken)
    {
        var url =
            "https://commons.wikimedia.org/w/api.php?action=query" +
            "&generator=search&gsrsearch=" + Uri.EscapeDataString(query) +
            "&gsrnamespace=6&gsrlimit=10" +
            "&prop=imageinfo&iiprop=url%7Cextmetadata&iiurlwidth=1200" +
            "&format=json";

        var json = await RetryPolicy.ExecuteAsync(maxRetries, TimeSpan.FromSeconds(2), async () =>
        {
            using var response = await http.GetAsync(url, cancellationToken).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException(
                    $"Wikimedia Commons returned {(int)response.StatusCode} for \"{query}\"");
            }
            return await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        }, cancellationToken);

        return ParseResults(json, query);
    }

    private static IReadOnlyList<ImageCandidate> ParseResults(string json, string query)
    {
        using var document = JsonDocument.Parse(json);
        if (!document.RootElement.TryGetProperty("query", out var queryElement) ||
            !queryElement.TryGetProperty("pages", out var pages) ||
            pages.ValueKind != JsonValueKind.Object)
        {
            return Array.Empty<ImageCandidate>();
        }

        var candidates = new List<ImageCandidate>();
        foreach (var pageProperty in pages.EnumerateObject())
        {
            var page = pageProperty.Value;
            if (page.ValueKind != JsonValueKind.Object) continue;

            if (!page.TryGetProperty("imageinfo", out var imageInfo) ||
                imageInfo.ValueKind != JsonValueKind.Array ||
                imageInfo.GetArrayLength() == 0)
            {
                continue;
            }

            var info = imageInfo[0];
            var imageUrl = info.TryGetProperty("thumburl", out var thumb) && thumb.ValueKind == JsonValueKind.String
                ? thumb.GetString()
                : GetString(info, "url");
            if (string.IsNullOrWhiteSpace(imageUrl)) continue;

            var title = GetString(page, "title")?.TrimStart().Replace("File:", "", StringComparison.OrdinalIgnoreCase) ?? "";

            var description = "";
            if (info.TryGetProperty("extmetadata", out var meta) &&
                meta.TryGetProperty("ImageDescription", out var desc) &&
                desc.TryGetProperty("value", out var descValue) &&
                descValue.ValueKind == JsonValueKind.String)
            {
                // extmetadata descriptions are HTML (may contain <p>, <a>, ...).
                description = StripHtml(descValue.GetString() ?? "");
            }

            candidates.Add(new ImageCandidate
            {
                Title = string.IsNullOrWhiteSpace(description) ? title : description,
                ImageUrl = imageUrl,
                SourceUrl = GetString(info, "descriptionurl") ?? "",
                Query = query,
                Width = GetInt(info, "width"),
                Height = GetInt(info, "height"),
                Provider = "Wikimedia"
            });
        }

        return candidates;
    }

    private static readonly Regex HtmlTagRegex = new(@"</?[^>]+>", RegexOptions.Compiled);

    private static string StripHtml(string html)
    {
        var text = HtmlTagRegex.Replace(html, " ");
        text = WebUtility.HtmlDecode(text);
        text = Regex.Replace(text, @"\s{2,}", " ");
        return text.Trim();
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
