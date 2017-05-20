using System;
using System.Collections.Generic;
using System.Text;

namespace AkkaRaft.Shared.Heartbeats
{
    public class SendHeartbeatResponse
    {
        public double HeartbeatId { get; private set; }
        public int Term { get; private set; }
        public int LogIndex { get; private set; }
        public int SenderId { get; private set; }
        public string SenderPath { get; private set; }

        public SendHeartbeatResponse(double heartbeatId,int senderId,string senderPath,int term,int logIndex)
        {
            HeartbeatId = heartbeatId;
            Term = term;
            LogIndex = LogIndex;
            SenderId = senderId;
            SenderPath = senderPath;
        }
    }
}
