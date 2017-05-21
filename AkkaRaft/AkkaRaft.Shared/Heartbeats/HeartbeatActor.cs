using Akka.Actor;
using Akka.Cluster.Tools.PublishSubscribe;
using System;
using System.Linq;
using System.Collections.Generic;
using Akka.Cluster;
using static Akka.Cluster.ClusterEvent;
using Serilog;
using AkkaRaft.Shared.Nodes;
using System.Threading.Tasks;
using AkkaRaft.Shared.Logs;

namespace AkkaRaft.Shared.Heartbeats
{
    public class HeartbeatActor:ReceiveActor
    {
        class SubAck { }
        class SubscribeHeartbeat { }

        private const int HEARTBEAT_INTERVAL_MS = 100;

        private bool _joinedCluster;
        private ICancelable _heartbeatTask;
        private bool _heartbeatStarted=false;
        private List<string> _subscribers;
        protected Cluster cluster = Cluster.Get(Context.System);
        private bool subscribed;
        public HeartbeatActor(StateEvents stateEvents,NodeEvents nodeEvents)
        {
            _subscribers = new List<string>();

            var mediator = DistributedPubSub.Get(Context.System).Mediator;

            Receive<SubscribeHeartbeat>(sub =>
            {
                string senderPath = Sender.Path.ToString();
                int newMembers = 0;
                if (!_subscribers.Contains(senderPath) && Sender != Self)
                {
                    Log.Information("{0}", "Subscriber added");
                    _subscribers.Add(senderPath);
                    newMembers++;
                }

                if (newMembers > 0)
                {
                    nodeEvents.OnMemberChanged?.Invoke(_subscribers.ToArray());
                }
            });

            Receive<SubAck>(ack => {
                if (!subscribed)
                {
                    mediator.Tell(new Publish("heartbeat", new SubscribeHeartbeat()), Self);
                    subscribed = true;
                }
            });
            
            Receive<Heartbeat>(hb =>
            {
                if (Sender != Self)
                {
                    Console.Write(".");
                    stateEvents.OnHeartbeat?.Invoke(Sender.Path.ToString(), hb);
                }
            });

            Receive<SendHeartbeatResponse>(s =>
            {
                var sender = Context.ActorSelection(s.SenderPath);
                sender.Tell(new HeartbeatResponse(s.HeartbeatId, Sender.Path.Uid, s.Term, s.NextIndex));
            });
            
            Receive<SendHeartbeat>(send =>
            {
                if (_subscribers.Count > 0)
                {
                    Console.Write(">");
                }

                var logEntries = stateEvents.OnSendHearbeat?.Invoke(Sender.Path.Uid);

                foreach (var subPath in _subscribers)
                {
                    var sub = Context.ActorSelection(subPath);
                    int nextIndex = (logEntries?.FirstOrDefault()?.Index ?? 0) + 1;
                    sub.Tell(new Heartbeat(Node.Term, nextIndex, Node.Uid, logEntries));
                }
            });

            Receive<HeartbeatResponse>(hbr =>
            {
                if (Sender != Self)
                {
                    stateEvents.OnHeartbeatResponse?.Invoke(hbr);
                }
            });

            //Receive<MemberStatusChange>(_ =>
            //{                
            //    var memberIds = cluster.State.Members
            //                    .Where(m => m.Status == MemberStatus.Up && m.Roles.Contains("heartbeat"))
            //                    .Select(m=>m.UniqueAddress.Uid).ToArray();

            //    nodeEvents.OnMemberChanged?.Invoke(memberIds);
            //});

            Receive<StartStopHeartbeat>(s => {
                Self.Tell(new SubAck());
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