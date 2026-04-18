# ConcurrencyLab

> Hands-on experiments with .NET concurrency primitives — built to internalize them beyond textbook level.

**Status:** 🚧 Active learning · ASP.NET Web API with Swagger UI for interactive exploration

## Why this exists

Built to internalize .NET concurrency primitives beyond textbook understanding. Each lesson is a small, runnable experiment focused on one concept. Swagger UI makes it easy to invoke endpoints and observe behavior — timings, exceptions, ordering — in real time.

## Experiments

- **1. Async Basics** — What `await` actually does to the calling thread, and how sequential vs concurrent awaits compare.
- **2. Parallel** — CPU-bound work with `Parallel.For` and PLINQ, and when parallelism actually pays off.
- **3. Shared State** — Race conditions on shared counters and balances, made visible without synchronization.
- **4. Synchronization** — Fixing those races with `Interlocked`, `lock`, `SemaphoreSlim`, `ReaderWriterLockSlim`, and `Barrier`.
- **5. Channels** — Producer-consumer patterns with `Channel<T>`, including bounded channels and backpressure.
- **6. Rate Limiting** — Throttling outbound calls with `SemaphoreSlim` as a permit pool.
- **7. Cancellation** — Cooperative cancellation via `CancellationToken` — first-wins, timeouts, and linked sources.
- **8. Async Pitfalls** — `async void`, sync-over-async, and why they deadlock or swallow exceptions.

## Running locally

**Prerequisites:** .NET 8 SDK

```bash
cd ConcurrencyLab
dotnet run
```

Open Swagger UI at `https://localhost:7198/swagger` and try the endpoints. Each lesson is a route group under `/N-<topic>` — read the code first, then invoke and observe.

## About

Built by [Alex Avram](https://github.com/AvramAlexC) — .NET developer in Timișoara.
Companion to [InboxAI](https://github.com/AvramAlexC/inboxai) — my main side project.
