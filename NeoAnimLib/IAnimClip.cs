using System.Collections.Generic;

namespace NeoAnimLib
{
    /// <summary>
    /// An interface that an animation clip should implement.
    /// This interface can be used to sample an animation.
    /// </summary>
    public interface IAnimClip
    {
        /// <summary>
        /// The name of this animation clip.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// The total length, in seconds, of this animation clip.
        /// </summary>
        float Length { get; }

        /// <summary>
        /// Samples this animation clip at a particular time, and writes the properties to the <paramref name="sample"/>.
        /// </summary>
        /// <param name="sample">The sample that is written to.</param>
        /// <param name="time">The time, in seconds, that the clip should be sampled at. May be less than 0 or more than <see cref="Length"/>.</param>
        void Sample(AnimSample sample, float time);

        /// <summary>
        /// Should get an enumeration of events that are at the between the <paramref name="startTime"/> (inclusive) and <paramref name="endTime"/> (exclusive).
        /// <paramref name="startTime"/> is always less than or equal to <paramref name="endTime"/>.
        /// The order of the returned events is the order in which they will be invoked.
        /// </summary>
        IEnumerable<AnimEvent> GetEventsInRange(float startTime, float endTime);

        // TODO events etc.
    }
}
