using System;

namespace RampUp.Actors.Time.Impl
{
    public sealed class Timer : IDisposable
    {
        private readonly IBus _bus;
        private readonly System.Threading.Timer _timer;
        public const int ResolutionInMiliseconds = 1 << ResolutionInMilisecondsLog;
        public const int ResolutionInMilisecondsLog = 5;

        public Timer(IBus bus)
        {
            _bus = bus;
            _timer = new System.Threading.Timer(OnTick,null, TimeSpan.MaxValue, TimeSpan.FromMilliseconds(ResolutionInMiliseconds));
        }

        private void OnTick(object state)
        {
            var tickOccured = new TimerTickOccured();
            _bus.Publish(ref tickOccured);
        }

        public void Dispose()
        {
            _timer.Dispose();
        }
    }
}