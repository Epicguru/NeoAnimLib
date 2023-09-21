using System;

namespace NeoAnimLib.Nodes
{
    /// <summary>
    /// Represents an animation node that does not support adding, inserting or removing
    /// child nodes.
    /// </summary>
    public abstract class LeafAnimNode : AnimNode
    {
        /// <summary>
        /// A node of this type does not support adding child nodes.
        /// This method will always throw an exception.
        /// </summary>
        /// <exception cref="NotImplementedException">This exception is always thrown.</exception>
        public sealed override void Add(AnimNode node)
            => throw new NotImplementedException($"{nameof(ClipAnimNode)} does not support adding child nodes.");

        /// <summary>
        /// A node of this type does not support removing child nodes.
        /// This method will always throw an exception.
        /// </summary>
        /// <exception cref="NotImplementedException">This exception is always thrown.</exception>
        public sealed override void Remove(AnimNode node)
            => throw new NotImplementedException($"{nameof(ClipAnimNode)} does not support removing child nodes.");

        /// <summary>
        /// A node of this type does not support inserting child nodes.
        /// This method will always throw an exception.
        /// </summary>
        /// <exception cref="NotImplementedException">This exception is always thrown.</exception>
        public sealed override void Insert(int index, AnimNode node)
            => throw new NotImplementedException($"{nameof(ClipAnimNode)} does not support inserting child nodes.");

        /// <inheritdoc/>
        protected LeafAnimNode() { }

        /// <inheritdoc/>
        protected LeafAnimNode(string name) : base(name) { }
    }
}
