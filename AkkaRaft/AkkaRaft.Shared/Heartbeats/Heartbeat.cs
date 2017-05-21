using AkkaRaft.Shared.Logs;
using System;
using System.Collections.Generic;

namespace AkkaRaft.Shared.Heartbeats
{
    public class Heartbeat
    {
        public double Id { get; private set; }
        public int Term { get; private set; }
        public int SenderId { get; private set; }
        public int? NextIndex { get; private set; }
        public LogEntry[] AppendEntries { get; private set; }

        public Heartbeat(int term, int? nextIndex, int senderId, LogEntry[] appendEntries = null)
        {
            Id = (DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalMilliseconds;
            SenderId = senderId;
            Term = term;
            NextIndex = nextIndex;
            AppendEntries = appendEntries;
        }
    }
}