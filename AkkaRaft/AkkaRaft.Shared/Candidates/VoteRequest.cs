using System;
using System.Collections.Generic;
using System.Text;

namespace AkkaRaft.Shared.Candidates
{
    public class VoteRequest
    {
        public int Term { get; private set; }
        public int SenderId { get; private set; }
        public VoteRequest(int term,int clusterUniqueId)
        {
            SenderId = clusterUniqueId;
            Term = term;
        }
    }
}