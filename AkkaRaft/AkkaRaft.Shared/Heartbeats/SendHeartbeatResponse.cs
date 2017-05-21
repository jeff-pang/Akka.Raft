using System;
using System.Collections.Generic;
using System.Text;

namespace AkkaRaft.Shared.Heartbeats
{
    public class SendHeartbeatResponse
    {
        public double HeartbeatId { get; private set; }
        public int Term { get; private set; }
        public int? NextIndex { get; private set; }
        public int MatchIndex { get; private set; }
        public string SenderPath { get; private set; }

        public SendHeartbeatResponse(double heartbeatId,int term,string senderPath,int matchIndex,int? nextIndex)
        {
            SenderPath = senderPath;
            HeartbeatId = heartbeatId;
            Term = term;
            NextIndex = nextIndex;
            MatchIndex = matchIndex;
        }
    }
}
