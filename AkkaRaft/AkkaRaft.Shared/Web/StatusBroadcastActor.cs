using Akka.Actor;
using Akka.Cluster.Tools.PublishSubscribe;
using AkkaRaft.Shared.Nodes;
using Serilog;
using System;
using System.Collections.Generic;
using System.Text;

namespace AkkaRaft.Shared.Web
{
    class SendStatus { };
    class SendTerminate { };

    public class StatusBroadcastActor:ReceiveActor
    {
        private const int BROADCAST_INTERVAL_MS = 500;
        private ICancelable _statusBroacast;

        public StatusBroadcastActor()
        {
            var mediator = DistributedPubSub.Get(Context.System).Mediator;

            Receive<SendStatus>(s =>
            {
                var ns = new NodeStatus(Node.Term, Node.ClusterUid)
                {
                    ElectionElapsed = Node.ElectionElapsed,
                    ElectionDuration = Node.ElectionDuration,
                    IsLeader = (Node.Role == Node.Roles.Leader),
                    Role = Node.Role.ToString(),
                    ProcessId = Node.ProcessId,
                    Votes = Node.Votes,
                    Majority = Node.Majority
                };

                mediator.Tell(new Publish("nodestatus", ns));
            });

            Receive<SendTerminate>(s =>
            {
                var ns = new NodeStatus(Node.Term, Node.ClusterUid)
                {
                    ElectionElapsed = Node.ElectionElapsed,
                    ElectionDuration = Node.ElectionDuration,
                    IsLeader = (Node.Role == Node.Roles.Leader),
                    Role = Node.Role.ToString(),
                    ProcessId = Node.ProcessId,
                    Votes = Node.Votes,
                    Majority = Node.Majority,
                    Terminated = true
                };
                mediator.Tell(new Publish("nodestatus", ns));
            });
        }

        protected override void PreStart()
        {
            Log.Information("{0}", "Status Broadcast Started");
            _statusBroacast = Context.System.Scheduler.ScheduleTellRepeatedlyCancelable(TimeSpan.FromMilliseconds(20),
           TimeSpan.FromMilliseconds(BROADCAST_INTERVAL_MS), Context.Self, new SendStatus(), ActorRefs.NoSender);
        }
    }
}