using System;
using System.Collections.Generic;

namespace SignalApp.Core
{
    internal sealed class RecordManagerV1 : IRecordManager
    {
        private readonly Stack<MonitorRecord> records;

        public RecordManagerV1()
        {
            records = new Stack<MonitorRecord>();
        }

        public event Action<MonitorRecord>? RecordAdded;

        public event Action<MonitorRecord>? RecordUpdated;

        public void PostRecord(MonitorRecord record)
        {
            if (records.TryPop(out var pinnedRecord))
            {
                if (ShouldAggregate(pinnedRecord, record))
                {
                    var updatedRecord = pinnedRecord with { Count = pinnedRecord.Count + 1 };
                    records.Push(updatedRecord);
                    RecordUpdated?.Invoke(updatedRecord);
                }
                else
                {
                    var closedRecord = pinnedRecord with { IsLive = false };
                    records.Push(closedRecord);
                    RecordUpdated?.Invoke(closedRecord);

                    var newRecord = record with { Count = 1, IsLive = true };
                    records.Push(newRecord);
                    RecordAdded?.Invoke(newRecord);
                }
            }
            else
            {
                var newRecord = record with { Count = 1, IsLive = true };
                records.Push(newRecord);
                RecordAdded?.Invoke(newRecord);
            }
        }

        private static bool ShouldAggregate(MonitorRecord pinnedRecord, MonitorRecord nextRecord)
        {
            var range = pinnedRecord.Bandwidth / 2.0;
            var lowerBound = pinnedRecord.Frequency - range;
            var upperBound = pinnedRecord.Frequency + range;

            return nextRecord.Frequency >= lowerBound && nextRecord.Frequency < upperBound;
        }
    }
}