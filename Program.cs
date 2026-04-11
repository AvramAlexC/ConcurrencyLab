using ConcurrencyLab.Endpoints;
using ConcurrencyLab.Services;

var builder = WebApplication.CreateBuilder(args);

// Register singleton services used by the concurrency demos
builder.Services.AddSingleton<BankAccountService>();
builder.Services.AddSingleton<RateLimiterService>();
builder.Services.AddSingleton<ProducerConsumerService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Concurrency Lab", Description = ".NET Concurrency Learning Project" });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

// Each group of endpoints teaches a different concurrency concept
app.MapGroup("/1-async-basics").MapAsyncBasicsEndpoints();
app.MapGroup("/2-parallel").MapParallelEndpoints();
app.MapGroup("/3-shared-state").MapSharedStateEndpoints();
app.MapGroup("/4-synchronization").MapSynchronizationEndpoints();
app.MapGroup("/5-channels").MapChannelEndpoints();
app.MapGroup("/6-rate-limiting").MapRateLimitingEndpoints();
app.MapGroup("/7-cancellation").MapCancellationEndpoints();
app.MapGroup("/8-async-pitfalls").MapPitfallEndpoints();

// Landing page listing all the lessons
app.MapGet("/", () => Results.Text("""
    === .NET Concurrency Lab ===

    Lessons (hit each endpoint to see it in action):

    1. /1-async-basics/sequential       — Sequential awaits
       /1-async-basics/concurrent        — Concurrent awaits with Task.WhenAll
       /1-async-basics/first-wins        — Task.WhenAny (first result wins)

    2. /2-parallel/for                   — Parallel.ForEachAsync
       /2-parallel/plinq                 — Parallel LINQ

    3. /3-shared-state/unsafe            — Race condition (broken counter)
       /3-shared-state/interlocked       — Fix with Interlocked
       /3-shared-state/lock              — Fix with lock

    4. /4-synchronization/semaphore      — SemaphoreSlim (bounded concurrency)
       /4-synchronization/reader-writer  — ReaderWriterLockSlim
       /4-synchronization/barrier        — Barrier (phased work)

    5. /5-channels/producer-consumer     — Channel<T> producer/consumer
       /5-channels/bounded               — Bounded channel with backpressure

    6. /6-rate-limiting/demo             — Token-bucket rate limiter

    7. /7-cancellation/timeout           — CancellationToken with timeout
       /7-cancellation/linked            — Linked cancellation tokens

    8. /8-async-pitfalls/async-void      — Why async void is dangerous
       /8-async-pitfalls/sync-over-async — Why .Result / .Wait() deadlocks
    """, "text/plain"));

app.Run();
