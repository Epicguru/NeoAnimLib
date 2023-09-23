using NeoAnimLib.Nodes;
using NeoAnimLib.Tests.Implementations;

namespace NeoAnimLib.Tests;

public class EndTests : TestBase
{
    [Fact]
    public void EndNotRaisedWhenNoConditionMet()
    {
        // ReSharper disable once RedundantArgumentDefaultValue
        var clip = new TestClipNode("TestClip", 1f);

        using var monitor = clip.Monitor();
        var rand = Random.Shared;

        for (int i = 0; i < 100; i++)
        {
            clip.Step((float)rand.NextDouble() + 0.5f);
        }

        monitor.Should().NotRaise(nameof(ClipAnimNode.OnEndPlay));
        monitor.Should().Raise(nameof(ClipAnimNode.OnStartPlay));
        monitor.Should().Raise(nameof(ClipAnimNode.OnPlaying));
        monitor.Should().Raise(nameof(ClipAnimNode.OnLoop));
        clip.LoopCount.Should().BeGreaterThanOrEqualTo(50);
        clip.IsEnded.Should().Be(false);

        // There is no end condition, it should not raise the end event.
    }

    [Theory]
    [InlineData(null, 100f, false)]
    [InlineData(1, 100f, true)]
    [InlineData(1, 1f, true)]
    [InlineData(1, 0.999f, false)]
    [InlineData(2, 1.999f, false)]
    [InlineData(2, 2f, true)]
    public void LoopEndCheck(int? targetLoops, float step, bool shouldRaise)
    {
        // ReSharper disable once RedundantArgumentDefaultValue
        var clip = new TestClipNode("TestClip", 1f);
        using var monitor = clip.Monitor();

        clip.TargetLoopCount = targetLoops;

        clip.Step(step);

        clip.IsEnded.Should().Be(shouldRaise);
        if (shouldRaise)
            monitor.Should().Raise(nameof(ClipAnimNode.OnEndPlay));
        else
            monitor.Should().NotRaise(nameof(ClipAnimNode.OnEndPlay));
    }

    [Theory]
    [InlineData(null, 1, false, 1)]
    [InlineData(null, 1, false, 0)]
    [InlineData(null, 0, false, 0)]
    [InlineData(1f, 1, true, 1)]
    [InlineData(1f, 0.5f, false, 1)]
    [InlineData(2.3f, 3f, true, 1)]
    [InlineData(5f, 10f, true, 0.5f)]
    [InlineData(5f, 10f, false, 0.3f)]
    [InlineData(1f, 1f, true, -1f)]
    [InlineData(1f, 0.9f, false, -1f)]
    [InlineData(1f, 1.1f, true, -1f)]
    public void DurationEndCheck(float? targetDuration, float step, bool shouldRaise, float speed)
    {
        // ReSharper disable once RedundantArgumentDefaultValue
        var clip = new TestClipNode("TestClip", 1f);
        using var monitor = clip.Monitor();

        clip.TargetDuration = targetDuration;
        clip.LocalSpeed = speed;

        clip.Step(step);

        clip.Duration.Should().Be(MathF.Abs(step * speed));
        clip.DurationUnscaled.Should().Be(MathF.Abs(step));

        monitor.Should().Raise(nameof(ClipAnimNode.OnStartPlay));
        monitor.Should().Raise(nameof(ClipAnimNode.OnPlaying));

        clip.IsEnded.Should().Be(shouldRaise);
        if (shouldRaise)
            monitor.Should().Raise(nameof(ClipAnimNode.OnEndPlay));
        else
            monitor.Should().NotRaise(nameof(ClipAnimNode.OnEndPlay));
    }
}