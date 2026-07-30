using System;
using System.Buffers;
using System.Buffers.Binary;

namespace SignalApp.Protocol
{
    internal class ProtocolEncoder : IProtocolEncoder
    {
        private const int HardcodedLength = 30;

        public ProtocolRecord DecodeHeader(ReadOnlySequence<byte> buffer)
        {
            var header = BinaryPrimitives.ReadUInt16LittleEndian(buffer.Slice(0, 2).FirstSpan);
            var type = (byte)(header >> 13); // shift last 3 bits to right
            var totalLength = (ushort)(header & 0b_0001_1111_1111_1111); // reset type portion

            return new ProtocolRecord
            {
                Type = type,
                TotalLength = totalLength
            };
        }

        public ProtocolRecord DecodeData(ReadOnlySequence<byte> buffer)
        {
            if (buffer.Length < 28)
            {
                throw new ArgumentException("Not enough bytes for hardcoded message.");
            }

            // hardcoded structure
            var timestampSlice = buffer.Slice(0, 8);
            var frequencySlice = buffer.Slice(8, 8);
            var bandwidthSlice = buffer.Slice(16, 4);
            var signalToNoiseRatioSlice = buffer.Slice(20, 8);

            var timestamp = DateTimeOffset.FromUnixTimeSeconds(
                Convert.ToInt64(
                    BinaryPrimitives.ReadUInt64LittleEndian(timestampSlice.FirstSpan)));
            var frequency = BinaryPrimitives.ReadUInt64LittleEndian(frequencySlice.FirstSpan);
            var bandwidth = BinaryPrimitives.ReadUInt32LittleEndian(bandwidthSlice.FirstSpan);
            var signalToNoiseRatio = BinaryPrimitives.ReadDoubleLittleEndian(signalToNoiseRatioSlice.FirstSpan);

            return new ProtocolRecord
            {
                Timestamp = timestamp.UtcDateTime,
                Frequency = frequency,
                Bandwidth = bandwidth,
                SignalNoiseRatio = signalToNoiseRatio,
            };
        }

        public byte[] Encode(ProtocolRecord record)
        {
            var buffer = new byte[HardcodedLength];

            ushort header = (ushort)((record.Type << 13) | HardcodedLength);
            BinaryPrimitives.WriteUInt16LittleEndian(buffer.AsSpan(0, 2), header);

            if (record.Timestamp.Kind != DateTimeKind.Utc)
            {
                throw new NotSupportedException("Only UTC timestamps are supported.");
            }

            var unixSeconds = (ulong)new DateTimeOffset(record.Timestamp).ToUnixTimeSeconds();
            BinaryPrimitives.WriteUInt64LittleEndian(buffer.AsSpan(2, 8), unixSeconds);
            BinaryPrimitives.WriteUInt64LittleEndian(buffer.AsSpan(10, 8), record.Frequency);
            BinaryPrimitives.WriteUInt32LittleEndian(buffer.AsSpan(18, 4), record.Bandwidth);
            BinaryPrimitives.WriteDoubleLittleEndian(buffer.AsSpan(22, 8), record.SignalNoiseRatio);

            return buffer;
        }
    }
}