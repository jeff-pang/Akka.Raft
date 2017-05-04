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
        private ICancelable _timerTask;
        private int _electionTimeout = 10000;
        private int _timeElapsed = 0;
        private bool _isStarted = false;
        public ElectionCycleActor()
        {
            randomiseTimeout();
            Log.Information("{0}", $"Timeout is {_electionTimeout}");

            Receive<TimeElapse>(t => {
                _timeElapsed += 100;
                Console.Write($"({_timeElapsed/100})");
                if(_timeElapsed >= _electionTimeout)
                {
                    randomiseTimeout();
                    NodeEvents.OnElectionTimeout?.Invoke();
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
                        _timerTask = Context.System.Scheduler.ScheduleTellRepeatedlyCancelable(TimeSpan.FromMilliseconds(10),
                        TimeSpan.FromMilliseconds(100), Context.Self, new TimeElapse(), ActorRefs.NoSender);                        
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

            _electionTimeout = ((BitConverter.ToInt16(b, 0) % 10000) % 5000) + 5000;
        }

        protected override void PreStart()
        {
            Self.Tell(new StartStopTime(true));
        }
    }
}
