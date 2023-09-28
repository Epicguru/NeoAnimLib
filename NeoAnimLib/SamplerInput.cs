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

        /// <summary>
        /// A generic channel value. This could be used to output different values from your implementation of <see cref="IAnimClip"/>.
        /// Does nothing within the main library.
        /// </summary>
        public int Channel { get; set; }

        /// <summary>
        /// A generic user object, to be used as you see fit (such as to pass a value into <see cref="IAnimClip.Sample"/>).
        /// See also: <seealso cref="GetUserData{T}"/>.
        /// </summary>
        public object? UserData { get; set; }

        /// <summary>
        /// The <see cref="AnimNode"/> that this sample is being done for.
        /// May be null.
        /// </summary>
        public AnimNode? Node { get; set; }

        /// <summary>
        /// Gets the value of <see cref="UserData"/> cast to <typeparamref name="T"/>.
        /// This is equivalent to <c>(T)SamplerInput.UserObject</c>, so it will throw an exception if the type does not match.
        /// </summary>
        public readonly T GetUserData<T>() => (T)UserData!;

        /// <summary>
        /// Assumes that <see cref="Node"/> is a <see cref="ClipAnimNode{T}"/> and returns the value of
        /// <see cref="ClipAnimNode{T}.UserData"/>. If <see cref="Node"/> is null or not of the correct type,
        /// this will throw an exception.
        /// See <see cref="TryGetAnimNodeUserData{T}(out T)"/> for a safer version of this method.
        /// </summary>
        public readonly T GetAnimNodeUserData<T>() where T : struct => ((ClipAnimNode<T>)Node!).UserData;

        /// <summary>
        /// Assumes that <see cref="Node"/> is a <see cref="ClipAnimNode{T}"/> and outputs the value of
        /// <see cref="ClipAnimNode{T}.UserData"/>.
        /// Returns true if successful, false if <see cref="Node"/> is null or of the wrong type.
        /// See <see cref="GetAnimNodeUserData{T}"/> for a faster but unsafe version of this method.
        /// </summary>
        public readonly bool TryGetAnimNodeUserData<T>(out T userData) where T : struct
        {
            if (!(Node is ClipAnimNode<T> cast))
            {
                userData = default;
                return false;
            }

            userData = cast.UserData;
            return true;
        }
    }
}
