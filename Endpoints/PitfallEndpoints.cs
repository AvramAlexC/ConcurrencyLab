namespace ConcurrencyLab.Endpoints;

public static class PitfallEndpoints
{
    public static RouteGroupBuilder MapPitfallEndpoints(this RouteGroupBuilder group)
    {
        // LESSON: async void — exceptions crash the process because they can't
        // be caught by the caller. Always use async Task instead.
        group.MapGet("/async-void", () =>
        {
            return new
            {
                Lesson = "async void is DANGEROUS — exceptions cannot be caught by callers.",
                Bad = "async void DoWork() { throw new Exception(); } // crashes the process!",
                Good = "async Task DoWork() { throw new Exception(); } // exception is captured in the Task",
                Rule = "Only use async void for event handlers (UI frameworks). Everywhere else, use async Task.",
                WhyItMatters = new[]
                {
                    "1. async void exceptions propagate to the SynchronizationContext, not the caller",
                    "2. The caller has no Task to await, so it can't know when work is done",
                    "3. You cannot compose or chain async void methods",
                    "4. Unit testing async void is nearly impossible"
                }
            };
        });

        // LESSON: Sync-over-async — calling .Result or .Wait() on a Task
        // can cause deadlocks in environments with a SynchronizationContext.
        group.MapGet("/sync-over-async", () =>
        {
            return new
            {
                Lesson = "Calling .Result or .Wait() on a Task can DEADLOCK.",
                DeadlockScenario = new[]
                {
                    "1. You call task.Result on the UI/ASP.NET thread",
                    "2. This blocks the thread, waiting for the task to complete",
                    "3. The task's continuation needs that same thread (via SynchronizationContext)",
                    "4. DEADLOCK: the thread waits for the task, the task waits for the thread"
                },
                Bad = new[]
                {
                    "var data = GetDataAsync().Result;     // blocks thread",
                    "GetDataAsync().Wait();                 // blocks thread",
                    "var data = GetDataAsync().GetAwaiter().GetResult(); // still blocks!"
                },
                Good = new[]
                {
                    "var data = await GetDataAsync();       // releases the thread",
                    "// If you MUST call sync, use ConfigureAwait(false) inside the async method",
                    "// Or restructure to be async all the way up"
                },
                AsyncAllTheWay = "The best fix is async all the way from controller to database."
            };
        });

        return group;
    }
}
