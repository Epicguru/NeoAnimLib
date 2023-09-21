namespace NeoAnimLib.Nodes
{
    /// <summary>
    /// Defines the possible behaviour that occurs when the end condition of a <see cref="ClipAnimNode"/>
    /// have been met, or the <see cref="ClipAnimNode.Stop"/> method is called.
    /// </summary>
    public enum ClipEndBehaviour
    {
        /// <summary>
        /// When the clip end condition is met, it is removed from its parent.
        /// </summary>
        RemoveFromParent,

        /// <summary>
        /// When the clip end condition is met, nothing happens.
        /// </summary>
        Nothing,
    }
}
