using NeoAnimLib.Nodes;
using System.Numerics;
using NeoAnimLib.Tests.Implementations;

namespace NeoAnimLib.Tests;

public class TimeTests
{
    [Theory]
    [InlineData(1, 1, 1)]
    [InlineData(1, 2, 2)]
    [InlineData(1, 0.1f, 0.1f)]
    [InlineData(0.123, 1f, 0.123f)]
    [InlineData(0.123, 0.5f, 0.123f * 0.5f)]
    [InlineData(100, 0, 0)]
    [InlineData(100, 0.01f, 1f)]
    [InlineData(100, -1, -100)]
    [InlineData(100, -2, -200)]
    public void TestSingleClipStep(float delta, float speed, float expected)
    {
        var clip = new TestClipNode("TestClip");
        clip.LocalTime.Should().Be(0);

        clip.LocalSpeed = speed;
        clip.Step(delta);

        clip.LocalTime.Should().Be(expected);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(10)]
    [InlineData(20)]
    [InlineData(50)]
    public void TestNestedStepSpeed(int depth)
    {
        var rand = new Random();
        AnimNode? prev = null;

        float expectedSpeed = 1f;

        for (int i = 0; i < depth; i++)
        {
            bool last = i == depth - 1;

            AnimNode node = last ? new TestClipNode("TestClip") : new MixAnimNode($"TestMixer_{i}");

            float speed = RandomInRange(rand, 0.01f, 5f) * rand.NextDouble() > 0.5 ? 1 : -1;
            expectedSpeed *= speed;
            node.LocalSpeed = speed;

            prev?.Add(node);
            prev = node;

            node.Speed.Should().Be(expectedSpeed);
        }
    }

    private static T RandomInRange<T>(Random rand, T min, T max) where T : IFloatingPoint<T>
    {
        T range = max - min;
        T t = T.CreateChecked(rand.NextDouble());
        return min + range * t;
    }
}
