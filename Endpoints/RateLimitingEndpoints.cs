using ConcurrencyLab.Services;

namespace ConcurrencyLab.Endpoints;

public static class RateLimitingEndpoints
{
    public static RouteGroupBuilder MapRateLimitingEndpoints(this RouteGroupBuilder group)
    {
        // LESSON: Token-bucket rate limiter using SemaphoreSlim.
        // Tokens replenish over time; requests must acquire a token to proceed.
        group.MapGet("/demo", async (RateLimiterService limiter) =>
        {
            var results = new List<object>();
            var sw = System.Diagnostics.Stopwatch.StartNew();

            // Fire 8 requests rapidly against a limiter that allows 3/second
            var tasks = Enumerable.Range(1, 8).Select(async i =>
            {
                var allowed = await limiter.TryAcquireAsync(TimeSpan.FromMilliseconds(2000));
                results.Add(new
                {
                    Request = i,
                    Allowed = allowed,
                    AtMs = sw.ElapsedMilliseconds
                });
            });

            await Task.WhenAll(tasks);
            sw.Stop();

            return new
            {
                Lesson = "Token-bucket rate limiter (3 tokens, refill 1/sec). Excess requests wait or get rejected.",
                Results = results.OrderBy(r => ((dynamic)r).AtMs)
            };
        });

        return group;
    }
}
