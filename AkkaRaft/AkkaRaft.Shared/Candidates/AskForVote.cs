using System;
using System.Collections.Generic;
using System.Text;

namespace AkkaRaft.Shared.Candidates
{
    public class AskForVote
    {
        public int Term { get; private set; }
        public AskForVote(int term)
        {
            Term = term;
        }
    }
}