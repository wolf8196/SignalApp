using System;

namespace SignalApp.Core
{
    public interface IRecordManager
    {
        event Action<MonitorRecord>? RecordAdded;

        event Action<MonitorRecord>? RecordUpdated;

        void PostRecord(MonitorRecord record);
    }
}