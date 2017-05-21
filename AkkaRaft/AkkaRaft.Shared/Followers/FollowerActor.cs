using Akka.Actor;
using Akka.Cluster;
using Akka.Cluster.Tools.PublishSubscribe;
using AkkaRaft.Shared.Candidates;
using AkkaRaft.Shared.Logs;
using AkkaRaft.Shared.Nodes;
using Serilog;
using System;
using System.Collections.Generic;
using System.Text;

namespace AkkaRaft.Shared.Followers
{
    public class FollowerActor:ReceiveActor
    {
        public FollowerActor(StateEvents nodeEvents)
        {
            Receive<VoteRequest>(vr =>
            {
                bool vote = nodeEvents.OnVoteRequest?.Invoke(vr) ?? false;
                if (vote)
                {
                    Sender.Tell(new Vote(vr.Term,Node.Uid));
                }
            });            
        }

        protected override void PreStart()
        {
            var mediator = DistributedPubSub.Get(Context.System).Mediator;
            mediator.Tell(new Subscribe("voterequest", Self));
        }
    }
}