using Serilog;
using System;

namespace AkkaRaft.Shared.Nodes.Roles
{
    public class CandidateRole : Role
    {
        public CandidateRole(RoleContext ctx) : base(ctx)
        {
            ActionBroker.StartWaitForVote();

            Node.Term++;
            Node.Votes = 0;

            Log.Information("{0}", $"Election Term {Node.Term}");

            gotVote(Node.Uid, Node.Term);
            Context.NodeEvents.OnHeartbeat = (senderpath, hb) =>
            {
                if(hb.Term>=Node.Term)
                {
                    Log.Information("{0}", "Stepping down");
                    ActionBroker.StopWaitForVote();
                    ActionBroker.StopHeartbeat();
                    Context.Transition<FollowerRole>();
                }
            };

            Context.NodeEvents.OnWaitForVoteTimeout = () => {
                ActionBroker.StartWaitForVote();
            };

            Context.NodeEvents.OnElectionTimeout = () =>
            {
                Context.Transition<CandidateRole>();
            };

            void gotVote(int uid,int term)
            {
                //if got more than majority vote, then becomes leader           
                //start sending heartbeat
                if (term == Node.Term)
                {
                    Node.Votes++;
                    Log.Information("{0}", $"Got {Node.Votes}/{Node.Majority} Vote");
                }

                if (Node.Votes >= Node.Majority)
                {
                    Log.Information("{0}", "Elected!");
                    ActionBroker.StopWaitForVote();
                    Context.Transition<LeadereRole>();
                }
            }

            Context.NodeEvents.OnGotVote = (uid, term) =>
            {
                gotVote(uid, term);
            };
        }

        public override string GetName()
        {
            return "Candidate";
        }
    }
}