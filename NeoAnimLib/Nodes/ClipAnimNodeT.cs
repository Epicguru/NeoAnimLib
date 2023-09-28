namespace NeoAnimLib.Nodes
{
    /// <summary>
    /// A subclass of <see cref="ClipAnimNode"/> that adds an additional field <see cref="UserData"/>
    /// that can contain custom user data for this node.
    /// </summary>
    public class ClipAnimNode<T> : ClipAnimNode where T : struct
    {
        /// <summary>
        /// Custom user data, of type <typeparamref name="T" />, for this node.
        /// </summary>
        public T UserData;

        /// <inheritdoc/>
        public ClipAnimNode(IAnimClip clip) : base(clip)
        {

        }
    }
}
