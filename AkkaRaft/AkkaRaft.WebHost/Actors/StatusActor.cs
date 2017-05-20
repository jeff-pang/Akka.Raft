using Akka.Actor;
using Akka.Cluster;
using Akka.Cluster.Tools.PublishSubscribe;
using AkkaRaft.Shared.Web;
using Serilog;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using static Akka.Cluster.ClusterEvent;
using AkkaRaft.WebHost.Websockets;
using System;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace AkkaRaft.WebHost.Actors
{
    public class StatusActor:ReceiveActor
    {
        protected Cluster cluster = Cluster.Get(Context.System);

        public StatusActor()
        {
            Receive<NodeStatus>(s =>
            {
                Console.Write(".");
                if (s.Role == "Leader")
                {

                }
                Action a = async () =>
                {
                    string json = JsonConvert.SerializeObject(s);
                    await WebsockController.Instance.SendMessageAsync(json);
                };
                a.Invoke();
            });
            
            Receive<MemberStatusChange>(_ =>
            {
                var selfStatus = cluster.State.Members.Where(m => m.UniqueAddress.Uid == cluster.SelfUniqueAddress.Uid).FirstOrDefault()?.Status ?? MemberStatus.Down;
                
                var members = cluster.State.Members.Where(m => (m.Status == MemberStatus.Joining
                    || m.Status == MemberStatus.Up) && m.Roles.Contains("heartbeat"));

                int membersCount = members.Count();
                {
                    Log.Information("{0}", $"{membersCount} members in cluster.");

                    foreach (var m in members)
                    {
                        Log.Information("{0}", $"Member {m.UniqueAddress.Uid} with roles {string.Join(",", m.Roles)} is {m.Status}");
                    }
                }
            });
        }

        protected override void PreStart()
        {
            cluster.Subscribe(Self, ClusterEvent.InitialStateAsEvents,
                new[] { typeof(ClusterEvent.IMemberEvent) });
            
            var mediator = DistributedPubSub.Get(Context.System).Mediator;
            mediator.Tell(new Subscribe("nodestatus", Self));
        }
    }
}
