using System;
using System.Collections.Generic;
using System.Text;

namespace AkkaRaft.Shared.Elections
{
    public class StartStopTime
    {
        public bool Start { get; private set; }
        public StartStopTime(bool start)
        {
            Start = start;
        }
    }
}
