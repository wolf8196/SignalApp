using System;

namespace SignalApp.Protocol
{
    // simple flat hardcoded strcuture
    public record ProtocolRecord
    {
        public ushort TotalLength { get; init; } // length from header (header + data)

        public byte Type { get; init; } // 3bit type; unknown; irrelevant

        // data
        public DateTime Timestamp { get; init; }

        public ulong Frequency { get; init; }

        public uint Bandwidth { get; init; }

        public double SignalNoiseRatio { get; init; }
    }
}