using System;
using System.Collections.Generic;
using System.Text;

namespace AkkaRaft.Shared.Leader
{
    public class SendAppendEntries
    {
        public int SenderId { get; private set; }
        public int LogIndex { get; private set; }
        public int Term { get; private set; }
        public SendAppendEntries(int senderId, int term, int logIndex)
        {
            SenderId = senderId;
            Term = term;
        }
    }
}