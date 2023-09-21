namespace NeoAnimLib
{
    /// <summary>
    /// Defines how a value will be set when interpolating between two states
    /// where one state has a property but the other does not.
    /// </summary>
    public enum MissingPropertyBehaviour
    {
        /// <summary>
        /// The final value is the interpolation between the known value and the default value for that property,
        /// as provided by a <see cref="DefaultValueSource"/>.
        /// </summary>
        UseDefaultValue,

        /// <summary>
        /// If either state is missing the property, the output will simply
        /// be the value of the state that does have the property, regardless of weight or the value of <code>t</code>.
        /// </summary>
        UseKnownValue
    }
}
