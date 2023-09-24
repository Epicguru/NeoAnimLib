
namespace NeoAnimLib.Nodes
{
    /// <summary>
    /// Defines the behaviour that happens when a <see cref="TransitionNode"/> reaches the end of its transition.
    /// </summary>
    public enum TransitionEndBehaviour
    {
        /// <summary>
        /// The transition node is replaced with the <see cref="TransitionNode.ToNode"/> in its parent if <see cref="TransitionNode.Blend"/>
        /// has reached 100%. If <see cref="TransitionNode.Blend"/> is less than 100%, then this node is instead replaced with a snapshot
        /// of the current state of this transition node.
        /// </summary>
        ReplaceInParent,

        /// <summary>
        /// This transition node is removed from its parent by a call to <see cref="AnimNode.Remove(AnimNode)"/>.
        /// Keep in mind the possible behaviour if the parent node is another <see cref="TransitionNode"/>
        /// </summary>
        RemoveFromParent,

        /// <summary>
        /// Nothing happens when the end condition is met, but the end event is still raised.
        /// </summary>
        Nothing
    }
}
