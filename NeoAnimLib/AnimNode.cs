
using System;
using System.Collections.Generic;
using System.Text;

namespace NeoAnimLib
{
    public class AnimNode
    {
        public AnimNode Parent { get; private set; }
        public string Name { get; set; }
        public float LocalWeight { get; set; } = 1f;
        public float Weight => (Parent?.Weight ?? 1) * LocalWeight;
        public float LocalSpeed { get; set; } = 1f;
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

        private readonly List<AnimNode> children = new List<AnimNode>();

        protected AnimNode() { }

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
            LocalStep(deltaTime * Speed);

            foreach (var child in children)
            {
                child.Step(deltaTime);
            }
        }

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

        public string PrintDebugTree()
        {
            var str = new StringBuilder(256);
            PrintDebugTree(str);
            return str.ToString();
        }

        public void PrintDebugTree(StringBuilder str)
        {
            str.Append(' ', Depth * 2);
            str.AppendLine(Name);

            foreach (var child in children)
                child.PrintDebugTree(str);
        }

        public override string ToString() => Name;
    }
}
