using System;
using RampUp.Actors;
using RampUp.Actors.Impl;
using RampUp.Buffers;

namespace RampUp.Tests.Actors
{
    public sealed class GuidMessageWriter : IMessageWriter
    {
        public const int MessageId = 9873845;
        public const int ChunkLength = 16;

        public unsafe bool Write<TMessage>(ref Envelope envelope, ref TMessage message, WriteDelegate write)
            where TMessage : struct
        {
            if (typeof (TMessage) != typeof (Guid))
            {
                throw new ArgumentException("Guids only!");
            }

            var guid = (Guid) ((object) message);

            var ch1 = new ByteChunk((byte*) &guid, 8);
            var ch2 = new ByteChunk(((byte*) &guid) + 8, 8);
            return write(MessageId, ch1, ch2);
        }
    }
}