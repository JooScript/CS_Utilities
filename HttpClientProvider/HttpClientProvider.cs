using System.Net;

namespace Utils.HttpClientProvider;

/// <summary>
/// Creates the single, shared HttpClient used by every service.
/// One instance per process - never created per request.
/// </summary>
public static class HttpClientProvider
{
    public static HttpClient Create()
    {
        var handler = new SocketsHttpHandler
        {
            PooledConnectionLifetime = TimeSpan.FromMinutes(15),
            PooledConnectionIdleTimeout = TimeSpan.FromMinutes(2),
            MaxConnectionsPerServer = 8,
            AutomaticDecompression = DecompressionMethods.All,
            AllowAutoRedirect = true
        };

        var client = new HttpClient(handler)
        {
            Timeout = TimeSpan.FromSeconds(90)
        };

        client.DefaultRequestHeaders.UserAgent.ParseAdd(
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/126.0.0.0 Safari/537.36");
        client.DefaultRequestHeaders.AcceptLanguage.ParseAdd("en-US,en;q=0.9");

        return client;
    }
}
