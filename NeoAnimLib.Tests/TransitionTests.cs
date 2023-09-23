using NeoAnimLib.Clips;
using NeoAnimLib.Nodes;
using NeoAnimLib.Tests.Implementations;

namespace NeoAnimLib.Tests;

public class TransitionTests : TestBase
{
    [Fact]
    public void CheckEventsAndSamples()
    {
        const float A = -1f;
        const float B = 2f;

        var clipA = new TestClip("ClipA");
        clipA.Samples.Add(new AnimPropertySample("PathA", A));

        var clipB = new TestClip("ClipA");
        clipB.Samples.Add(new AnimPropertySample("PathA", B));

        TransitionNode node = new TransitionNode(clipA, clipB);

        var from = node.FromClip as ClipAnimNode;
        var to = node.ToClip as ClipAnimNode;
        from.Should().NotBeNull();
        to.Should().NotBeNull();

        from.Clip.Should().Be(clipA);
        from.LocalTime.Should().Be(0);
        from.HasStartedPlaying.Should().BeFalse();

        to.Clip.Should().Be(clipB);
        to.LocalTime.Should().Be(0);
        to.HasStartedPlaying.Should().BeFalse();

        void CheckBlend(float blend, float expectedValue)
        {
            node.Blend = blend;

            using var sample = node.Sample(new SamplerInput
            {
                DefaultValueSource = null,
                MissingPropertyBehaviour = MissingPropertyBehaviour.UseKnownValue,
            });

            bool found = sample.TryGetProperty("PathA", out var prop);
            found.Should().BeTrue();

            prop.Path.Should().Be("PathA");
            prop.Value.Should().Be(expectedValue);
        }

        // Check that samples correctly interpolate between A and B values.
        for (float t = 0; t <= 1f; t += 0.05f)
        {
            CheckBlend(t, Lerp(A, B, t));
        }

        // Check stepping and events:
        from.IsEnded.Should().BeFalse();
        from.LocalTime.Should().Be(0);
        to.IsEnded.Should().BeFalse();
        to.LocalTime.Should().Be(0);

        using var monitorA = from.Monitor();
        using var monitorB = to.Monitor();
        node.Step(0.1f);

        from.HasStartedPlaying.Should().BeTrue();
        from.LocalTime.Should().Be(0.1f);
        to.HasStartedPlaying.Should().BeTrue();
        to.LocalTime.Should().Be(0.1f);

        monitorA.Should().Raise(nameof(ClipAnimNode.OnStartPlay));
        monitorB.Should().Raise(nameof(ClipAnimNode.OnStartPlay));
        monitorA.Should().Raise(nameof(ClipAnimNode.OnPlaying));
        monitorB.Should().Raise(nameof(ClipAnimNode.OnPlaying));

        // Check that samples correctly interpolate between A and B values (again, after step).
        for (float t = 0; t <= 1f; t += 0.05f)
        {
            CheckBlend(t, Lerp(A, B, t));
        }

        node.FromClip.Should().Be(from);
        node.ToClip.Should().Be(to);
    }

    [Theory]
    [InlineData(0f, 0f, 1f, false, 0f)]
    [InlineData(1f, 0f, 1f, false, 1f)]
    [InlineData(1f, 0f, 1f, true, 1f)]
    [InlineData(1f, 0f, 0f, false, 0f)]
    [InlineData(1f, 0f, 0.01f, false, 1f)]
    [InlineData(1f, 1f, 1f, false, 1f)]
    [InlineData(1f, 1f, 0.5f, false, 0.5f)]
    [InlineData(1f, 1f, 2f, false, 1f)]
    [InlineData(1f, 2f, 1f, false, 0.5f)]
    [InlineData(1f, 2f, 2f, false, 1f)]
    [InlineData(1f, 1f, -1f, false, 1f)]
    [InlineData(1f, 1f, -1f, false, 1f)]
    [InlineData(1f, 1f, 0.5f, false, 0.5f)]
    [InlineData(1f, 0.1f, 1f, false, 1)]
    public void TestTransitionDuration(float deltaTime, float duration, float speed, bool unscaled, float expectedBlend)
    {
        var clipA = new TestClip("ClipA");
        var clipB = new TestClip("ClipB");
        TransitionNode node = new TransitionNode(clipA, clipB);

        var from = node.FromClip as ClipAnimNode;
        var to = node.ToClip as ClipAnimNode;
        from.Should().NotBeNull();
        to.Should().NotBeNull();

        node.Blend.Should().Be(0);

        from.Clip.Should().Be(clipA);
        from.LocalTime.Should().Be(0);
        from.HasStartedPlaying.Should().BeFalse();

        to.Clip.Should().Be(clipB);
        to.LocalTime.Should().Be(0);
        to.HasStartedPlaying.Should().BeFalse();

        if (unscaled)
            node.TransitionDurationUnscaled = duration;
        else
            node.TransitionDuration = duration;

        node.LocalSpeed = speed;

        node.Step(deltaTime);

        node.Blend.Should().BeGreaterThanOrEqualTo(0f);
        node.Blend.Should().Be(expectedBlend);
    }

