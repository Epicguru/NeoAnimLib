using System;

namespace NeoAnimLib
{
    /// <summary>
    /// A path-value pair.
    /// The <see cref="Path"/> is a string that points to a particular animated property, and
    /// the <see cref="Value"/> is the actual value of that property.
    /// </summary>
    public readonly struct AnimPropertySample
    {
        /// <summary>
        /// Linearly interpolates between two samples, creating a new sample
        /// that has the <see cref="Path"/> from <paramref name="a"/>, and the <see cref="Value"/>
        /// that is <paramref name="t"/>% between <paramref name="a"/>.Value and <paramref name="b"/>.Value.
        /// <paramref name="t"/> is in the range [0, 1] for [a.Value and b.Value], but may be lower of higher.
        /// </summary>
        public static AnimPropertySample Lerp(in AnimPropertySample a, in AnimPropertySample b, float t)
            =>  new AnimPropertySample(a.Path, a.Value + (b.Value - a.Value) * t);

        /// <summary>
        /// The full path of this animated property.
        /// Serves as the ID of this property.
        /// </summary>
        public string Path { get; }

        /// <summary>
        /// The value of the property at this point in time.
        /// </summary>
        public float Value { get; }

        /// <summary>
        /// Constructs a new <see cref="AnimPropertySample"/>
        /// provided a path and a value. The path cannot be null.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="value"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public AnimPropertySample(string path, float value)
        {
            Path = path ?? throw new ArgumentNullException(nameof(path));
            Value = value;
        }
    }
}
