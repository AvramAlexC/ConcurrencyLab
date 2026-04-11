namespace ConcurrencyLab.Endpoints;

public static class ParallelEndpoints
{
    public static RouteGroupBuilder MapParallelEndpoints(this RouteGroupBuilder group)
    {
        // LESSON: Parallel.ForEachAsync — CPU/IO work across many items
        // with controlled degree of parallelism.
        group.MapGet("/for", async () =>
        {
            var items = Enumerable.Range(1, 20).ToList();
            var results = new System.Collections.Concurrent.ConcurrentBag<string>();
            var sw = System.Diagnostics.Stopwatch.StartNew();

            await Parallel.ForEachAsync(items, new ParallelOptions { MaxDegreeOfParallelism = 4 }, async (item, ct) =>
            {
                await Task.Delay(100, ct); // simulate async IO per item
                results.Add($"Item {item} processed on thread {Environment.CurrentManagedThreadId}");
            });

            sw.Stop();
            return new
            {
                Lesson = "Parallel.ForEachAsync — 20 items, max 4 concurrent. Notice different thread IDs.",
                TotalMs = sw.ElapsedMilliseconds,
                ExpectedMs = "~500ms (20 items / 4 parallel * 100ms each)",
                ItemCount = results.Count,
                SampleResults = results.Take(6)
            };
        });

        // LESSON: PLINQ — parallel LINQ queries for CPU-bound transforms.
        group.MapGet("/plinq", () =>
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();

            var results = Enumerable.Range(1, 100)
                .AsParallel()
                .WithDegreeOfParallelism(4)
                .Select(n =>
                {
                    // Simulate CPU work
                    Thread.SpinWait(500_000);
                    return new { Number = n, ThreadId = Environment.CurrentManagedThreadId };
                })
                .ToList();

            sw.Stop();

            var distinctThreads = results.Select(r => r.ThreadId).Distinct().Count();

            return new
            {
                Lesson = "PLINQ — parallel LINQ for CPU-bound work. Items processed across multiple threads.",
                TotalMs = sw.ElapsedMilliseconds,
                ItemsProcessed = results.Count,
                ThreadsUsed = distinctThreads
            };
        });

        return group;
    }
}
