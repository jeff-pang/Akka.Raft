using Akka.Actor;
using AkkaRaft.Shared.Nodes;
using Serilog;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace AkkaRaft.Shared.Elections
{
    public class ElectionCycleActor:ReceiveActor
    {
        class TimeElapse { }

        private const int TIME_STEP_MS =50;
        private const int MIN_DURATION_MS = 5000;
        private const int MAX_DURATION_MS = 10000;
        private const int MS_PER_SEC = 1000;

        private ICancelable _timerTask;
        private int _electionDuration = MAX_DURATION_MS;
        private int _timeElapsed = 0;
        private bool _isStarted = false;
        private NodeEvents _nodeEvents;
        public int ElectionDuration { get => _electionDuration; set => _electionDuration = value; }

        public ElectionCycleActor(StateEvents stateEvents,NodeEvents nodeEvents)
        {
            _nodeEvents = nodeEvents;

            randomiseTimeout();
            Log.Information("{0}", $"Duration is {(float)_electionDuration/ MS_PER_SEC}");

            Receive<TimeElapse>(t => {

                _timeElapsed += TIME_STEP_MS;

                nodeEvents.OnElectionElapsed?.Invoke(_timeElapsed);

                Console.Write($"({(float)_timeElapsed/ MS_PER_SEC})");
                if(_timeElapsed >= _electionDuration)
                {
                    randomiseTimeout();
                    stateEvents.OnElectionTimeout?.Invoke();
                }
            });

            Receive<TimeReset>(t => {
                _timeElapsed = 0;
            });

            Receive<StartStopTime>(s => {
                if(s.Start)
                {
                    if (!_isStarted)
                    {
                        _isStarted = true;
                        _timerTask = Context.System.Scheduler.ScheduleTellRepeatedlyCancelable(TimeSpan.FromMilliseconds(20),
                        TimeSpan.FromMilliseconds(TIME_STEP_MS), Context.Self, new TimeElapse(), ActorRefs.NoSender);          
                    }
                }
                else
                {
                    if (_isStarted)
                    {
                        _isStarted = false;
                        _timerTask?.Cancel();
                    }
                }
            });
        }
        private void randomiseTimeout()
        {
            byte[] b = new byte[2];
            RandomNumberGenerator.Create().GetBytes(b);
            double rand = Math.Abs((double)BitConverter.ToInt16(b, 0)) / 100000;
            Log.Information("{0}",$"Election is now {_electionDuration}ms");
            _electionDuration = (int)(rand * (MAX_DURATION_MS - MIN_DURATION_MS) + MIN_DURATION_MS);
            _nodeEvents.OnElectionDurationChanged?.Invoke(_electionDuration);
        }

        protected override void PreStart()
        {
            Self.Tell(new StartStopTime(true));
        }
    }
}
