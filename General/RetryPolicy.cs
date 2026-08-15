namespace Utils.General;

public static class RetryPolicy
{
    public static async Task<T> ExecuteAsync<T>(
        int maxRetries,
        TimeSpan baseDelay,
        Func<Task<T>> action,
        CancellationToken cancellationToken)
    {
        var attempt = 0;

        while (true)
        {
            try
            {
                return await action().ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                attempt++;
                if (attempt > maxRetries)
                {
                    throw new InvalidOperationException(
                        $"Operation failed after {maxRetries} retries: {ex.Message}", ex);
                }

                var delay = baseDelay * Math.Pow(2, attempt - 1);
                delay += TimeSpan.FromMilliseconds(Random.Shared.Next(0, 500));

                try
                {
                    await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
            }
        }
    }

    public static Task<T> ExecuteAsync<T>(int maxRetries, TimeSpan baseDelay, Func<Task<T>> action) =>
        ExecuteAsync(maxRetries, baseDelay, action, CancellationToken.None);
}