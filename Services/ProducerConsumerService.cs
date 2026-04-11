using System.Threading.Channels;

namespace ConcurrencyLab.Services;

/// <summary>
/// A background service demonstrating the producer/consumer pattern
/// using Channel<T>. Registered as a singleton for the channel endpoints.
/// </summary>
public class ProducerConsumerService
{
    private readonly Channel<string> _channel = Channel.CreateBounded<string>(
        new BoundedChannelOptions(10) { FullMode = BoundedChannelFullMode.Wait });

    public ChannelWriter<string> Writer => _channel.Writer;
    public ChannelReader<string> Reader => _channel.Reader;
}
