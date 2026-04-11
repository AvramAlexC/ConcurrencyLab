namespace ConcurrencyLab.Services;

/// <summary>
/// A simple token-bucket rate limiter built on SemaphoreSlim.
/// Tokens are replenished by a background timer.
/// </summary>
public class RateLimiterService : IDisposable
{
    private readonly SemaphoreSlim _semaphore;
    private readonly Timer _refillTimer;

    public RateLimiterService()
    {
        // Start with 3 tokens, replenish 1 every second
        _semaphore = new SemaphoreSlim(3, 3);
        _refillTimer = new Timer(_ =>
        {
            if (_semaphore.CurrentCount < 3)
                _semaphore.Release();
        }, null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
    }

    public async Task<bool> TryAcquireAsync(TimeSpan timeout)
    {
        return await _semaphore.WaitAsync(timeout);
    }

    public void Dispose()
    {
        _refillTimer.Dispose();
        _semaphore.Dispose();
    }
}
