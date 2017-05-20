using System;
using System.Collections.Generic;

namespace AkkaRaft.Shared.Heartbeats
{
    public class Heartbeat
    {        
        public double Id { get; private set; }
        public int Term { get; private set; }
        public int SenderId { get; private set; }
        public int LogIndex { get; private set; }

        public Heartbeat(int term, int logIndex,int senderId)
        {
            Id = (DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalMilliseconds;
            SenderId = senderId;
            Term = term;
            LogIndex = logIndex;
        }
    }
}