using System;
using RampUp.Buffers;

namespace RampUp.Tests.Actors
{
    public unsafe class OneSegmentOnlyPool : ISegmentPool
    {
        private Segment* _segment;

        public OneSegmentOnlyPool(Segment* segment)
        {
            _segment = segment;
            SegmentLength = segment->Length;
        }

        public void Dispose()
        {
        }

        public bool TryPop(out Segment* result)
        {
            result = _segment;
            _segment = null;
            return result != null;
        }

        public int TryPop(int numberOfSegmentsToRetrieve, out Segment* startingSegment)
        {
            throw new System.NotImplementedException();
        }

        public void Push(Segment* segment)
        {
            if (_segment != null)
            {
                throw new InvalidOperationException();
            }

            _segment = segment;
        }

        public int SegmentLength { get; }
    }
}