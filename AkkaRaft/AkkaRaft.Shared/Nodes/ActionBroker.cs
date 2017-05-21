using Akka.Actor;
using AkkaRaft.Shared.Candidates;
using AkkaRaft.Shared.Elections;
using AkkaRaft.Shared.Heartbeats;
using AkkaRaft.Shared.Web;
using System;
using System.Collections.Generic;
using System.Text;

namespace AkkaRaft.Shared.Nodes
{
    internal class ActionBroker
    {
        static IActorRef _heartbeat;
        static IActorRef _electionCycle;
        static IActorRef _candidate;
        static IActorRef _follower;
        static IActorRef _statusBroadcast;
        static IActorRef _data;

        public static void SetData(IActorRef data)
        {
            _data = data;
        }

        public static void SendData(string payload, string actorPath)
        {
            //_data.Tell(new SendData(payload, actorPath));
        }

        public static void SetStatusBroadcast(IActorRef statusBroadcast)
        {
            _statusBroadcast = statusBroadcast;
        }

        public static void SetHeartbeat(IActorRef heartbeat)
        {
            _heartbeat = heartbeat;
        }

        public static void SetElection(IActorRef electionCycle)
        {
            _electionCycle = electionCycle;
        }
        
        public static void SetCandidate(IActorRef candidate)
        {
            _candidate = candidate;
        }
        
        public static void SetFollower(IActorRef follower)
        {
            _follower = follower;
        }

        public static void StartHeartbeat()
        {
            _heartbeat?.Tell(new StartStopHeartbeat(true));
        }

        public static void StopHeartbeat()
        {
            _heartbeat?.Tell(new StartStopHeartbeat(false));
        }
        
        public static void StartElectionTimer()
        {
            _electionCycle?.Tell(new StartStopTime(true));
        }

        public static void StopElectionTimer()
        {
            _electionCycle?.Tell(new StartStopTime(false));
        }

        public static void ResetElectionTimer()
        {
            _electionCycle?.Tell(new TimeReset());
        }

        public static void AskForVote(int term)
        {
            _candidate?.Tell(new AskForVote(term));
        }

        public static void StartWaitForVote()
        {
            _candidate?.Tell(new StartWaitForVote(true));
        }

        public static void StopWaitForVote()
        {
            _candidate?.Tell(new StartWaitForVote(false));
        }
        public static void SendTerminateSignal()
        {
            _statusBroadcast.Tell(new SendTerminate());
        }
        public static void SendHeartbeatResponse(double heartbeatId, int term,string senderPath, int matchIndex,int? nextIndex)
        {
            _heartbeat.Tell(new SendHeartbeatResponse(heartbeatId, term, senderPath, matchIndex,(nextIndex ?? 0) + 1));
        }
        
        public static void Stop(TimeSpan timeout)
        {
            SendTerminateSignal();
            _electionCycle?.GracefulStop(timeout);
            _electionCycle = null;
            _heartbeat?.GracefulStop(timeout);
            _heartbeat = null;
            _candidate?.GracefulStop(timeout);
            _candidate = null;
            _follower?.GracefulStop(timeout);
            _follower = null;
        }
    }
}