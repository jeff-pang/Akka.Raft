using System;
using System.Collections.Generic;
using System.Text;

namespace AkkaRaft.Shared.Nodes
{
    public class FollowerState
    {
        public int MatchIndex { get; set; }
    }
}
