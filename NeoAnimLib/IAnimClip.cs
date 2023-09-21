namespace NeoAnimLib
{
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

        // TODO events etc.
    }
}
