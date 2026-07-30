using System;
using System.Collections.Generic;
using System.Linq;

namespace SignalApp.Core
{
    internal sealed class RecordManagerV2 : IRecordManager
    {
        private readonly Stack<MonitorRecord> records;
        private readonly List<MonitorRecord> pinnedRecordGroup;

        public RecordManagerV2()
        {
            records = new Stack<MonitorRecord>();
            pinnedRecordGroup = new List<MonitorRecord>();
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
                    pinnedRecordGroup.Add(record);
                    // use median record only for notifications
                    // keep everything internally as is
                    var medianRecord = GetMedianPinnedRecord();
                    RecordUpdated?.Invoke(medianRecord with { Id = updatedRecord.Id, Count = updatedRecord.Count, IsLive = updatedRecord.IsLive });
                }
                else
                {
                    var closedRecord = pinnedRecord with { IsLive = false };
                    records.Push(closedRecord);
                    var medianRecord = GetMedianPinnedRecord();
                    pinnedRecordGroup.Clear();
                    RecordUpdated?.Invoke(medianRecord with { Id = closedRecord.Id, Count = closedRecord.Count, IsLive = closedRecord.IsLive });

                    AddNewRecord(record);
                }
            }
            else
            {
                AddNewRecord(record);
            }
        }

        private static bool ShouldAggregate(MonitorRecord pinnedRecord, MonitorRecord nextRecord)
        {
            var range = pinnedRecord.Bandwidth / 2.0;
            var lowerBound = pinnedRecord.Frequency - range;
            var upperBound = pinnedRecord.Frequency + range;

            return nextRecord.Frequency >= lowerBound && nextRecord.Frequency < upperBound;
        }

        private void AddNewRecord(MonitorRecord record)
        {
            var newRecord = record with { Count = 1, IsLive = true };
            records.Push(newRecord);
            pinnedRecordGroup.Add(record);
            RecordAdded?.Invoke(newRecord);
        }

        private MonitorRecord GetMedianPinnedRecord()
        {
            var sorted = pinnedRecordGroup.OrderBy(x => x.Frequency).ToList();
            return sorted.Count % 2 == 0
                ? sorted[sorted.Count / 2 - 1] // pick lower record if there are two median records (no reasons)
                : sorted[sorted.Count / 2];
        }
    }
}