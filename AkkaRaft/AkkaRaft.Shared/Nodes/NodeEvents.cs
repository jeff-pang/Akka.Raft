using System;
using System.Collections.Generic;
using System.Text;

namespace AkkaRaft.Shared.Nodes
{
    public class NodeEvents
    {
        public Action<string[]> OnMemberChanged { get; set; }
        public Action<int> OnElectionDurationChanged { get; set; }
        public Action<int> OnElectionElapsed { get; set; }

    }
}
