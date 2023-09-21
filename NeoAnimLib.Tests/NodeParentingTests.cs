
namespace NeoAnimLib.Tests;

public class NodeParentingTests
{
    [Fact]
    public void CheckAddAndRemove()
    {
        var root = new AnimNode("Root");
        root.AllChildren.Should().BeEmpty();

        var child = new AnimNode("Child");

        root.Add(child);

        // Should not allow adding twice:
        Assert.ThrowsAny<Exception>(() =>
        {
            root.Add(child);
        });

        // Should have the new child, and nothing else.
        root.DirectChildren.Count.Should().Be(1);
        root.DirectChildren[0].Should().Be(child);

        // Child should have parent assigned.
        child.Parent.Should().Be(root);

        // Attempt to remove from parent:
        root.Remove(child);

        // Should not allow removing twice:
        Assert.ThrowsAny<Exception>(() =>
        {
            root.Remove(child);
        });

        // Should now be empty:
        root.DirectChildren.Count.Should().Be(0);
        root.AllChildren.Should().BeEmpty();
    }

    [Fact]
    public void DepthCheck()
    {
        var root = new AnimNode("Root");
        var child = new AnimNode("Child");
        root.Add(child);

        root.Depth.Should().Be(0);
        child.Depth.Should().Be(1);

        root.Remove(child);
        child.Depth.Should().Be(0);
    }
}