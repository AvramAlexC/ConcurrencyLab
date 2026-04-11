namespace ConcurrencyLab.Endpoints;

public static class AsyncBasicsEndpoints
{
    public static RouteGroupBuilder MapAsyncBasicsEndpoints(this RouteGroupBuilder group)
    {
        // LESSON: Sequential awaits — each task waits for the previous one to finish.
        // Total time = sum of all individual delays.
        group.MapGet("/sequential", async () =>
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();

            var a = await SimulateWorkAsync("Service A", 500);
            var b = await SimulateWorkAsync("Service B", 400);
            var c = await SimulateWorkAsync("Service C", 300);

            sw.Stop();
            return new
            {
                Lesson = "Sequential awaits — each call waits for the previous one.",
                TotalMs = sw.ElapsedMilliseconds,
                ExpectedMs = "~1200ms (500+400+300)",
                Results = new[] { a, b, c }
            };
        });

        // LESSON: Concurrent awaits — all tasks run at the same time via Task.WhenAll.
        // Total time = max of all individual delays.
        group.MapGet("/concurrent", async () =>
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();

            var taskA = SimulateWorkAsync("Service A", 500);
            var taskB = SimulateWorkAsync("Service B", 400);
            var taskC = SimulateWorkAsync("Service C", 300);

            var results = await Task.WhenAll(taskA, taskB, taskC);

            sw.Stop();
            return new
            {
                Lesson = "Concurrent awaits — all tasks started at once, awaited together.",
                TotalMs = sw.ElapsedMilliseconds,
                ExpectedMs = "~500ms (max of 500, 400, 300)",
                Results = results
            };
        });

        // LESSON: Task.WhenAny — returns as soon as the first task completes.
        // Useful for fallback/racing patterns.
        group.MapGet("/first-wins", async () =>
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();

            var fast = SimulateWorkAsync("Fast CDN", 200);
            var medium = SimulateWorkAsync("Medium CDN", 600);
            var slow = SimulateWorkAsync("Slow CDN", 1500);

            var winner = await await Task.WhenAny(fast, medium, slow);

            sw.Stop();
            return new
            {
                Lesson = "Task.WhenAny — first completed task wins (race pattern).",
                TotalMs = sw.ElapsedMilliseconds,
                Winner = winner
            };
        });

        return group;
    }

    private static async Task<string> SimulateWorkAsync(string name, int delayMs)
    {
        await Task.Delay(delayMs);
        return $"{name} completed in ~{delayMs}ms";
    }
}
