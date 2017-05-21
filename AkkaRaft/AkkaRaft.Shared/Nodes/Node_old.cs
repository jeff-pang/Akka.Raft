//using AkkaRaft.Shared.Logs;
//using Serilog;
//using System;
//using System.Linq;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.Text;

//namespace AkkaRaft.Shared.Nodes
//{
//    public class Node_old
//    {
//        public enum Roles
//        {
//            Follower = 0,
//            Candidate = 1,
//            Leader = 2
//        }

//        public static int Term { get; private set; }
//        public static int ClusterUid { get; private set; }
//        public static int CurrentLeaderId { get; private set; }
//        public static int ElectionElapsed { get; set; }
//        public static int ElectionDuration { get; set; }

//        public static List<LogEntry> LogEntries { get; private set; }

//        public static int CommitIndex { get; private set; }

//        public static Dictionary<long, FollowerState> Followers { get; private set; }

//        static Roles _role;
//        public static Roles Role
//        {
//            get { return _role; }
//            set
//            {
//                Log.Information("{0}", $"Role is now {_role.ToString()}");
//                _role = value;
//            }
//        }
//        private int _votedForTerm;

//        public static int Votes { get; private set; }
//        public static int Majority { get; private set; }
//        public static int ProcessId { get; private set; }
//        public static string LeaderPath { get; private set; }
//        public Node_old(int clusterUniqueId)
//        {
//            Followers = new Dictionary<long, FollowerState>();
//            ProcessId = Process.GetCurrentProcess().Id;
//            ClusterUid = clusterUniqueId;
//            Log.Information("{0}", $"My Uid is {ClusterUid}");
//            LogEntries = new List<LogEntry>();

//            NodeEvents.OnElectionDurationChanged = dur => ElectionDuration = dur;
//            NodeEvents.OnElectionElapsed = el => ElectionElapsed = el;

//            NodeEvents.OnElectionTimeout = () =>
//            {
//                //step up as candidate
//                //ask for vote
//                if (Role == Roles.Follower)
//                {
//                    Role = Roles.Candidate;
//                    Term++;
//                    Votes = 0;

//                    Log.Information("{0}", $"Starting election for term {Term}");
//                    ActionBroker.StopElectionTimer();
//                    ActionBroker.AskForVote(Term);
//                    ActionBroker.StartWaitForVote();
//                }
//            };

//            NodeEvents.OnGotVote = (uid, term) =>
//            {
//                //if got more than majority vote, then becomes leader           
//                //start sending heartbeat
//                if (term == Term)
//                {
//                    Votes++;
//                }

//                Log.Information("{0}", $"Got {Votes}/{Majority} votes for term {term} from {uid}");
//                if (Votes >= Majority)
//                {
//                    Log.Information("{0}", "Elected!");
//                    Role = Roles.Leader;
//                    CurrentLeaderId = ClusterUid;
//                    ActionBroker.StopWaitForVote();
//                    ActionBroker.StartHeartbeat();
//                }
//            };

//            NodeEvents.OnHeartbeat = (senderpath, hb) =>
//            {
//                //resets election time
//                //otherwise becomes candidate and send request for votes         
//                ActionBroker.ResetElectionTimer();

//                //if heartbeat has term equal or bigger than self, then step down
//                if (hb.Term >= Term)
//                {
//                    //need to roll back changes and take match leader's log entries
//                    if (Role != Roles.Follower)
//                    {
//                        Log.Information("{0}", "Stepping down");
//                        Role = Roles.Follower;
//                        ActionBroker.StopHeartbeat();
//                        ActionBroker.StartElectionTimer();
//                    }

//                    if (hb.Term != Term)
//                    {
//                        Log.Information("{0}", $"Updating from term {Term} to {hb.Term}");
//                        Term = hb.Term;
//                    }
//                }

