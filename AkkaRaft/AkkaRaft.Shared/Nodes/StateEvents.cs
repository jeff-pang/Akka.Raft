using AkkaRaft.Shared.Candidates;
using AkkaRaft.Shared.Heartbeats;
using AkkaRaft.Shared.Logs;
using System;
using System.Collections.Generic;
using System.Text;

namespace AkkaRaft.Shared.Nodes
{
    public class StateEvents
    {
        public Action OnElectionTimeout { get; set; }
        public Action OnWaitForVoteTimeout { get; set; }
        public Action<string,Heartbeat> OnHeartbeat { get; set; }
        public Action<HeartbeatResponse> OnHeartbeatResponse { get; set; }
        public Action<int,int> OnGotVote { get; set; }
        public Func<VoteRequest,bool> OnVoteRequest { get; set; }
        public Func<long,LogEntry[]> OnSendHearbeat { get; set; }

        public Action<string> OnReceiveData { get; set; }

        public enum Events
        {
            None=0,
            OnElectionTimeout=1,
            MemberChanged=2,
            OnHeartbeat=3,
        }
        
        public void Clear()
        {
            OnElectionTimeout = null;
            OnWaitForVoteTimeout = null;
            OnHeartbeat = null;
            OnHeartbeatResponse = null;
            OnGotVote = null;
            OnVoteRequest = null;
            OnSendHearbeat = null;
        }
    }

}