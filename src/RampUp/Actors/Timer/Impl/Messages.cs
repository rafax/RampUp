using System;
using System.Diagnostics;

namespace RampUp.Actors.Timer.Impl
{
    public class Messages
    {
        public struct RegisterTimeout
        {
            public readonly long Id;
            public readonly long AbstoluteTicks;

            public RegisterTimeout(long id, TimeSpan timeout)
            {
                Id = id;
                AbstoluteTicks = timeout.Ticks + Stopwatch.GetTimestamp();
            }
        }

        public struct TimeoutOccured
        {
            public readonly long Id;

            public TimeoutOccured(long id)
            {
                Id = id;
            }
        }
    }
}