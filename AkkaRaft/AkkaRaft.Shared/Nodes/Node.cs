using Serilog;
using System;
using System.Collections.Generic;
using System.Text;

namespace AkkaRaft.Shared.Nodes
{
    public class Node
    {
        public enum Roles
        {
            Follower=0,
            Candidate=1,
            Leader=2
        }

        public Roles Role { get; private set; }
        public static int Term { get; private set; }
        private int _votedForTerm;
        private int _majority;
        private int _votes;
        public static int ClusterUid { get; private set; }
        public Node(int clusterUniqueId)
        {
            ClusterUid = clusterUniqueId;

            Log.Information("{0}", $"My Uid is {ClusterUid}");

            NodeEvents.OnElectionTimeout = () => {
                //step up as candidate
                //ask for vote
                if (Role == Roles.Follower)
                {
                    Role = Roles.Candidate;
                    Term++;
                    _votes = 0;

                    Log.Information("{0}", $"Starting election for term {Term}");
                    ActionBroker.StopElectionTimer();
                    ActionBroker.AskForVote(Term);
                    ActionBroker.StartWaitForVote();
                }
            };

            NodeEvents.OnGotVote = (uid,term) =>
            {
                //if got more than majority vote, then becomes leader           
                //start sending heartbeat
                if (term == Term)
                {
                    _votes++;
                }

                Log.Information("{0}", $"Got {_votes}/{_majority} votes for term {term} from {uid}");
                if (_votes >= _majority)
                {
                    Log.Information("{0}", "Elected!");
                    Role = Roles.Leader;
                    ActionBroker.StopWaitForVote();
                    ActionBroker.StartHeartbeat();
                }
            };

            NodeEvents.OnHeartbeat = hb => {
                //resets election time
                //otherwise becomes candidate and send request for votes         
                ActionBroker.ResetElectionTimer();

                //if heartbeat has term equal or bigger than self, then step down
                if (hb.Term >= Term)
                {
                    if (Role != Roles.Follower)
                    {
                        Log.Information("{0}", "Stepping down");
                        Role = Roles.Follower;
                        ActionBroker.StopHeartbeat();
                        ActionBroker.StartElectionTimer();
                    }

                    if (hb.Term != Term)
                    {
                        Log.Information("{0}", $"Updating from term {Term} to {hb.Term}");
                        Term = hb.Term;
                    }

                }
            };

            NodeEvents.OnJoinedCluster = () => {
                if (Role != Roles.Leader)
                {
                    Log.Information("{0}","Starting election timer");
                    ActionBroker.StartElectionTimer();
                }
            };

            NodeEvents.OnVoteRequest = vr =>
            {
                //if not voted this term and is not a leader, then vote for the candidate
                if (_votedForTerm < vr.Term && Role != Roles.Leader)
                {
                    if (vr.SenderId != ClusterUid)
                    {
                        Log.Information("{0}", $"Vote request from candidate {vr.SenderId} for term {vr.Term}");
                    }
                    else
                    {
                        Log.Information("{0}", $"Vote request from self in term {vr.Term}");
                    }

                    _votedForTerm = vr.Term;

                    return true;
                }
                else
                {
                    Log.Information("{0}", $"Not voting. {vr.SenderId} asking for term {vr.Term}, last voted for {_votedForTerm} and role is {Role.ToString()}");
                    return false;
                }
            };

            NodeEvents.OnWaitForVoteTimeout = () =>
            {
                //restart election time                
                Role = Roles.Follower;
                ActionBroker.ResetElectionTimer();
                ActionBroker.StartElectionTimer();                
            };

            NodeEvents.OnMemberChanged = count => {
                //update majority
                _majority = (count + 1) / 2;
                Log.Information("{0}", $"Majority is now {_majority}");
            };
        }
        public void Stop(TimeSpan timeSpan)
        {
            ActionBroker.Stop(timeSpan);
        }
    }
}
