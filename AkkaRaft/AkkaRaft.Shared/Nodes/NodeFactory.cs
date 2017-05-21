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

namespace AkkaRaft.Shared.Nodes
{
    public class NodeFactory
    {
        public Node MakeNode(ActorSystem system)
        {
            var cluster = Cluster.Get(system);
            int uid=cluster.SelfUniqueAddress.Uid;
            StateEvents stateEvents = new StateEvents();
            NodeEvents nodeEvents = new NodeEvents();

            var node = new Node(uid, stateEvents, nodeEvents);

            ActionBroker.SetFollower(system.ActorOf(Props.Create<FollowerActor>(stateEvents),"follower"));
            ActionBroker.SetCandidate(system.ActorOf(Props.Create<CandidateActor>(stateEvents),"candidate"));
            
            ActionBroker.SetElection(system.ActorOf(Props.Create<ElectionCycleActor>(stateEvents, nodeEvents),"electionCycle"));
            ActionBroker.SetHeartbeat(system.ActorOf(Props.Create<HeartbeatActor>(stateEvents,nodeEvents),"heartbeat"));
            ActionBroker.SetStatusBroadcast(system.ActorOf(Props.Create<StatusBroadcastActor>(), "status"));
            //ActionBroker.SetData(system.ActorOf<DataActor>("data"));

            return node;
        }
    }
}
