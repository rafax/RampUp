using System;
using System.Runtime.InteropServices;
using RampUp.Actors.Impl;
using RampUp.Buffers;
using RampUp.Ring;

namespace RampUp.Actors.Time.Impl
{
    /// <summary>
    /// The servicing actor, being registered with the actor requiring timeouts.
    /// </summary>
    /// <remarks>
    /// Based somehow on the timing wheels: <see cref="http://www.cs.columbia.edu/~nahum/w6998/papers/ton97-timing-wheels.pdf"/>
    /// </remarks>
    public unsafe class Scheduler : IScheduler, IHandle<TimerTickOccured>, IDisposable
    {
        private const int SizeOfSegment = 8 /*sizeof(Segment*)*/;
        private const int TimeoutsPerSegment = SingleThreadSegmentPool.SegmentSize / SizeOfSegment;
        private const int TimeoutsPerSegmentMask = TimeoutsPerSegment - 1;

        private readonly Segment* _segment;
        private readonly ISegmentPool _pool;
        private readonly IBus _bus;
        private readonly MessageHandler _handler;
        private readonly ITimeProvider _provider;
        private readonly Segment** _wheel;
        private readonly SegmentChainMessageStore _store;
        private Envelope _envelope;
        private long _lastIndex;
        private DateTime _lastDateTime;

        public Scheduler(ActorId thisActor, IBus bus, ISegmentPool pool, IMessageWriter writer, MessageHandler handler)
        {
            _envelope = new Envelope(thisActor);
            _bus = bus;
            _pool = pool;
            _handler = handler;
            _store = new SegmentChainMessageStore(writer, pool);
            var segment = _pool.Pop();
            _wheel = (Segment**)segment->Buffer;
            _segment = segment;

            // ensure clean wheel
            for (var i = 0; i < TimeoutsPerSegment; i++)
            {
                _wheel[i] = null;
            }
        }

        public void Schedule<TMessage>(TimeSpan timeout, ref TMessage message) where TMessage : struct
        {
            var milliseconds = (long)timeout.TotalMilliseconds;
            var segmentDiff = milliseconds >> Timer.ResolutionInMilisecondsLog;

            // if it's overlapping put max -1 value.
            if (segmentDiff > TimeoutsPerSegmentMask)
            {
                segmentDiff = TimeoutsPerSegmentMask;
            }

            var index = (_lastIndex + segmentDiff) & TimeoutsPerSegmentMask;
            _store.Write(ref _envelope, ref message, ref _wheel[index]);
        }

        public void Handle(ref Envelope envelope, ref TimerTickOccured msg)
        {
            var now = _provider.Now;

            // TODO
        }

        public void Dispose()
        {
            _pool.Push(_segment);
        }
    }
}