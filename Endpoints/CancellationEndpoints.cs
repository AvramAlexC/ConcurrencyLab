namespace ConcurrencyLab.Endpoints;

public static class CancellationEndpoints
{
    public static RouteGroupBuilder MapCancellationEndpoints(this RouteGroupBuilder group)
    {
        // LESSON: CancellationToken with a timeout.
        // The work is cancelled if it exceeds the deadline.
        group.MapGet("/timeout", async () =>
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(500));
            var log = new List<string>();
            var sw = System.Diagnostics.Stopwatch.StartNew();

            try
            {
                for (int i = 1; i <= 20; i++)
                {
                    cts.Token.ThrowIfCancellationRequested();
                    await Task.Delay(100, cts.Token);
                    log.Add($"Step {i} at {sw.ElapsedMilliseconds}ms");
                }
            }
            catch (OperationCanceledException)
            {
                log.Add($"CANCELLED at {sw.ElapsedMilliseconds}ms");
            }

            return new
            {
                Lesson = "CancellationTokenSource with 500ms timeout. Work stops after the deadline.",
                Steps = log
            };
        });

        // LESSON: Linked cancellation tokens — combine multiple cancellation sources.
        // Either source can trigger cancellation.
        group.MapGet("/linked", async () =>
        {
            using var userCts = new CancellationTokenSource();
            using var timeoutCts = new CancellationTokenSource(TimeSpan.FromMilliseconds(800));
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
                userCts.Token, timeoutCts.Token);

            var log = new List<string>();
            var sw = System.Diagnostics.Stopwatch.StartNew();

            // Simulate user cancellation at 400ms
            _ = Task.Delay(400).ContinueWith(_ =>
            {
                userCts.Cancel();
                log.Add($"User requested cancellation at {sw.ElapsedMilliseconds}ms");
            });

            try
            {
                for (int i = 1; i <= 20; i++)
                {
                    linkedCts.Token.ThrowIfCancellationRequested();
                    await Task.Delay(100, linkedCts.Token);
                    log.Add($"Step {i} at {sw.ElapsedMilliseconds}ms");
                }
            }
            catch (OperationCanceledException)
            {
                log.Add($"CANCELLED at {sw.ElapsedMilliseconds}ms — " +
                    $"by user: {userCts.IsCancellationRequested}, " +
                    $"by timeout: {timeoutCts.IsCancellationRequested}");
            }

            return new
            {
                Lesson = "Linked tokens — combine user cancellation (400ms) and timeout (800ms). First one wins.",
                Steps = log
            };
        });

        return group;
    }
}
