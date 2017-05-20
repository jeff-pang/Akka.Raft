using System;
using System.Collections.Generic;
using System.Text;

namespace AkkaRaft.Shared.Logs
{
    public class LogEntryResponse
    {
        public int Index { get; set; }
        public int Term { get; set; }
    }
}