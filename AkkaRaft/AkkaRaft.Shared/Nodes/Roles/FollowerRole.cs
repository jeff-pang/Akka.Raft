using Serilog;
using System;
using System.Collections.Generic;
using System.Text;

namespace AkkaRaft.Shared.Nodes.Roles
{
    public class FollowerRole : Role
    {
        public FollowerRole(RoleContext ctx) : base(ctx)
        {
            ActionBroker.ResetElectionTimer();
            ActionBroker.StartElectionTimer();

            Context.NodeEvents.OnHeartbeat = (senderpath, hb) =>
            {
                ActionBroker.ResetElectionTimer();
                Node.Term = hb.Term;
                ActionBroker.SendHeartbeatResponse(hb.Id, Node.Term, senderpath,Node.CommitIndex, Node.CommitIndex);
            };

            Context.NodeEvents.OnElectionTimeout = () =>
            {
                ActionBroker.StopElectionTimer();
                Context.Transition<CandidateRole>();
            };

            Context.NodeEvents.OnVoteRequest = request =>
            {
                ActionBroker.StopElectionTimer();

                //if not voted this term

                if (Node.VotedForTerm < request.Term)
                {
                    Log.Information("{0}", $"Voting for {request.SenderId}");
                    Node.VotedForTerm = request.Term;

                    return true;
                }
                else
                {
                    Log.Information("{0}", $"Not voting. {request.SenderId} asking for term {request.Term}, last voted for {Node.Term}");
                    return false;
                }
            };
        }

        public override string GetName()
        {
            return "Follower";
        }

    }
}