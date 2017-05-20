using AkkaRaft.Shared.Candidates;
using AkkaRaft.Shared.Heartbeats;
using System;
using System.Collections.Generic;
using System.Text;

namespace AkkaRaft.Shared.Nodes
{
    public class NodeEvents
    {
        public static Action OnElectionTimeout { get; set; }
        public static Action<int> OnElectionDurationChanged { get; set; }
        public static Action<int> OnElectionElapsed { get; set; }
        public static Action OnWaitForVoteTimeout { get; set; }
        public static Action<string,Heartbeat> OnHeartbeat { get; set; }
        public static Action OnJoinedCluster { get; set; }
        public static Action<HeartbeatResponse> OnHeartbeatResponse { get; set; }
        public static Action<int> OnMemberChanged { get; set; }
        public static Action<int,int> OnGotVote { get; set; }
        public static Func<VoteRequest,bool> OnVoteRequest { get; set; }
        
        public enum Events
        {
            None=0,
            OnElectionTimeout=1,
            MemberChanged=2,
            OnHeartbeat=3,
        }
        
        public Events Event { get; private set; }
        public object[] Args { get; private set; }
        public NodeEvents(Events e,params object[] args)
        {
            Event = e;
            Args = args;
        }
    }
}