using System;
using System.Diagnostics.CodeAnalysis;
using System.IO.Pipelines;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

[assembly: ExcludeFromCodeCoverage]

namespace SignalApp.Protocol.Tests
{
    public sealed class ProtocolClientTest : IDisposable
    {
        private readonly ProtocolClient target;

        private readonly Pipe pipe;
        private readonly PipeWriter writer;
        private readonly PipeReader reader;

        public ProtocolClientTest()
        {
            pipe = new Pipe();
            writer = pipe.Writer;
            reader = pipe.Reader;

            target = new ProtocolClient(new ProtocolEncoder(), reader.AsStream());
        }

        [Theory]
        [InlineData(1, 100)]
        [InlineData(5, 200)]
        [InlineData(6, 1000)]
        public async Task ReadsNextRecordAsync(int chunkSize, int delay)
        {
            byte[] data = [30, 0, 176, 133, 156, 105, 0, 0, 0, 0, 128, 207, 44, 6, 0, 0, 0, 0, 212, 48, 0, 0, 184, 30, 133, 235, 81, 120, 46, 64];

            var expected = new ProtocolRecord
            {
                Type = 0,
                TotalLength = 30,
                Timestamp = new DateTime(2026, 2, 23, 16, 52, 0, DateTimeKind.Utc),
                Frequency = 103_600_000UL,
                Bandwidth = 12_500u,
                SignalNoiseRatio = 15.235
            };

            var actualTask = Task.Run(() => target.ReadNextAsync(default));
            var fillUpTask = Task.Run(async () =>
            {
                foreach (var input in data.Chunk(chunkSize))
                {
                    await writer.WriteAsync(input, TestContext.Current.CancellationToken);
                    await Task.Delay(delay, TestContext.Current.CancellationToken);
                }
            },
            TestContext.Current.CancellationToken);

            await Task.WhenAll(actualTask, fillUpTask);
            var actual = await actualTask;

            actual.Should().BeEquivalentTo(expected);
        }

        public void Dispose()
        {
            writer.Complete(new Exception());
            reader.Complete(new Exception());
        }
    }
}