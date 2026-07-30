using System;

namespace SignalApp
{
    public class MonitorRecordModel
    {
        public required Guid Id { get; init; }

        public required DateTime Timestamp { get; init; }

        public required double FrequencyMegaHz { get; init; }

        public required double BandwidthKiloHz { get; init; }

        public required double SignalNoiseRatio { get; init; }

        public bool IsLive { get; init; }

        public int Count { get; init; }
    }
}