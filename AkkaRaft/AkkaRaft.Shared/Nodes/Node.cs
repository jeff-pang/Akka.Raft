using AkkaRaft.Shared.Logs;
using AkkaRaft.Shared.Nodes.Roles;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace AkkaRaft.Shared.Nodes
{
    public class Node
    {
        public static int Term { get; set; }
        public static int Uid { get; set; }
        public static int CurrentLeaderId { get; set; }
        public static int ElectionElapsed { get; set; }
        public static int ElectionDuration { get; set; }
        public static string Role { get; private set; }
        public static List<LogEntry> LogEntries { get; set; }

        public static int CommitIndex { get; private set; }

        public static Dictionary<long, FollowerState> Followers { get; set; }
        
        public static int VotedForTerm { get; set; }

        public static int Votes { get; set; }
        public static int Majority { get; set; }
        public static int ProcessId { get; set; }
        public static string LeaderPath { get; set; }

        private RoleContext _role;
        public Node(int id, StateEvents stateEvents, NodeEvents nodeEvents)
        {
            Followers = new Dictionary<long, FollowerState>();
            ProcessId = Process.GetCurrentProcess().Id;
            Uid = id;
            Log.Information("{0}", $"My Uid is {Uid}");
            LogEntries = new List<LogEntry>();

            _role = RoleContext.Start(stateEvents);

            _role.OnRoleChanged = name =>
                Role = name;
            nodeEvents.OnElectionDurationChanged = dur =>
                ElectionDuration = dur;
            nodeEvents.OnElectionElapsed = el => 
                ElectionElapsed = el;
            nodeEvents.OnMemberChanged = memberIds =>
            {
                Majority = memberIds.Length;
                Log.Information("{0}", $"Majority is now {Majority}");
            };
        }
    }
}