    [Theory]
    [InlineData(null, 0f, false, false, 1f)]
    [InlineData(null, 0f, false, false, 0f)]
    [InlineData(null, 1f, false, false, 1f)]
    [InlineData(null, 1f, false, false, 0f)]
    [InlineData(1f, 1f, true, false, 1f)]
    [InlineData(1f, 1f, true, true, 1f)]
    [InlineData(1f, 0.99f, false, false, 1f)]
    [InlineData(1f, 0.99f, false, true, 1f)]
    [InlineData(1f, 1f, true, false, -1f)]
    [InlineData(1f, 1f, true, true, -1f)]
    [InlineData(1f, 0.99f, false, true, -1f)]
    [InlineData(1f, 0.99f, false, false, -1f)]
    [InlineData(0.5f, 0.99f, true, false, -1f)]
    [InlineData(0.5f, 0.99f, true, true, -1f)]
    public void TestTransitionEnd(float? duration, float step, bool shouldEnd, bool unscaled, float speed)
    {
        var clipA = new TestClip("ClipA") { Length = 1 };
        var clipB = new TestClip("ClipB") { Length = 1 };
        TransitionNode node = new TransitionNode(clipA, clipB);

        if (unscaled)
            node.TransitionDurationUnscaled = duration;
        else
            node.TransitionDuration = duration;

        node.LocalSpeed = speed;

        using var monitor = node.Monitor();

        node.Step(step);

        // Check ended and raised:
        node.IsEnded.Should().Be(shouldEnd);
        if (shouldEnd)
            monitor.Should().Raise(nameof(TransitionNode.OnTransitionEnd));
        else
            monitor.Should().NotRaise(nameof(TransitionNode.OnTransitionEnd));
    }

    [Theory]
    [InlineData(0f, null, false)]
    [InlineData(1f, null, false)]
    [InlineData(2f, null, false)]
    [InlineData(0f, 2f, false)]
    [InlineData(1f, 2f, false)]
    [InlineData(1.99f, 2f, false)]
    [InlineData(2f, 2f, true)]
    [InlineData(2.01f, 2f, true)]
    [InlineData(4f, 2f, true)]
    [InlineData(20f, 2f, true)]
    public void TestTransitionReplacement(float step, float? duration, bool shouldReplace)
    {
        var clipA = new TestClip("ClipA") { Length = 1 };
        clipA.Samples.Add(new AnimPropertySample("A", 123f));
        var clipB = new TestClip("ClipB") { Length = 1 };

        TransitionNode node = new TransitionNode(clipA, clipB);
        ClipAnimNode from = (node.FromClip as ClipAnimNode)!;
        ClipAnimNode to = (node.ToClip as ClipAnimNode)!;

        to.Should().NotBeNull();
        to.Clip.Should().Be(clipB);

        from.Should().NotBeNull();
        from.Clip.Should().Be(clipA);
        from.TargetDuration = duration;

        node.Step(step);

        from.IsEnded.Should().Be(shouldReplace);
        if (shouldReplace)
        {
            node.FromClip.Should().NotBe(from);
            node.FromClip.Should().BeOfType<ClipAnimNode>();
            ClipAnimNode newFrom = (node.FromClip as ClipAnimNode)!;
            newFrom.Clip.Should().BeOfType<SnapshotAnimClip>();

            using var sample = newFrom.Sample(new SamplerInput { MissingPropertyBehaviour = MissingPropertyBehaviour.UseKnownValue });
            bool found = sample.TryGetProperty("A", out var prop);
            found.Should().BeTrue();
            prop.Value.Should().Be(123f);
        }
        else
        {
            node.FromClip.Should().Be(from);
        }

        node.AllChildren.Should().HaveCount(2);
        node.ToClip.Should().Be(to);
    }
}
