using System;

namespace RampUp.Actors.Time
{
    public interface ITimeProvider
    {
        /// <summary>
        /// Gets now.
        /// </summary>
        DateTime Now { get; }
    }
}