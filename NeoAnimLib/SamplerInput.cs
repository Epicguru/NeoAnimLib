using NeoAnimLib.Nodes;

namespace NeoAnimLib
{
    /// <summary>
    /// A collection of options that can be used to customize the output
    /// returned from <see cref="AnimNode.Sample"/>.
    /// </summary>
    public struct SamplerInput
    {
        /// <summary>
        /// Provides a source for property values if they are missing.
        /// Only required when using <see cref="MissingPropertyBehaviour.UseDefaultValue"/>.
        /// </summary>
        public DefaultValueSource? DefaultValueSource { get; set; }

        /// <summary>
        /// Defines how property values are obtained if/when properties are present in one sample but not in another.
        /// Only used in certain nodes such as <see cref="MixAnimNode"/>.
        /// </summary>
        public MissingPropertyBehaviour MissingPropertyBehaviour { get; set; }
    }
}
