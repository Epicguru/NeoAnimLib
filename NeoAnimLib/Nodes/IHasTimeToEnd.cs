namespace NeoAnimLib.Nodes
{
    /// <summary>
    /// An interface that exposes the <see cref="GetTimeToEnd"/> method.
    /// For use on <see cref="AnimNode"/> classes.
    /// </summary>
    public interface IHasTimeToEnd
    {
        /// <summary>
        /// Returns the time, in seconds, that this <see cref="AnimNode"/> is expected to last for.
        /// For example, on a <see cref="ClipAnimNode"/>, this would be derived from either <see cref="ClipAnimNode.Duration"/> or <see cref="ClipAnimNode.TargetLoopCount"/>.
        /// It will return null if the time-to-end is not known or cannot be calculated.
        /// Will not return a value less than 0.
        /// </summary>
        float? GetTimeToEnd();
    }
}
