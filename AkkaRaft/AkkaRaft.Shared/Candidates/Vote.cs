using System;
using System.Collections.Generic;
using System.Text;

namespace AkkaRaft.Shared.Candidates
{
    public class Vote
    {
        public int Term { get; private set; }
        public Vote(int term)
        {
            Term = term;
        }
    }
}
