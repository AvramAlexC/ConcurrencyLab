namespace ConcurrencyLab.Endpoints;

public static class SynchronizationEndpoints
{
    public static RouteGroupBuilder MapSynchronizationEndpoints(this RouteGroupBuilder group)
    {
        // LESSON: SemaphoreSlim — limits how many tasks can enter a section concurrently.
        // Unlike lock (1 at a time), a semaphore allows N at a time.
        group.MapGet("/semaphore", async () =>
        {
            var semaphore = new SemaphoreSlim(3); // max 3 concurrent
            var log = new System.Collections.Concurrent.ConcurrentBag<string>();
            var sw = System.Diagnostics.Stopwatch.StartNew();

            var tasks = Enumerable.Range(1, 10).Select(async i =>
            {
                await semaphore.WaitAsync();
                try
                {
                    log.Add($"Task {i} entered at {sw.ElapsedMilliseconds}ms (thread {Environment.CurrentManagedThreadId})");
                    await Task.Delay(300); // simulate work
                }
                finally
                {
                    semaphore.Release();
                }
            });

            await Task.WhenAll(tasks);
            sw.Stop();

            return new
            {
                Lesson = "SemaphoreSlim(3) — only 3 tasks run at once. Others queue up.",
                TotalMs = sw.ElapsedMilliseconds,
                ExpectedMs = "~1200ms (10 tasks / 3 slots * 300ms)",
                Timeline = log.OrderBy(x => x).Take(10)
            };
        });

        // LESSON: ReaderWriterLockSlim — allows many concurrent readers
        // but exclusive access for writers.
        group.MapGet("/reader-writer", async () =>
        {
            var rwLock = new ReaderWriterLockSlim();
            var sharedData = "initial";
            var log = new System.Collections.Concurrent.ConcurrentBag<string>();

            // Spawn readers and writers
            var readers = Enumerable.Range(1, 6).Select(i => Task.Run(() =>
            {
                rwLock.EnterReadLock();
                try
                {
                    log.Add($"Reader {i} sees: '{sharedData}' (concurrent reads OK)");
                    Thread.Sleep(100);
                }
                finally
                {
                    rwLock.ExitReadLock();
                }
            }));

            var writers = Enumerable.Range(1, 2).Select(i => Task.Run(() =>
            {
                rwLock.EnterWriteLock();
                try
                {
                    sharedData = $"written by writer {i}";
                    log.Add($"Writer {i} updated data (exclusive access)");
                    Thread.Sleep(150);
                }
                finally
                {
                    rwLock.ExitWriteLock();
                }
            }));

            await Task.WhenAll(readers.Concat(writers));
            rwLock.Dispose();

            return new
            {
                Lesson = "ReaderWriterLockSlim — many readers OR one writer at a time.",
                FinalValue = sharedData,
                Events = log.ToArray()
            };
        });

        // LESSON: Barrier — synchronizes multiple participants in phases.
        // All must reach the barrier before any can proceed to the next phase.
        group.MapGet("/barrier", async () =>
        {
            var phaseLog = new System.Collections.Concurrent.ConcurrentBag<string>();
            var barrier = new Barrier(3, b =>
            {
                phaseLog.Add($"--- All participants completed phase {b.CurrentPhaseNumber} ---");
            });

            var tasks = Enumerable.Range(1, 3).Select(worker => Task.Run(() =>
            {
                for (int phase = 0; phase < 3; phase++)
                {
                    Thread.Sleep(Random.Shared.Next(50, 200)); // variable work
                    phaseLog.Add($"Worker {worker} finished phase {phase}");
                    barrier.SignalAndWait(); // wait for others
                }
            }));

            await Task.WhenAll(tasks);
            barrier.Dispose();

            return new
            {
                Lesson = "Barrier(3) — 3 workers must finish each phase before any moves on.",
                Phases = phaseLog.ToArray()
            };
        });

        return group;
    }
}
