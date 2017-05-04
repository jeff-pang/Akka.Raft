using Akka.Actor;
using Akka.Cluster;
using Akka.Cluster.Tools.PublishSubscribe;
using AkkaRaft.Shared.Nodes;
using Serilog;
using System;
using System.Collections.Generic;
using System.Text;

namespace AkkaRaft.Shared.Candidates
{
    public class CandidateActor:ReceiveActor
    {
        public class WaitTimeout { }
        
        private bool _timeStarted;
        private ICancelable _timerTask;
        public CandidateActor()
        {
            var mediator = DistributedPubSub.Get(Context.System).Mediator;

            Receive<AskForVote>(a=>{
                Log.Information("{0}","Asks for votes");
                mediator.Tell(new Publish("voterequest", new VoteRequest(a.Term, Node.ClusterUid)));
            });

            Receive<Vote>(v => {
                Log.Information("{0}", "Reset Wait timeout");
                //reset
                stopWait();
                startWait();
                NodeEvents.OnGotVote?.Invoke(v.SenderId, v.Term);
                
            });

            Receive<WaitTimeout>(v =>
            {
                if (_timeStarted)
                {
                    Log.Information("{0}", "Wait timeout");
                    NodeEvents.OnWaitForVoteTimeout?.Invoke();
                }
            });

            Receive<StartWaitForVote>(w => {
                if(w.Start)
                {
                    Log.Information("{0}", "Waiting for vote");
                    startWait();
                }
                else
                {
                    Log.Information("{0}", "Stopped waiting for vote");
                    stopWait();
                }
            });

        }

        private void startWait()
        {
            if (!_timeStarted)
            {
                _timeStarted = true;
                _timerTask = Context.System.Scheduler.ScheduleTellOnceCancelable(TimeSpan.FromSeconds(3),
                    Context.Self, new WaitTimeout(), ActorRefs.NoSender);
            }
        }

        private void stopWait()
        {
            if (_timeStarted)
            {
                _timeStarted = false;
                _timerTask?.Cancel();
            }
        }
    }
}