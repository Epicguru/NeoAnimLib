using NeoAnimLib.Nodes;
using System.Numerics;
using NeoAnimLib.Tests.Implementations;

namespace NeoAnimLib.Tests;

public class TimeTests : TestBase
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

    [Fact]
    public void TestClipEvents()
    {
        const float LENGTH = 1f;
        // ReSharper disable once RedundantArgumentDefaultValue
        var clip = new TestClipNode("TestClip", LENGTH);

        int loopEventCount = 0;
        // ReSharper disable once AccessToModifiedClosure
        clip.OnLoop += _ => loopEventCount++;

        // Do first step, and also check that the OnStartPlay and OnPlaying are raised.
        using (var monitor = clip.Monitor())
        {
            // Moving forwards by length should loop once.
            clip.Step(LENGTH);

            monitor.Should().Raise(nameof(ClipAnimNode.OnStartPlay), "StartPlay is expected to be raised once step is called");
            monitor.Should().Raise(nameof(ClipAnimNode.OnPlaying), "OnPlaying is expected to be raised once step is called");
        }

        // Verify.
        clip.LocalTime.Should().Be(LENGTH);
        loopEventCount.Should().Be(1);
        clip.LoopCount.Should().Be(1);

        // Reset.
        clip.Reset();
        loopEventCount = 0;

        // Moving forwards by less than length should not cause a loop.
        clip.Step(LENGTH - 0.01f);

        // Verify.
        clip.LocalTime.Should().Be(LENGTH - 0.01f);
        loopEventCount.Should().Be(0);
        clip.LoopCount.Should().Be(0);

        // Reset.
        clip.Reset();
        loopEventCount = 0;

        void Move(float time, int expectedLoops)
        {
            // Moving forwards by less than length should not cause a loop.
            clip.LocalSpeed = time < 0f ? -1f : 1f;
            clip.Step(MathF.Abs(time));

            // Verify.
            clip.LocalTime.Should().Be(time);
            loopEventCount.Should().Be(expectedLoops);
            clip.LoopCount.Should().Be(expectedLoops);

            // Reset.
            clip.Reset();
            loopEventCount = 0;
        }

        Move(0, 0);
        Move(LENGTH * 2.5f, 2);
        Move(LENGTH * 123.45f, 123);

        Move(LENGTH *  0.5f, 0);
        Move(LENGTH * -0.5f, 0);

        Move(LENGTH * -1.0f, 1);
        Move(LENGTH * -2.0f, 2);
        Move(LENGTH * -2.5f, 2);
        Move(LENGTH * -3.01f, 3);
    }

    private static T RandomInRange<T>(Random rand, T min, T max) where T : IFloatingPoint<T>
    {
        T range = max - min;
        T t = T.CreateChecked(rand.NextDouble());
        return min + range * t;
    }
}
