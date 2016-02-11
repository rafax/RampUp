using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using RampUp.Buffers;

namespace RampUp.Actors.Timer.Impl
{
    /// <summary>
    /// This timer implementations uses hashed approach for a timing wheel with additional bucketing inside the hash bucket
    /// <see cref="http://www.cs.columbia.edu/~nahum/w6998/papers/ton97-timing-wheels.pdf"/>
    /// </summary>
    public unsafe class HashedBucketTimingWheel : IHandle<Messages.RegisterTimeout>
    {
        private readonly ISegmentPool _pool;
        private readonly Segment* _segment;
        private readonly Segment** _lowerTickSegments;
        private readonly int _bucketMask;
        private readonly int _entriesPerSegment;

        [StructLayout(LayoutKind.Explicit, Size = 16)]
        private struct Header
        {
            [FieldOffset(0)] public long Ticks;
            [FieldOffset(8)] public int Counter;
        }

        [StructLayout(LayoutKind.Explicit, Size = 16)]
        private struct Entry
        {
            [FieldOffset(0)] public readonly ActorId Actor;
            [FieldOffset(8)] public readonly long Id;

            public Entry(ActorId actor, long id)
            {
                Actor = actor;
                Id = id;
            }
        }

        public HashedBucketTimingWheel(ISegmentPool pool)
        {
            _pool = pool;
            _segment = pool.Pop();

            var bucketCount = _segment->Length/sizeof (Segment*);
            bucketCount = 1 << bucketCount.Log2();
            _bucketMask = bucketCount - 1;

            _lowerTickSegments = (Segment**) _segment->Buffer;
            _entriesPerSegment = (_segment->Length - sizeof (Header))/sizeof (Entry);
        }

        public void Handle(ref Envelope envelope, ref Messages.RegisterTimeout msg)
        {
            var ticks = RoundUp(msg.AbstoluteTicks);
            var index = ticks & _bucketMask;
            var head = _lowerTickSegments[index];

            var actorId = envelope.Sender;
            var id = msg.Id;

            if (head == null)
            {
                _lowerTickSegments[index] = AddInNewSegment(actorId, ticks, id);
            }
            else
            {
                // try first head as it will iterate with next
                if (TryAdd(head, ticks, actorId, id))
                {
                    return;
                }

                var current = head;
                while (current->Next != null)
                {
                    // try add
                    if (TryAdd(current, ticks, actorId, id))
                    {
                        return;
                    }
                    current = current->Next;
                }

                // nothing more to search, current contains last segment, add new one
                current->Next = AddInNewSegment(actorId, ticks, id);
            }
        }

        private bool TryAdd(Segment* current, int ticks, ActorId actorId, long id)
        {
            var header = GetHeader(current);
            if (header->Ticks == ticks && header->Counter < _entriesPerSegment)
            {
                GetEntries(current)[header->Counter] = new Entry(actorId, id);
                header->Counter += 1;
                return true;
            }
            return false;
        }

        private Segment* AddInNewSegment(ActorId actorId, int ticks, long id)
        {
            var segment = _pool.Pop();
            var header = GetHeader(segment);
            *header = new Header {Counter = 1, Ticks = ticks};
            var entries = GetEntries(segment);
            entries[0] = new Entry(actorId, id);
            return segment;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Header* GetHeader(Segment* segment)
        {
            return (Header*) segment->Buffer;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Entry* GetEntries(Segment* segment)
        {
            return (Entry*) (segment->Buffer + sizeof (Header));
        }

        private int RoundUp(long abstoluteTicks)
        {
            throw new NotImplementedException();
        }
    }
}