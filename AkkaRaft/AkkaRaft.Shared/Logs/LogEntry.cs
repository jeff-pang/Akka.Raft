using System;
using System.Collections.Generic;
using System.Text;

namespace AkkaRaft.Shared.Logs
{
    public class LogEntry
    {
        public string Data { get; set; }
        public bool IsCommited { get; set; }
        public int Index { get; set; }
        public int Term { get; set; }
    }
}