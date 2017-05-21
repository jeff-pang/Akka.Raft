using System;
using System.Collections.Generic;
using System.Text;

namespace AkkaRaft.Shared.Heartbeats
{
    public class HeartbeatResponse
    {
        public double Id { get; private set; }
        public int? LogIndex { get; private set; }
        public int Term { get; private set; }
        public long SenderId { get; private set; }

        public HeartbeatResponse(double id,long senderId, int term, int? logIndex)
        {
            Id = id;
            Term = term;
            LogIndex = logIndex;
            SenderId = senderId;
        }
    }
}