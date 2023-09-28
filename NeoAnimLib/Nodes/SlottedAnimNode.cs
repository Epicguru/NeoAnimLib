using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace NeoAnimLib.Nodes
{
    /// <summary>
    /// A subclass of <see cref="MixAnimNode"/> where each direct child node is designated a slot,
    /// and subsequently sorted by that slot number.
    /// There can only be one node per slot number, so if a node is added using <see cref="Insert(int, AnimNode)"/>
    /// and there is an existing node with the same slot number, the original is removed.
    /// </summary>
    public class SlottedAnimNode : MixAnimNode, IComparer<AnimNode>
    {
        private readonly Dictionary<AnimNode, NodeMetadata> metadataMap = new Dictionary<AnimNode, NodeMetadata>();

        /// <inheritdoc/>
        public SlottedAnimNode()
        {

        }

        /// <inheritdoc/>
        public SlottedAnimNode(string name) : base(name)
        {

        }

        /// <summary>
        /// Tries to get a child <see cref="AnimNode"/> for a particular slot.
        /// </summary>
        /// <param name="slot"></param>
        /// <returns></returns>
        public AnimNode? GetNodeAt(int slot)
        {
            foreach (var c in DirectChildren)
            {
                if (metadataMap.TryGetValue(c, out var found) && found.Slot == slot)
                    return c;
            }
            return null;
        }

        /// <summary>
        /// Attempts to get the slot number associated with a child node.
        /// Returns true if the lookup was successful, false otherwise.
        /// </summary>
        public bool GetSlotOf(AnimNode node, out int slot)
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node));

            bool found = metadataMap.TryGetValue(node, out var meta);
            slot = found ? meta.Slot : 0;
            return found;
        }

        /// <summary>
        /// Attempts to get the slot number associated with a child node.
        /// If the specified node is not a direct child of this node, then <paramref name="defaultValue"/>
        /// is returned.
        /// </summary>
        public int GetSlotOf(AnimNode node, int defaultValue = -1)
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node));

            return metadataMap.TryGetValue(node, out var meta) ? meta.Slot : defaultValue;
        }

        /// <summary>
        /// This Add method is not supported on <see cref="SlottedAnimNode"/> and will always throw an exception.
        /// Use <see cref="Insert(int, AnimNode)"/> or <see cref="Replace(AnimNode, AnimNode?)"/> instead.
        /// </summary>
        /// <exception cref="InvalidOperationException">This exception is always called.</exception>
        public sealed override void Add(AnimNode _) => throw new InvalidOperationException($"{nameof(Add)} cannot be called on a {nameof(SlottedAnimNode)}. Use Insert instead.");

        /// <inheritdoc/>
        public override void Remove(AnimNode node)
        {
            base.Remove(node);
            metadataMap.Remove(node);
        }

        /// <summary>
        /// Inserts a new node into a particular slot.
        /// If there is already a node in that slot, that existing node is
        /// removed without performing the end behaviour.
        /// </summary>
        public override void Insert(int slot, AnimNode node)
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node));
            if (node.Parent != null)
                throw new InvalidOperationException($"The input node '{node}' already has a parent '{node.Parent}' so it cannot be inserted");

            // This should be caught by the Parent check above, so this is just a sanity check:
            Debug.Assert(!Children.Contains(node));

            // Remove existing:
            var existing = GetNodeAt(slot);
            if (existing != null)
                Remove(existing);
            
            // Insert new into the right place by sorting:
            Children.Add(node);
            metadataMap.Add(node, new NodeMetadata
            {
                Slot = slot
            });
            node.Parent = this;

            Children.Sort(this);
        }

        /// <inheritdoc/>
        public override void Replace(AnimNode existing, AnimNode? replacement)
        {
            if (existing == null)
                throw new ArgumentNullException(nameof(existing));

            if (!GetSlotOf(existing, out int slot))
                throw new InvalidOperationException($"Node {existing} is not a direct child so it cannot be replaced.");

            Remove(existing);

            if (replacement != null)
                Insert(slot, replacement);
        }

        private class NodeMetadata
        {
            public int Slot { get; set; }
        }

        /// <inheritdoc/>
        public int Compare(AnimNode? x, AnimNode? y)
        {
            if (x == null && y == null)
                return 0;
            if (x == null)
                return 1;
            if (y == null)
                return -1;

            int slotA = GetSlotOf(x);
            int slotB = GetSlotOf(y);
            return slotA - slotB;
        }
    }
}
