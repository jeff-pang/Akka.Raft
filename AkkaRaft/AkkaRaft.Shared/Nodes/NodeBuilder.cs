using System;
using System.Collections.Generic;
using System.Text;
using AkkaRaft.Shared.Nodes;
using Akka.Actor;
using AkkaRaft.Shared.Heartbeats;
using AkkaRaft.Shared.Elections;
using AkkaRaft.Shared.Candidates;
using AkkaRaft.Shared.Followers;
using Akka.Cluster;

namespace AkkaRaft.Shared.Nodes
{
    public class NodeBuilder
    {
        public Node MakeNode(ActorSystem system)
        {
            int uid=Cluster.Get(system).SelfUniqueAddress.Uid;
            var node = new Node(uid);

            ActionBroker.SetFollower(system.ActorOf<FollowerActor>("follower"));
            ActionBroker.SetCandidate(system.ActorOf<CandidateActor>("candidate"));
            ActionBroker.SetElection(system.ActorOf<ElectionCycleActor>("electionCycle"));
            ActionBroker.SetHeartbeat(system.ActorOf<HeartbeatActor>("heartbeat"));
            
            return node;
        }
    }
}