//                CurrentLeaderId = hb.SenderId;
//                LeaderPath = senderpath;
//                if (Role == Roles.Follower)
//                {
//                    if (hb.AppendEntries != null)
//                    {
//                        //compare from the last element to the first
//                        //if the index is lesser and term is different, remove it
//                        var reversed = hb.AppendEntries.OrderByDescending(e => e.Index);
//                        foreach (var entry in reversed)
//                        {
//                            if ((LogEntries.Count >= entry.Index) && LogEntries[entry.Index].Term != entry.Term)
//                            {
//                                int countToRemove = LogEntries.Count - entry.Index;
//                                LogEntries.RemoveRange(entry.Index, countToRemove);
//                                break;
//                            }
//                        }

//                        foreach (var entry in hb.AppendEntries)
//                        {
//                            LogEntries.Add(entry);
//                            CommitIndex = entry.Index;
//                        }
//                    }

//                    var lastEntry = LogEntries.LastOrDefault();
//                    int nextIndex = (lastEntry?.Index ?? 0) + 1;
//                    int term = lastEntry?.Term ?? 0;
//                    ActionBroker.SendHeartbeatResponse(hb.Id, hb.SenderId, senderpath, term, CommitIndex, nextIndex);
//                }
//            };

//            NodeEvents.OnSendHearbeat = senderId =>
//            {
//                if (Followers.ContainsKey(senderId))
//                {
//                    var info = Followers[senderId];

//                    if (info.MatchIndex < CommitIndex)
//                    {
//                        var entries = LogEntries.Where(e => e.Index > info.MatchIndex).OrderBy(e => e.Index);
//                        return entries.ToArray();
//                    }
//                }
//                return null;
//            };

//            NodeEvents.OnHeartbeatResponse = hbr =>
//            {
//                if (Followers.ContainsKey(hbr.SenderId))
//                {
//                    var info = Followers[hbr.SenderId];
//                    info.MatchIndex = hbr.LogIndex ?? 0;
//                }
//            };

//            NodeEvents.OnJoinedCluster = () =>
//            {
//                if (Role != Roles.Leader)
//                {
//                    Log.Information("{0}", "Starting election timer");
//                    ActionBroker.StartElectionTimer();
//                }
//            };

//            NodeEvents.OnReceiveData = data =>
//            {
//                if (Role == Roles.Leader)
//                {
//                    var entry = new LogEntry
//                    {
//                        Index = LogEntries.Count,
//                        Term = Term,
//                        Data = data
//                    };
//                    LogEntries.Add(entry);
//                    Log.Information("Appended entry at index {0} Term {1}", entry.Index, entry.Term);
//                }
//                else
//                {
//                    //redirect to leader
//                    ActionBroker.SendData(data, LeaderPath);
//                }
//            };

//            NodeEvents.OnVoteRequest = vr =>
//            {
//                //if not voted this term and is not a leader, then vote for the candidate
//                if (_votedForTerm < vr.Term && Role != Roles.Leader)
//                {
//                    if (vr.SenderId != ClusterUid)
//                    {
//                        Log.Information("{0}", $"Vote request from candidate {vr.SenderId} for term {vr.Term}");
//                    }
//                    else
//                    {
//                        Log.Information("{0}", $"Vote request from self in term {vr.Term}");
//                    }

//                    _votedForTerm = vr.Term;

//                    return true;
//                }
//                else
//                {
//                    Log.Information("{0}", $"Not voting. {vr.SenderId} asking for term {vr.Term}, last voted for {_votedForTerm} and role is {Role.ToString()}");
//                    return false;
//                }
//            };

//            NodeEvents.OnWaitForVoteTimeout = () =>
//            {
//                //restart election time                
//                Role = Roles.Follower;
//                Votes = 0;
//                ActionBroker.ResetElectionTimer();
//                ActionBroker.StartElectionTimer();
//            };

//            NodeEvents.OnMemberChanged = memberIds =>
//            {
//                //update majority

//                Majority = (memberIds.Count + 1) / 2;
//                Log.Information("{0}", $"Majority is now {Majority}");
//            };

//        }

//        public void OnKill()
//        {
//            ActionBroker.SendTerminateSignal();
//        }

//        public void Stop(TimeSpan timeSpan)
//        {
//            ActionBroker.Stop(timeSpan);
//        }
//    }
//}
