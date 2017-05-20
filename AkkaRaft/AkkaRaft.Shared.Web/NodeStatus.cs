using System;

namespace AkkaRaft.Shared.Web
{
    public class NodeStatus
    {        
        public double Time { get; private set; }
        public int Term { get; private set; }
        public int NodeId { get; private set; }
        public int ElectionDuration { get; set; }
        public int ElectionElapsed { get; set; }
        public bool IsLeader { get; set; }
        public string Role { get; set; }
        public int ProcessId { get; set; }
        public bool Terminated { get; set; }
        public int Votes { get; set; }
        public int Majority { get; set; }

        public NodeStatus(int term, int clusterUniqueId)
        {
            Time = (DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
            NodeId = clusterUniqueId;
            Term = term;
        }
    }
}