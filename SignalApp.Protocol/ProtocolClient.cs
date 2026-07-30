using System;
using System.Buffers;
using System.IO;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;

namespace SignalApp.Protocol
{
    internal class ProtocolClient : IProtocolClient
    {
        private const int HeaderSize = 2;

        private readonly IProtocolEncoder encoder;
        private readonly Stream stream;
        private readonly PipeReader reader;

        public ProtocolClient(IProtocolEncoder encoder, Stream stream)
        {
            this.encoder = encoder;
            this.stream = stream;
            reader = PipeReader.Create(stream);
        }

        public async Task<ProtocolRecord> ReadNextAsync(CancellationToken token)
        {
            var buffer = await ReadBytesAsync(HeaderSize, token);

            var header = encoder.DecodeHeader(buffer);

            if (buffer.Length < header.TotalLength)
            {
                MarkExamined(buffer);
                buffer = await ReadBytesAsync(header.TotalLength, token);
            }

            var processedSequence = buffer.Slice(0, header.TotalLength);
            var data = encoder.DecodeData(processedSequence.Slice(HeaderSize));

            MarkConsumed(processedSequence);

            return new ProtocolRecord
            {
                Type = header.Type,
                TotalLength = header.TotalLength,
                Timestamp = data.Timestamp,
                Bandwidth = data.Bandwidth,
                Frequency = data.Frequency,
                SignalNoiseRatio = data.SignalNoiseRatio,
            };
        }

        private async Task<ReadOnlySequence<byte>> ReadBytesAsync(int count, CancellationToken token)
        {
            while (true) // read while we don't have the required amount available
            {
                token.ThrowIfCancellationRequested();

                var readResult = await reader.ReadAsync(token);

                if (readResult.Buffer.Length < count)
                {
                    if (readResult.IsCanceled)
                    {
                        throw new TimeoutException("Failed to read from the transport connection in specified period of time.");
                    }

                    if (readResult.IsCompleted)
                    {
                        throw new InvalidOperationException("Pipe reader was completed.");
                    }

                    MarkExamined(readResult.Buffer);
                    continue;
                }

                return readResult.Buffer;
            }
        }

        private void MarkConsumed(ReadOnlySequence<byte> buffer)
        {
            reader.AdvanceTo(buffer.End, buffer.End);
        }

        private void MarkExamined(ReadOnlySequence<byte> buffer)
        {
            reader.AdvanceTo(buffer.Start, buffer.End);
        }

        public void Dispose()
        {
            stream.Dispose();
            reader.Complete(new Exception());
        }
    }
}