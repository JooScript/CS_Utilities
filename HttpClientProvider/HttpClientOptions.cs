using System.Net;

namespace Utils.HttpClientProvider;

public class HttpClientOptions
{
    public TimeSpan PooledConnectionLifetime { get; init; } =
        TimeSpan.FromMinutes(15);

    public TimeSpan PooledConnectionIdleTimeout { get; init; } =
        TimeSpan.FromMinutes(2);

    public int MaxConnectionsPerServer { get; init; } = 8;

    public TimeSpan Timeout { get; init; } =
        TimeSpan.FromSeconds(90);

    public bool AllowAutoRedirect { get; init; } = true;

    public DecompressionMethods AutomaticDecompression { get; init; } =
        DecompressionMethods.All;

    public string UserAgent { get; init; } =
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) " +
        "AppleWebKit/537.36 (KHTML, like Gecko) " +
        "Chrome/126.0.0.0 Safari/537.36";

    public string AcceptLanguage { get; init; } =
        "en-US,en;q=0.9,ar;q=0.8";
}