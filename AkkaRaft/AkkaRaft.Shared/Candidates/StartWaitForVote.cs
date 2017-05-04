using System;
using System.Collections.Generic;
using System.Text;

namespace AkkaRaft.Shared.Candidates
{
    public class StartWaitForVote
    {
        public bool Start { get; private set; }
        public StartWaitForVote(bool start)
        {
            Start = start;
        }
    }
}
