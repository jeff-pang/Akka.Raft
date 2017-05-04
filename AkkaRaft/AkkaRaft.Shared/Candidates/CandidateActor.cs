using Akka.Actor;
using Akka.Cluster;
using Akka.Cluster.Tools.PublishSubscribe;
using AkkaRaft.Shared.Nodes;
using System;
using System.Collections.Generic;
using System.Text;

namespace AkkaRaft.Shared.Candidates
{
    public class CandidateActor:ReceiveActor
    {
        public CandidateActor()
        {
            var mediator = DistributedPubSub.Get(Context.System).Mediator;
            var uid = Cluster.Get(Context.System).SelfUniqueAddress.Uid;
            Receive<AskForVote>(a=>{                
                mediator.Tell(new Publish("voterequest", new VoteRequest(a.Term, uid)));
            });

            Receive<Vote>(v => {                

                NodeEvents.OnGotVote?.Invoke(Sender.Path.Uid, v.Term);
            });
        }
    }
}