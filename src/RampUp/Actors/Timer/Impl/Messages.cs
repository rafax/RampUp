using System;

namespace RampUp.Actors.Timer.Impl
{
    public class Messages
    {
        public struct RegisterTimeout
        {
            public readonly long Id;
            public readonly TimeSpan Timeout;

            public RegisterTimeout(long id, TimeSpan timeout)
            {
                Id = id;
                Timeout = timeout;
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