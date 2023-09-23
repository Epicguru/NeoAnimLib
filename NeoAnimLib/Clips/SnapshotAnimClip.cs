using System;
using System.Collections.Generic;

namespace NeoAnimLib.Clips
{
    /// <summary>
    /// An implementation of <see cref="IAnimClip"/> that is a static collection of
    /// <see cref="AnimPropertySample"/>s, meaning that it will always return the exact same properties
    /// when sampled regardless of time.
    /// The <see cref="Length"/> is always 0 and it will never have any events.
    /// </summary>
    public class SnapshotAnimClip : IAnimClip
    {
        private readonly List<AnimPropertySample> samples;

        /// <inheritdoc/>
        public string Name { get; }

        /// <summary>
        /// The length of this <see cref="SnapshotAnimClip"/>: it is always 0.
        /// </summary>
        public float Length => 0;

        /// <summary>
        /// Creates a new snapshot clip from an enumeration of samples.
        /// The samples enumeration must not be null.
        /// </summary>
        public SnapshotAnimClip(IEnumerable<AnimPropertySample> samples, string name = "")
        {
            if (samples == null)
                throw new ArgumentNullException(nameof(samples));

            Name = name;
            this.samples = new List<AnimPropertySample>();
            this.samples.AddRange(samples);
        }

        /// <summary>
        /// Creates a new snapshot clip based on a sample object. The sample must not be null.
        /// This copies all the <see cref="AnimPropertySample"/> from <see cref="AnimSample.Samples"/>.
        /// </summary>
        public SnapshotAnimClip(AnimSample sample, string name = "")
        {
            Name = name;
            samples = new List<AnimPropertySample>(sample.Samples);
        }

        /// <inheritdoc/>
        public void Sample(AnimSample sample, float time)
        {
            foreach (var s in samples)
                sample.SetProperty(s);
        }

        /// <inheritdoc/>
        public IEnumerable<AnimEvent> GetEventsInRange(float startTime, float endTime)
        {
            // Snapshots don't have events.
            yield break;
        }
    }
}
