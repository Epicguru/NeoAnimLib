
namespace NeoAnimLib.Nodes
{
    /// <summary>
    /// Defines the behaviour that happens when a <see cref="TransitionNode"/> reaches the end of its transition.
    /// </summary>
    public enum TransitionEndBehaviour
    {
        /// <summary>
        /// The transition node is replaced with the <see cref="TransitionNode.ToNode"/> in its parent.
        /// </summary>
        ReplaceWithToNode,

        /// <summary>
        /// Nothing happens when the end condition is met, but the end event is still raised.
        /// </summary>
        Nothing
    }
}
