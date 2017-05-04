using System;

namespace AkkaRaft.Shared.Heartbeats
{
    public class Heartbeat
    {        
        public double Id { get; private set; }
        public int Term { get; private set; }
        public int SenderId { get; private set; }
        public Heartbeat(int term, int clusterUniqueId)
        {
            Id = (DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalMilliseconds;
            SenderId = clusterUniqueId;
            Term = term;
        }
    }
}