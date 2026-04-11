using System.Threading.Channels;

namespace ConcurrencyLab.Endpoints;

public static class ChannelEndpoints
{
    public static RouteGroupBuilder MapChannelEndpoints(this RouteGroupBuilder group)
    {
        // LESSON: Channel<T> — the modern .NET producer/consumer pattern.
        // Producers write items, consumers read them, fully async.
        group.MapGet("/producer-consumer", async () =>
        {
            var channel = Channel.CreateUnbounded<string>();
            var consumed = new System.Collections.Concurrent.ConcurrentBag<string>();
            var sw = System.Diagnostics.Stopwatch.StartNew();

            // Producer — writes 10 items with small delays
            var producer = Task.Run(async () =>
            {
                for (int i = 1; i <= 10; i++)
                {
                    await channel.Writer.WriteAsync($"Message {i}");
                    await Task.Delay(50);
                }
                channel.Writer.Complete(); // signal no more items
            });

            // Consumer — reads until the channel is complete
            var consumer = Task.Run(async () =>
            {
                await foreach (var item in channel.Reader.ReadAllAsync())
                {
                    consumed.Add($"{item} consumed at {sw.ElapsedMilliseconds}ms");
                }
            });

            await Task.WhenAll(producer, consumer);
            sw.Stop();

            return new
            {
                Lesson = "Channel<T> — async producer/consumer. Writer.Complete() signals end of stream.",
                TotalMs = sw.ElapsedMilliseconds,
                Items = consumed.OrderBy(x => x).ToArray()
            };
        });

        // LESSON: Bounded channel — has a max capacity. When full, the producer
        // waits (backpressure) instead of consuming unlimited memory.
        group.MapGet("/bounded", async () =>
        {
            var channel = Channel.CreateBounded<int>(new BoundedChannelOptions(3)
            {
                FullMode = BoundedChannelFullMode.Wait // producer blocks when full
            });
            var log = new System.Collections.Concurrent.ConcurrentBag<string>();
            var sw = System.Diagnostics.Stopwatch.StartNew();

            // Fast producer — tries to write 10 items quickly
            var producer = Task.Run(async () =>
            {
                for (int i = 1; i <= 10; i++)
                {
                    await channel.Writer.WriteAsync(i);
                    log.Add($"Produced {i} at {sw.ElapsedMilliseconds}ms");
                }
                channel.Writer.Complete();
            });

            // Slow consumer — takes 200ms per item
            var consumer = Task.Run(async () =>
            {
                await foreach (var item in channel.Reader.ReadAllAsync())
                {
                    await Task.Delay(200);
                    log.Add($"Consumed {item} at {sw.ElapsedMilliseconds}ms");
                }
            });

            await Task.WhenAll(producer, consumer);
            sw.Stop();

            return new
            {
                Lesson = "Bounded channel (capacity=3). Producer slows down when buffer is full (backpressure).",
                TotalMs = sw.ElapsedMilliseconds,
                Timeline = log.OrderBy(x => x).ToArray()
            };
        });

        return group;
    }
}
