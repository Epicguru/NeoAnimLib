using NeoAnimLib.Nodes;

namespace NeoAnimLib.Tests;

public class SlotsTest : TestBase
{
    [Fact]
    public void TestSlotSorting()
    {
        var slots = new SlottedAnimNode();

        // Generate random pairs:
        const int MIN = -100;
        const int MAX =  100;
        const int TO_ADD = 1000;
        const int TO_REMOVE = 100;
        const int TO_REPLACE = 100;
        const int SEED = 12345;

        var rand = new Random(SEED);
        var list = new List<NodeOrderPair>(TO_ADD);
        for (int i = 0; i < TO_ADD; i++)
        {
            int slot = MIN + (int)((MAX - MIN) * rand.NextDouble());
            list.Add(new NodeOrderPair
            {
                Node = new AnimNode($"Node in slot {slot}"),
                Slot = slot
            });
        }

        void CheckOrder()
        {
            var knownSlots = new HashSet<int>();
            int prevSlot = int.MinValue;
            foreach (var child in slots.DirectChildren)
            {
                slots.GetSlotOf(child, out int slot).Should().BeTrue();
                knownSlots.Add(slot).Should().BeTrue();

                slot.Should().BeGreaterThan(prevSlot);
                prevSlot = slot;
            }
        }

        // Insert:
        foreach (var item in list)
        {
            slots.Insert(item.Slot, item.Node);
        }
        CheckOrder();

        // Sanity check
        list.Sort();
        for (int i = 1; i < list.Count; i++)
        {
            int prev = list[i - 1].Slot;
            int curr = list[i].Slot;
            prev.Should().BeLessThanOrEqualTo(curr);
        }

        // Removal:
        for (int i = 0; i < TO_REMOVE; i++)
        {
            var toRemove = slots.DirectChildren[rand.Next(0, slots.DirectChildren.Count)];
            int oldCount = slots.DirectChildren.Count;
            int toRemoveSlot = slots.GetSlotOf(toRemove);

            slots.Remove(toRemove);

            toRemove.Parent.Should().BeNull();
            slots.GetNodeAt(toRemoveSlot).Should().BeNull();
            slots.AllChildren.Should().NotContain(toRemove);
            slots.DirectChildren.Count.Should().Be(oldCount - 1);
        }
        CheckOrder();

        // Replacement.
        for (int i = 0; i < TO_REPLACE; i++)
        {
            var toReplace = slots.DirectChildren[rand.Next(0, slots.DirectChildren.Count)];
            int oldCount = slots.DirectChildren.Count;
            int toReplaceSlot = slots.GetSlotOf(toReplace);

            var replacement = new AnimNode("Replacement");

            slots.Replace(toReplace, replacement);

            toReplace.Parent.Should().BeNull();
            slots.GetNodeAt(toReplaceSlot).Should().Be(replacement);
            slots.AllChildren.Should().NotContain(toReplace);
            slots.AllChildren.Should().Contain(replacement);
            slots.DirectChildren.Count.Should().Be(oldCount);
        }
        CheckOrder();
    }

    private readonly struct NodeOrderPair : IComparable<NodeOrderPair>
    {
        public required AnimNode Node { get; init; }
        public required int Slot { get; init; }

        public int CompareTo(NodeOrderPair other)
            => Slot.CompareTo(other.Slot);
    }
}
