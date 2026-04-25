using ConcurrencyLab.Services;

namespace ConcurrencyLab.Endpoints;

public static class SharedStateEndpoints
{
    public static RouteGroupBuilder MapSharedStateEndpoints(this RouteGroupBuilder group)
    {
        // LESSON: Race condition — multiple tasks increment a shared counter
        // WITHOUT synchronization. The final count will often be WRONG.
        group.MapGet("/unsafe", async () =>
        {
            int counter = 0;
            const int iterations = 100_000;

            var tasks = Enumerable.Range(0, 10).Select(_ => Task.Run(() =>
            {
                for (int i = 0; i < iterations; i++)
                    counter++; // NOT thread-safe! Read-modify-write is not atomic.
            }));

            await Task.WhenAll(tasks);

            return new
            {
                Lesson = "Race condition! 10 tasks each incrementing 100,000 times.",
                Expected = 10 * iterations,
                Actual = counter,
                Lost = 10 * iterations - counter,
                Bug = "counter++ is read-modify-write (3 steps). Threads overwrite each other."
            };
        });

        // LESSON: Interlocked — atomic operations fix the race condition
        // without the overhead of a full lock.
        group.MapGet("/interlocked", async () =>
        {
            int counter = 0;
            const int iterations = 100_000;

            var tasks = Enumerable.Range(0, 10).Select(_ => Task.Run(() =>
            {
                for (int i = 0; i < iterations; i++)
                    Interlocked.Increment(ref counter); // atomic increment
            }));

            await Task.WhenAll(tasks);

            return new
            {
                Lesson = "Interlocked.Increment — atomic, lock-free, thread-safe.",
                Expected = 10 * iterations,
                Actual = counter,
                Correct = counter == 10 * iterations
            };
        });

        // LESSON: lock — a mutual-exclusion block that only lets one thread
        // execute the critical section at a time.
        group.MapGet("/lock", async () =>
        {
            int counter = 0;
            var lockObj = new object();
            const int iterations = 100_000;

            var tasks = Enumerable.Range(0, 10).Select(_ => Task.Run(() =>
            {
                for (int i = 0; i < iterations; i++)
                {
                    lock (lockObj)
                    {
                        counter++; // safe inside the lock
                    }
                }
            }));

            await Task.WhenAll(tasks);

            return new
            {
                Lesson = "lock(obj) — mutual exclusion. Only one thread enters at a time.",
                Expected = 10 * iterations,
                Actual = counter,
                Correct = counter == 10 * iterations,
                Tradeoff = "Safer than Interlocked for multi-step operations, but slower due to contention."
            };
        });

        // LESSON: lock on a real stateful service — BankAccountService guards
        // its balance with a private lock. Concurrent Transfer calls remain
        // consistent because each read-modify-write happens inside the lock.
        group.MapGet("/account", async (BankAccountService account) =>
        {
            const int transfersPerSide = 1_000;
            var startingBalance = account.Balance;

            var deposits = Enumerable.Range(0, transfersPerSide)
                .Select(_ => Task.Run(() => account.Transfer(1m)));
            var withdrawals = Enumerable.Range(0, transfersPerSide)
                .Select(_ => Task.Run(() => account.Transfer(-1m)));

            await Task.WhenAll(deposits.Concat(withdrawals));

            var endingBalance = account.Balance;

            return new
            {
                Lesson = "lock on an injected service — Transfer is critical-section safe.",
                StartingBalance = startingBalance,
                Deposits = transfersPerSide,
                Withdrawals = transfersPerSide,
                EndingBalance = endingBalance,
                Consistent = endingBalance == startingBalance,
                Tradeoff = "Encapsulating the lock inside the service keeps callers simple, " +
                           "but every public method serializes on the same lock."
            };
        });

        return group;
    }
}
