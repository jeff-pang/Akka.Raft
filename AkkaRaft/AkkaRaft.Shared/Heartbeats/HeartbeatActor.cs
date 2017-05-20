using Akka.Actor;
using Akka.Cluster.Tools.PublishSubscribe;
using System;
using System.Linq;
using System.Collections.Generic;
using Akka.Cluster;
using static Akka.Cluster.ClusterEvent;
using Serilog;
using AkkaRaft.Shared.Nodes;

namespace AkkaRaft.Shared.Heartbeats
{
    public class HeartbeatActor:ReceiveActor
    {
        private const int HEARTBEAT_INTERVAL_MS = 1000;

        private bool _joinedCluster;
        private ICancelable _heartbeatTask;
        private int _membersCount;
        private bool _heartbeatStarted=false;

        protected Cluster cluster = Cluster.Get(Context.System);
        
        public HeartbeatActor()
        {
            var mediator = DistributedPubSub.Get(Context.System).Mediator;

            Receive<Heartbeat>(hb =>
            {
                if (Sender != Self)
                {
                    Console.Write(".");
                    NodeEvents.OnHeartbeat?.Invoke(Sender.Path.ToString(),hb);
                }
            });

            Receive<SendHeartbeatResponse>(s =>
            {
                var sender = Context.ActorSelection(s.SenderPath);
                sender.Tell(new HeartbeatResponse(s.HeartbeatId, s.Term, s.LogIndex));
            });

            Receive<SendHeartbeat>(send =>
            {
                Console.Write(">");
                mediator.Tell(new Publish("heartbeat", new Heartbeat(Node.Term,Node.LogIndex, Node.ClusterUid)));
            });

            Receive<HeartbeatResponse>(hbr =>
            {
                if (Sender != Self)
                {
                    NodeEvents.OnHeartbeatResponse?.Invoke(hbr);
                }
            });

            Receive<MemberStatusChange>(_ =>
            {
                var selfStatus = cluster.State.Members.Where(m => m.UniqueAddress.Uid == cluster.SelfUniqueAddress.Uid).FirstOrDefault()?.Status ?? MemberStatus.Down;
                if (!_joinedCluster && selfStatus == MemberStatus.Up)
                {
                    _joinedCluster = true;
                    NodeEvents.OnJoinedCluster?.Invoke();
                }
            
                var members = cluster.State.Members.Where(m => (m.Status == MemberStatus.Joining 
                    || m.Status == MemberStatus.Up) && m.Roles.Contains("heartbeat"));

                int membersCount = members.Count();
                if(_membersCount!=membersCount)
                {
                    _membersCount = membersCount;
                    Log.Information("{0}", $"{membersCount} members in cluster.");

                    foreach (var m in members)
                    {
                        Log.Information("{0}", $"Member {m.UniqueAddress.Uid} with roles {string.Join(",", m.Roles)} is {m.Status}");
                    }

                    NodeEvents.OnMemberChanged?.Invoke(_membersCount);
                }
            });

            Receive<StartStopHeartbeat>(s => {
                if(s.Start)
                {
                    if (!_heartbeatStarted)
                    {
                        _heartbeatStarted = true;
                        Log.Information("{0}", "Heartbeat Start");
                        _heartbeatTask = Context.System.Scheduler.ScheduleTellRepeatedlyCancelable(TimeSpan.FromMilliseconds(20),
                        TimeSpan.FromMilliseconds(HEARTBEAT_INTERVAL_MS), Context.Self, new SendHeartbeat(), ActorRefs.NoSender);
                    }
                }
                else
                {
                    if (_heartbeatStarted)
                    {
                        _heartbeatStarted = false;
                        Log.Information("{0}", "Heartbeat Stop");
                        _heartbeatTask?.Cancel();
                    }
                }
            });
        }

        protected override void PreStart()
        {
            cluster.Subscribe(Self, ClusterEvent.InitialStateAsEvents,
                new[] { typeof(ClusterEvent.IMemberEvent) });

            var mediator = DistributedPubSub.Get(Context.System).Mediator;
            mediator.Tell(new Subscribe("heartbeat", Self));
            
        }
        
        protected override void PostStop()
        {
            _heartbeatTask?.Cancel();
            cluster.Unsubscribe(Self);
        }
    }
}