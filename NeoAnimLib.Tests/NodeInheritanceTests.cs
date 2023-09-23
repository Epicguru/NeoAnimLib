namespace NeoAnimLib.Tests;

public class NodeInheritanceTests : TestBase
{
    private const float TOLERANCE = 0.0001f;

    [Theory]
    [InlineData(0)]
    [InlineData(0.1f)]
    [InlineData(0.5f)]
    [InlineData(1f)]
    [InlineData(2f)]
    public void TestWeightInheritance(float factor)
    {
        var root = new AnimNode("Root");
        var child1 = new AnimNode("Child1");
        var child2 = new AnimNode("Child2")
        {
            LocalWeight = 0.5f
        };

        // Ensure default weights are good.
        root.Weight.Should().BeApproximately(1, TOLERANCE);
        child1.Weight.Should().BeApproximately(1, TOLERANCE);
        child2.Weight.Should().BeApproximately(0.5f, TOLERANCE);

        root.Add(child1);
        root.Add(child2);

        // Ensure weights are being inherited.
        root.LocalWeight = factor;
        root.Weight.Should().BeApproximately(factor, TOLERANCE);
        child1.Weight.Should().BeApproximately(factor, TOLERANCE);
        child2.Weight.Should().BeApproximately(factor * 0.5f, TOLERANCE);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(0.1f)]
    [InlineData(0.5f)]
    [InlineData(1f)]
    [InlineData(2f)]
    public void TestSpeedInheritance(float factor)
    {
        var root = new AnimNode("Root");
        var child1 = new AnimNode("Child1");
        var child2 = new AnimNode("Child2")
        {
            LocalSpeed = 0.5f
        };

        // Ensure default speeds are good.
        root.Speed.Should().BeApproximately(1, TOLERANCE);
        child1.Speed.Should().BeApproximately(1, TOLERANCE);
        child2.Speed.Should().BeApproximately(0.5f, TOLERANCE);

        root.Add(child1);
        root.Add(child2);

        // Ensure speeds are being inherited.
        root.LocalSpeed = factor;
        root.Speed.Should().BeApproximately(factor, TOLERANCE);
        child1.Speed.Should().BeApproximately(factor, TOLERANCE);
        child2.Speed.Should().BeApproximately(factor * 0.5f, TOLERANCE);
    }
}
