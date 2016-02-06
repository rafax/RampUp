using System;
using RampUp.Actors.Impl;
using RampUp.Buffers;
using RampUp.Ring;

namespace RampUp.Actors.Timer.Impl
{
    /// <summary>
    /// The servicing actor, being registered with the actor requiring timeouts.
    /// </summary>
    public class Scheduler : IScheduler, IHandle<Messages.TimeoutOccured>
    {
        private const int FirstChunkOffset = sizeof (int)*4;
        private readonly IBus _bus;
        private readonly ISegmentPool _pool;
        private readonly IMessageWriter _writer;
        private readonly MessageHandler _handler;
        private unsafe Segment* _lastWritten;

        public Scheduler(IBus bus, ISegmentPool pool, IMessageWriter writer, MessageHandler handler)
        {
            _bus = bus;
            _pool = pool;
            _writer = writer;
            _handler = handler;
        }

        public unsafe void Schedule<TMessage>(TimeSpan timeout, ref TMessage message) where TMessage : struct
        {
            var empty = new Envelope();
            if (_writer.Write(ref empty, ref message, Write) == false)
            {
                throw new InvalidOperationException("Not enough memory in the pool");
            }

            var registerTimeout = new Messages.RegisterTimeout((long) _lastWritten, timeout);
            _bus.Publish(ref registerTimeout);
        }

        private unsafe bool Write(int messagetypeid, ByteChunk chunk, ByteChunk chunk2)
        {
            Segment* segment;
            if (_pool.TryPop(out segment) == false)
            {
                return false;
            }

            var bytes = segment->Buffer;
            *(int*) bytes = messagetypeid;
            *((int*) bytes + 1) = chunk.Length;
            *((int*) bytes + 2) = chunk2.Length;

            Native.MemcpyUnmanaged(bytes + FirstChunkOffset, chunk.Pointer, chunk.Length);
            Native.MemcpyUnmanaged(bytes + FirstChunkOffset + chunk.Length, chunk2.Pointer, chunk2.Length);

            _lastWritten = segment;
            return true;
        }

        public unsafe void Handle(ref Envelope envelope, ref Messages.TimeoutOccured msg)
        {
            var segment = (Segment*) msg.Id;

            var bytes = segment->Buffer;
            var messagetypeid = *(int*) bytes;
            var chunkLength = *((int*) bytes + 1);
            var chunk2Length = *((int*) bytes + 2);

            var chunk = new ByteChunk(bytes + FirstChunkOffset, chunkLength + chunk2Length);
            _handler(messagetypeid, chunk);
            _pool.Push(segment);
        }
    }
}