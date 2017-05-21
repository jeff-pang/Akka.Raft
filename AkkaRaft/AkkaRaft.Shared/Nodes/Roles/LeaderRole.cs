using Serilog;
using System;

namespace AkkaRaft.Shared.Nodes.Roles
{
    public class LeadereRole : Role
    {
        public LeadereRole(RoleContext ctx) : base(ctx)
        {
            Node.CurrentLeaderId = Node.Uid;
            ActionBroker.StartHeartbeat();

            Context.NodeEvents.OnHeartbeat = (senderpath, hb) =>
            {
                if(hb.Term> Node.Term)
                {
                    Log.Information("{0}", "Stepping down");
                    ActionBroker.StopHeartbeat();
                    Context.Transition<FollowerRole>();
                }
            };
        }

        public override string GetName()
        {
            return "Leader";
        }
    }
}