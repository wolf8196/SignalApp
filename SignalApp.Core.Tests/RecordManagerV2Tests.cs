using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Xunit;

namespace SignalApp.Core.Tests
{
    public class RecordManagerV2Tests
    {
        private readonly RecordManagerV2 target;

        public RecordManagerV2Tests()
        {
            target = new RecordManagerV2();
        }

        [Theory]
        [MemberData(nameof(TestData))]
        public void FiresEventsCorrectly(List<MonitorRecord> recordSequence, List<MonitorRecord> expectedAdds, List<MonitorRecord> expectedUpdates)
        {
            var actualAdds = new List<MonitorRecord>();
            var actualUpdates = new List<MonitorRecord>();
            target.RecordAdded += actualAdds.Add;
            target.RecordUpdated += actualUpdates.Add;

            foreach (var record in recordSequence)
            {
                target.PostRecord(record);
            }

            actualAdds.Should().BeEquivalentTo(expectedAdds);
            actualUpdates.Should().BeEquivalentTo(expectedUpdates);
        }

        public static TheoryData<List<MonitorRecord>, List<MonitorRecord>, List<MonitorRecord>> TestData()
        {
            var pinnedRecord1 = new MonitorRecord
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                Timestamp = new DateTime(2026, 5, 23, 12, 1, 1),
                Frequency = 103_599_400,
                Bandwidth = 12_500,
                SignalNoiseRatio = 15.24,
            };
            var medianRecord1 = new MonitorRecord
            {
                Id = Guid.NewGuid(),
                Timestamp = new DateTime(2026, 5, 23, 12, 1, 4),
                Frequency = 103_600_000,
                Bandwidth = 12_500,
                SignalNoiseRatio = 14.12
            };
            var pinnedRecord2 = new MonitorRecord
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000002"),
                Timestamp = new DateTime(2026, 5, 23, 12, 1, 6),
                Frequency = 104_200_000,
                Bandwidth = 108_230,
                SignalNoiseRatio = 14.80,
            };
            var medianRecord2 = new MonitorRecord
            {
                Id = Guid.NewGuid(),
                Timestamp = new DateTime(2026, 5, 23, 12, 1, 7),
                Frequency = 104_185_000,
                Bandwidth = 108_230,
                SignalNoiseRatio = 13.95
            };
            var pinnedRecord3 = new MonitorRecord
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000003"),
                Timestamp = new DateTime(2026, 5, 23, 12, 1, 9),
                Frequency = 99_800_000,
                Bandwidth = 25_000,
                SignalNoiseRatio = 15.45,
            };
            var pinnedRecord4 = new MonitorRecord
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000004"),
                Timestamp = new DateTime(2026, 5, 23, 12, 1, 12),
                Frequency = 105_500_000,
                Bandwidth = 12_500,
                SignalNoiseRatio = 12.01
            };
            var medianRecord4 = new MonitorRecord
            {
                Id = Guid.NewGuid(),
                Timestamp = new DateTime(2026, 5, 23, 12, 1, 13),
                Frequency = 105_497_800,
                Bandwidth = 12_500,
                SignalNoiseRatio = 11.68
            };
            var pinnedRecord5 = new MonitorRecord
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000005"),
                Timestamp = new DateTime(2026, 5, 23, 12, 1, 15),
                Frequency = 103_605_000,
                Bandwidth = 12_500,
                SignalNoiseRatio = 14.35
            };
            var pinnedRecord6 = new MonitorRecord
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000006"),
                Timestamp = new DateTime(2026, 5, 23, 12, 1, 16),
                Frequency = 103_597_500,
                Bandwidth = 12_500,
                SignalNoiseRatio = 15.10
            };

            return new TheoryData<List<MonitorRecord>, List<MonitorRecord>, List<MonitorRecord>>
            {
                // sequence, expected adds, expected updates
                {
                    [
                        pinnedRecord1,
                        new MonitorRecord
                        {
                            Id = Guid.NewGuid(),
                            Timestamp = new DateTime(2026, 5, 23, 12, 1, 3),
                            Frequency = 103_601_800,
                            Bandwidth = 12_500,
                            SignalNoiseRatio = 15.61
                        },
                        medianRecord1,
                        new MonitorRecord
                        {
                            Id = Guid.NewGuid(),
                            Timestamp = new DateTime(2026, 5, 23, 12, 1, 5),
                            Frequency = 103_602_100,
                            Bandwidth = 12_500,
                            SignalNoiseRatio = 15.93
                        },

                        pinnedRecord2,
                        medianRecord2,
                        new MonitorRecord
                        {
                            Id = Guid.NewGuid(),
                            Timestamp = new DateTime(2026, 5, 23, 12, 1, 8),
                            Frequency = 104_215_500,
                            Bandwidth = 108_230,
                            SignalNoiseRatio = 15.22
                        },
                        pinnedRecord3,
                        new MonitorRecord
                        {
                            Id = Guid.NewGuid(),
                            Timestamp = new DateTime(2026, 5, 23, 12, 1, 10),
                            Frequency = 99_812_000,
                            Bandwidth = 25_000,
                            SignalNoiseRatio = 14.70
                        },
                        new MonitorRecord
                        {
                            Id = Guid.NewGuid(),
                            Timestamp = new DateTime(2026, 5, 23, 12, 1, 11),
                            Frequency = 99_791_500,
                            Bandwidth = 25_000,
                            SignalNoiseRatio = 16.08
                        },
                        pinnedRecord4,
                        new MonitorRecord
                        {
                            Id = Guid.NewGuid(),
                            Timestamp = new DateTime(2026, 5, 23, 12, 1, 13),
                            Frequency = 105_497_800,
                            Bandwidth = 12_500,
                            SignalNoiseRatio = 11.68
                        },
                        new MonitorRecord
                        {
                            Id = Guid.NewGuid(),
                            Timestamp = new DateTime(2026, 5, 23, 12, 1, 14),
                            Frequency = 105_503_200,
                            Bandwidth = 12_500,
                            SignalNoiseRatio = 12.44
                        },
                        pinnedRecord5,
                        pinnedRecord6
                    ],
                    [
                       pinnedRecord1 with { Count = 1, IsLive = true },
                       pinnedRecord2 with { Count = 1, IsLive = true },
                       pinnedRecord3 with { Count = 1, IsLive = true },
                       pinnedRecord4 with { Count = 1, IsLive = true },
                       pinnedRecord5 with { Count = 1, IsLive = true },
                       pinnedRecord6 with { Count = 1, IsLive = true }
                    ],
                    [
                        pinnedRecord1 with { Count = 2, IsLive = true },
                        medianRecord1 with { Id = pinnedRecord1.Id, Count = 3, IsLive = true },
                        medianRecord1 with { Id = pinnedRecord1.Id, Count = 4, IsLive = true },
                        medianRecord1 with { Id = pinnedRecord1.Id, Count = 4, IsLive = false },
                        medianRecord2 with { Id = pinnedRecord2.Id, Count = 2, IsLive = true },
                        pinnedRecord2 with { Count = 3, IsLive = true },
                        pinnedRecord2 with { Count = 3, IsLive = false },
                        pinnedRecord3 with { Count = 2, IsLive = true },
                        pinnedRecord3 with { Count = 3, IsLive = true },
                        pinnedRecord3 with { Count = 3, IsLive = false },
                        medianRecord4 with { Id = pinnedRecord4.Id,Count = 2, IsLive = true },
                        pinnedRecord4 with { Count = 3, IsLive = true },
                        pinnedRecord4 with { Count = 3, IsLive = false },
                        pinnedRecord5 with { Count = 1, IsLive = false },
                    ]
                }
            };
        }
    }
}