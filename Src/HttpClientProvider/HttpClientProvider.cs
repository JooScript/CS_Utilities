namespace Utils.HttpClientProvider;

public static class HttpClientProvider
{
    public static HttpClient Create(HttpClientOptions? options = null)
    {
        options ??= new HttpClientOptions();

        var handler = new SocketsHttpHandler
        {
            PooledConnectionLifetime = options.PooledConnectionLifetime,
            PooledConnectionIdleTimeout = options.PooledConnectionIdleTimeout,
            MaxConnectionsPerServer = options.MaxConnectionsPerServer,
            AutomaticDecompression = options.AutomaticDecompression,
            AllowAutoRedirect = options.AllowAutoRedirect
        };

        var client = new HttpClient(handler)
        {
            Timeout = options.Timeout
        };

        client.DefaultRequestHeaders.UserAgent.ParseAdd(options.UserAgent);

        client.DefaultRequestHeaders.AcceptLanguage.ParseAdd(options.AcceptLanguage);

        return client;
    }
}