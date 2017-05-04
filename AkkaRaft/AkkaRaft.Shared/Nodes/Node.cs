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

        private int _term;
        private int _votedForTerm;
        private int _majority;
        private int _votes;
        private int _uid;
        public Node(int clusterUniqueId)
        {
            _uid = clusterUniqueId;
            NodeEvents.OnElectionTimeout = () => {
                //step up as candidate
                //ask for vote
                if (Role == Roles.Follower)
                {
                    Role = Roles.Candidate;
                    _term++;
                    _votes = 0;

                    Log.Information("{0}", $"Starting election for term {_term}");
                    ActionBroker.StopElectionTimer();
                    ActionBroker.AskForVote(_term);
                }
            };

            NodeEvents.OnGotVote = (uid,term) =>
            {
                //if got more than majority vote, then becomes leader           
                //start sending heartbeat
                if (term == _term)
                {
                    _votes++;
                }

                Log.Information("{0}", $"Got {_votes}/{_majority} votes for term {term}");
                if (_votes >= _majority)
                {
                    Role = Roles.Leader;
                    ActionBroker.StartHeartbeat();
                }
            };
            NodeEvents.OnHeartbeat = hb => {
                //resets election time
                //otherwise becomes candidate and send request for votes         
                ActionBroker.ResetElectionTimer();
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
                //if not voted this term and is not a candidate or leader, then vote for the candidate
                if (_votedForTerm < vr.Term)
                {
                    if (vr.SenderId != _uid)
                    {
                        _votedForTerm = vr.Term;
                        Log.Information("{0}", $"Voting for term {vr.Term}");
                    }
                    else
                    {
                        Log.Information("{0}", $"Voting for self in term {vr.Term}");
                    }
                    return true;
                }
                else
                    return false;
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
