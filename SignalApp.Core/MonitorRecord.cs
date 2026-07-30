using System;

namespace SignalApp.Core
{
    public record MonitorRecord
    {
        public required Guid Id { get; init; }

        public required DateTime Timestamp { get; init; }

        public required ulong Frequency { get; init; }

        public required uint Bandwidth { get; init; }

        public required double SignalNoiseRatio { get; init; }

        public bool IsLive { get; init; }

        public int Count { get; init; }
    }
}