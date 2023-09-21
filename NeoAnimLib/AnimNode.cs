
using System;
using System.Collections.Generic;
using System.Text;

namespace NeoAnimLib
{
    /// <summary>
    /// The base class for all nodes in an animation graph.
    /// </summary>
    public class AnimNode
    {
        /// <summary>
        /// The parent node. May be null (such as when this node is the root node).
        /// </summary>
        public AnimNode? Parent { get; private set; }
        /// <summary>
        /// The optional name of this node. Can be used for debugging.
        /// May be null.
        /// </summary>
        public string? Name { get; set; }
        /// <summary>
        /// The local weight of this node.
        /// The way it is interpreted depends on the <see cref="Parent"/> node type.
        /// Normally it is expected to be in the [0, 1] range.
        /// The default value is 1.
        /// </summary>
        public float LocalWeight { get; set; } = 1f;
        /// <summary>
        /// The absolute weight of this node, calculated by taking the <see cref="Parent"/>'s <see cref="Weight"/>
        /// and multiplying it by the <see cref="LocalWeight"/>.
        /// </summary>
        public float Weight => (Parent?.Weight ?? 1) * LocalWeight;
        /// <summary>
        /// The local speed of this node.
        /// <see cref="Speed"/> is used as a multiplier to the deltaTime that is passed in to <see cref="Step(float)"/>.
        /// </summary>
        public float LocalSpeed { get; set; } = 1f;
        /// <summary>
        /// The absolute speed of this node.
        /// This value is used as a multiplier to the deltaTime that is passed in to <see cref="Step(float)"/>.
        /// </summary>
        public float Speed => (Parent?.Speed ?? 1) * LocalSpeed;
        /// <summary>
        /// Gets a read-only list of the direct child nodes of this node: it does not include the children of those children.
        /// </summary>
        public IReadOnlyList<AnimNode> DirectChildren => children;
        /// <summary>
        /// Gets an enumeration of all child nodes beneath this one, in a depth-first manner.
        /// </summary>
        public IEnumerable<AnimNode> AllChildren
        {
            get
            {
                foreach (var child in children)
                {
                    yield return child;
                    foreach (var sub in child.AllChildren)
                        yield return sub;
                }
            }
        }
        /// <summary>
        /// The 'depth' of this node i.e. how many parents it has above it.
        /// </summary> 
        public int Depth => (Parent?.Depth ?? -1) + 1;
        /// <summary>
        /// The current time, in seconds, that this node is currently at.
        /// </summary>
        public float LocalTime { get; protected set; }
        /// <summary>
        /// This is true when any end condition has been met.
        /// </summary>
        public bool IsEnded { get; protected set; }

        private readonly List<AnimNode> children = new List<AnimNode>();

        /// <summary>
        /// Default constructor. Does nothing.
        /// </summary>
        protected AnimNode() { }

        /// <summary>
        /// Creates a new node and assigns the <see cref="Name"/>.
        /// </summary>
        /// <param name="name"></param>
        public AnimNode(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Adds a new node as a direct child of this one.
        /// Will throw an exception if the node is null or already a child of this one, or already has a parent.
        /// </summary>
        public virtual void Add(AnimNode node)
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node));
            if (children.Contains(node))
                throw new Exception("Attempted to add node twice.");
            if (node.Parent != null)
                throw new Exception("Node '{node}' already has a parent.");

            children.Add(node);
            node.Parent = this;
        }

        /// <summary>
        /// Inserts a new node into the children list of this one, at a specific index.
        /// Will throw exceptions if the index is invalid or the operation cannot be performed for any
        /// other reason.
        /// </summary>
        public virtual void Insert(int index, AnimNode node)
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node));
            if (children.Contains(node))
                throw new Exception("Attempted to add node twice.");
            if (node.Parent != null)
                throw new Exception("Node '{node}' already has a parent.");

            if (index < 0 || index >= children.Count)
                throw new IndexOutOfRangeException($"Index {index} is out of the valid range (there are {children.Count} items)");

            children.Insert(index, node);
            node.Parent = this;
        }

        /// <summary>
        /// Removes a child node. The node must be a direct child of this one.
        /// Will throw an exception if the node is null or not a direct child.
        /// </summary>
        public virtual void Remove(AnimNode node)
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node));
            if (!children.Contains(node))
                throw new Exception("Attempted to remove a node that is not a direct child of this one.");

            children.Remove(node);
            node.Parent = null;
        }

        /// <summary>
        /// Updates this node and all child nodes recursively
        /// by advancing forwards their state by <paramref name="deltaTime"/> seconds.
        /// </summary>
        public virtual void Step(float deltaTime)
        {
            LocalStep(deltaTime);

            // Do not step children if removed:
            if (IsEnded)
                return;

            for (int i = 0; i < children.Count; i++)
            {
                var child = children[i];

                child.Step(deltaTime);

                // Account for children being removed during the Step call:
                if (i >= children.Count || children[i] != child)
                {
                    i--;
                }
            }
        }

        /// <summary>
        /// Performs any stepping necessary, such as advancing the animation or raising events.
        /// Default implementation in <see cref="AnimNode"/> does nothing.
        /// </summary>
        /// <param name="deltaTime">The time, in seconds, to advance by.</param>
        protected virtual void LocalStep(float deltaTime)
        {

        }

        /// <summary>
        /// Samples this node at the <see cref="LocalTime"/>
        /// using the <see cref="SamplerInput"/> provided.
        /// Returns a new <see cref="AnimSample"/> which will need to be disposed of when no longer in use.
        /// </summary>
        /// <param name="input">Sampler settings that determine how the output sample is composed.</param>
        /// <returns>A new instance of <see cref="AnimSample"/>.</returns>
        /// <exception cref="NotImplementedException">If the particular node type it is called on does not support sampling.</exception>
        public virtual AnimSample Sample(in SamplerInput input) => throw new NotImplementedException($"A node of type {GetType()} cannot be sampled.");

        /// <summary>
        /// Makes a string that contains a debug view of this node and all children nodes
        /// for debugging purposes.
        /// </summary>
        public string PrintDebugTree()
        {
            var str = new StringBuilder(256);
            PrintDebugTree(str);
            return str.ToString();
        }

        /// <summary>
        /// Populated a <see cref="StringBuilder"/> with a string that contains a debug view of this node and all children nodes
        /// for debugging purposes.
        /// </summary>
        public void PrintDebugTree(StringBuilder str)
        {
            str.Append(' ', Depth * 2);
            str.AppendLine(Name);

            foreach (var child in children)
                child.PrintDebugTree(str);
        }

        /// <inheritdoc/>
        public override string ToString() => Name ?? "";
    }
}
