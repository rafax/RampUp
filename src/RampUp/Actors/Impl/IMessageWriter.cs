using RampUp.Buffers;

namespace RampUp.Actors.Impl
{
    public interface IMessageWriter
    {
        /// <summary>
        /// Writes the <paramref name="message"/> with <paramref name="envelope"/> using <paramref name="write"/>.
        /// </summary>
        /// <returns>If the write was successful, passing over just <see cref="WriteDelegate"/>.</returns>
        /// <remarks>
        /// The writer transforms <paramref name="envelope"/> into <see cref="ByteChunk"/> as well as the <paramref name="message"/>.
        /// Then calles the <see cref="WriteDelegate"/>.
        /// </remarks>
        bool Write<TMessage>(ref Envelope envelope, ref TMessage message, WriteDelegate write)
            where TMessage : struct;
    }

    /// <summary>
    /// The write delegate's contract is to write the <paramref name="chunk"/> concatenated with <paramref name="chunk2"/> so that <see cref="MessageReader.MessageHandlerImpl"/>
    /// will obtain concatenated chunks as one.
    /// </summary>
    public delegate bool WriteDelegate(int messageTypeId, ByteChunk chunk, ByteChunk chunk2);
}