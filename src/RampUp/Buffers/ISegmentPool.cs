using System;

namespace RampUp.Buffers
{
    public unsafe interface ISegmentPool : IDisposable
    {
        bool TryPop(out Segment* result);
        int TryPop(int numberOfSegmentsToRetrieve, out Segment* startingSegment);
        void Push(Segment* segment);

        /// <summary>
        /// Segment size, a power of 2, not lower than 4096
        /// </summary>
        int SegmentLength { get; }
    }
}