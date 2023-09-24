
using NeoAnimLib.Nodes;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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
        public AnimNode? Parent { get; internal set; }
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
        public IReadOnlyList<AnimNode> DirectChildren => Children;
        /// <summary>
        /// Gets an enumeration of all child nodes beneath this one, in a depth-first manner.
        /// </summary>
        public IEnumerable<AnimNode> AllChildren
        {
            get
            {
                foreach (var child in Children)
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
        /// <summary>
        /// The index of this node within its parent's direct children list.
        /// Will return -1 if <see cref="Parent"/> is null.
        /// </summary>
        public int IndexInParent => Parent == null ? -1 : Parent.Children.IndexOf(this);

        /// <summary>
        /// Internal list of children.
        /// Modify directly with care.
        /// </summary>
        protected readonly List<AnimNode> Children = new List<AnimNode>();

        private readonly List<AnimNode> tempChildren = new List<AnimNode>();

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
            if (Children.Contains(node))
                throw new Exception("Attempted to add node twice.");
            if (node.Parent != null)
                throw new Exception("Node '{node}' already has a parent.");

            Children.Add(node);
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
            if (Children.Contains(node))
                throw new Exception("Attempted to add node twice.");
            if (node.Parent != null)
                throw new Exception("Node '{node}' already has a parent.");

            if (index < 0 || index > Children.Count)
                throw new IndexOutOfRangeException($"Index {index} is out of the valid range (there are {Children.Count} items)");

            Children.Insert(index, node);
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
            if (!Children.Contains(node))
                throw new Exception("Attempted to remove a node that is not a direct child of this one.");

            Children.Remove(node);
            node.Parent = null;
        }

        /// <summary>
        /// Calls <see cref="Remove(AnimNode)"/> and then <see cref="Insert(int, AnimNode)"/> to
        /// replace a direct child node with another.
        /// If the replacement is null then this method functions the same way as <see cref="Remove(AnimNode)"/>.
        /// <paramref name="existing"/> must not be null and it must be a <b>direct</b> child of this node.
        /// </summary>
        /// <param name="existing"></param>
        /// <param name="replacement"></param>
        public virtual void Replace(AnimNode existing, AnimNode? replacement)
        {
            if (existing == null)
                throw new ArgumentNullException(nameof(existing));

            int index = existing.IndexInParent;
            Remove(existing);

            if (replacement != null)
                Insert(index, replacement);
        }

        /// <summary>
        /// Updates this node and all child nodes recursively
        /// by advancing forwards their state by <paramref name="deltaTime"/> seconds.
        /// </summary>
        public virtual void Step(float deltaTime)
        {
            if (deltaTime < 0f)
                throw new ArgumentOutOfRangeException(nameof(deltaTime), $"{nameof(deltaTime)} ({deltaTime}) should not be less than 0. To play an animation in reverse, change the {nameof(LocalSpeed)} property to a negative value.");

            LocalStep(deltaTime);

            /*
             * Can't just loop through children and step.
             * Scenarios that need to be handled:
             *  - When Step is called and it triggers the addition of a new child.
             *  - When Step is called and it triggers the removal of an existing child.
             */

            tempChildren.AddRange(Children);

            // Step all existing children, ensuring that we don't step any that were removed by the stepping of another.
            foreach (AnimNode? c in tempChildren.Where(c => Children.Contains(c)))
            {
                c.Step(deltaTime);
            }

            /*
             * I have decided not to step newly added children, as it seemed to cause more issues than benefits,
             * with nodes being stepped twice if they were already part of the graph as a descendent.
             */
            // Step all newly added children, if any:
            //foreach (AnimNode? c in Children.Where(c => !tempChildren.Contains(c)))
            //{
            //    c.Step(deltaTime);
            //}

            tempChildren.Clear();
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
            str.AppendLine($"[{GetType().Name}] {this}");

            foreach (var child in Children)
                child.PrintDebugTree(str);
        }

        /// <summary>
        /// 
        /// </summary>
        public void TransitionTo(AnimNode? other, float duration)
        {
            // TODO it is probably a good idea to allow other to be null
            // which would transition to a blank state i.e. a clip that returns no samples.

            if (other == null)
                throw new ArgumentNullException(nameof(other));
            if (duration <= 0f)
                throw new ArgumentOutOfRangeException(nameof(duration), duration.ToString(CultureInfo.InvariantCulture));

            if (Parent == null)
                throw new InvalidOperationException("Can only call TransitionTo if this node has a parent.");

            if (other.Parent != null)
                throw new InvalidOperationException("Other must have a null parent.");

            var transitionNode = new TransitionNode()
            {
                TransitionDuration = duration
            };
            Parent.Replace(this, transitionNode);
            transitionNode.FromNode = this;
            transitionNode.ToNode = other;
        }

        /// <inheritdoc/>
        public override string ToString() => Name ?? $"<no-name {GetType()}>";
    }
}
