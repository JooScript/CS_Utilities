namespace Utils.HttpClientProvider;

public class HttpClientOptions
{
    public TimeSpan PooledConnectionLifetime { get; } = TimeSpan.FromMinutes(15);

    public TimeSpan PooledConnectionIdleTimeout { get; } = TimeSpan.FromMinutes(2);

    public int MaxConnectionsPerServer { get; } = 8;

    public TimeSpan Timeout { get; } = TimeSpan.FromSeconds(90);

    public bool AllowAutoRedirect { get; } = true;

    public string UserAgent { get; } =
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) " +
        "AppleWebKit/537.36 (KHTML, like Gecko) " +
        "Chrome/126.0.0.0 Safari/537.36";

    public string AcceptLanguage { get; } = "en-US,en;q=0.9,ar;q=0.8";
}