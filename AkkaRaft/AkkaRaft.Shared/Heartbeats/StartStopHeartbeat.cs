using System;
using System.Collections.Generic;
using System.Text;

namespace AkkaRaft.Shared.Heartbeats
{
    public class StartStopHeartbeat
    {
        public bool Start { get; private set; }
        public StartStopHeartbeat(bool start)
        {
            Start = start;
        }
    }
}
