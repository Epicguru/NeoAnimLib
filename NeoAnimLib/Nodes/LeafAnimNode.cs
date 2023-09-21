using System;

namespace NeoAnimLib.Nodes
{
    /// <summary>
    /// Represents an animation node that does not support adding, inserting or removing
    /// child nodes.
    /// </summary>
    public abstract class LeafAnimNode : AnimNode
    {
        public sealed override void Add(AnimNode node)
            => throw new NotImplementedException($"{nameof(ClipAnimNode)} does not support adding child nodes.");

        public sealed override void Remove(AnimNode node)
            => throw new NotImplementedException($"{nameof(ClipAnimNode)} does not support removing child nodes.");

        public sealed override void Insert(int index, AnimNode node)
            => throw new NotImplementedException($"{nameof(ClipAnimNode)} does not support inserting child nodes.");

        protected LeafAnimNode() { }

        protected LeafAnimNode(string name) : base(name) { }
    }
}
