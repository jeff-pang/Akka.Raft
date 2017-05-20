using Akka.Actor;
using System;
using System.Collections.Generic;
using System.Text;

namespace AkkaRaft.Shared.Leader
{
    public class LeaderActor: ReceiveActor
    {
        public LeaderActor()
        {
            Receive<SendAppendEntries>(s => {

            });
        }


    }
}
