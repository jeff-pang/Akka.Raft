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
using AkkaRaft.Shared.Web;
using AkkaRaft.Shared.Leader;

namespace AkkaRaft.Shared.Nodes
{
    public class NodeFactory
    {
        public Node MakeNode(ActorSystem system)
        {
            var cluster = Cluster.Get(system);
            int uid=cluster.SelfUniqueAddress.Uid;
            var node = new Node(uid);

            ActionBroker.SetLeader(system.ActorOf<LeaderActor>("leader"));
            ActionBroker.SetFollower(system.ActorOf<FollowerActor>("follower"));
            ActionBroker.SetCandidate(system.ActorOf<CandidateActor>("candidate"));
            ActionBroker.SetElection(system.ActorOf<ElectionCycleActor>("electionCycle"));
            ActionBroker.SetHeartbeat(system.ActorOf<HeartbeatActor>("heartbeat"));
            ActionBroker.SetStatusBroadcast(system.ActorOf<StatusBroadcastActor>("status"));
            
            return node;
        }
    }
}
