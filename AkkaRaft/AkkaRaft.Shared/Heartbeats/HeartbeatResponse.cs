using System;
using System.Collections.Generic;
using System.Text;

namespace AkkaRaft.Shared.Heartbeats
{
    public class HeartbeatResponse
    {
        public double Id { get; set; }
        public int LogIndex { get; set; }
        public int Term { get; set; }

        public HeartbeatResponse(double id, int term, int logIndex)
        {
            Id = id;
            Term = term;
            LogIndex = logIndex;
        }
    }
}