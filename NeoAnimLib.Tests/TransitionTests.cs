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

        ClipAnimNode from = (node.FromNode as ClipAnimNode)!;
        ClipAnimNode to = (node.ToNode as ClipAnimNode)!;
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
            sample.Should().NotBeNull();

            bool found = sample!.TryGetProperty("PathA", out var prop);
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

        node.FromNode.Should().Be(from);
        node.ToNode.Should().Be(to);
    }

    [Theory]
    [InlineData(0f, 0f, 1f, 0f)]
    [InlineData(1f, 0f, 1f, 1f)]
    [InlineData(1f, 0f, 0f, 0f)]
    [InlineData(1f, 0f, 0.01f, 1f)]
    [InlineData(1f, 1f, 1f, 1f)]
    [InlineData(1f, 1f, 0.5f, 0.5f)]
    [InlineData(1f, 1f, 2f, 1f)]
    [InlineData(1f, 2f, 1f, 0.5f)]
    [InlineData(1f, 2f, 2f, 1f)]
    [InlineData(1f, 1f, -1f, 1f)]
    [InlineData(1f, 0.1f, 1f, 1)]
    public void TestTransitionDuration(float deltaTime, float duration, float speed, float expectedBlend)
    {
        var clipA = new TestClip("ClipA");
        var clipB = new TestClip("ClipB");
        TransitionNode node = new TransitionNode(clipA, clipB);

        ClipAnimNode from = (node.FromNode as ClipAnimNode)!;
        ClipAnimNode to = (node.ToNode as ClipAnimNode)!;
        from.Should().NotBeNull();
        to.Should().NotBeNull();

        node.Blend.Should().Be(0);

        from.Clip.Should().Be(clipA);
        from.LocalTime.Should().Be(0);
        from.HasStartedPlaying.Should().BeFalse();

        to.Clip.Should().Be(clipB);
        to.LocalTime.Should().Be(0);
        to.HasStartedPlaying.Should().BeFalse();

        node.TransitionDuration = duration;

        node.LocalSpeed = speed;

        node.Step(deltaTime);

        node.Blend.Should().BeGreaterThanOrEqualTo(0f);
        node.Blend.Should().Be(expectedBlend);
    }

    [Theory]
    [InlineData(null, 0f, false, 1f)]
    [InlineData(null, 0f, false, 0f)]
    [InlineData(null, 1f, false, 1f)]
    [InlineData(null, 1f, false, 0f)]
    [InlineData(1f, 1f, true, 1f)]
    [InlineData(1f, 0.99f, false, 1f)]
    [InlineData(1f, 1f, true, -1f)]
    [InlineData(1f, 0.99f, false, -1f)]
    [InlineData(0.5f, 0.99f, true, -1f)]
    public void TestTransitionEnd(float? duration, float step, bool shouldEnd, float speed)
    {
        var clipA = new TestClip("ClipA") { Length = 1 };
        var clipB = new TestClip("ClipB") { Length = 1 };
        TransitionNode node = new TransitionNode(clipA, clipB)
        {
            TransitionDuration = duration,
            LocalSpeed = speed
        };

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
        ClipAnimNode from = (node.FromNode as ClipAnimNode)!;
        ClipAnimNode to = (node.ToNode as ClipAnimNode)!;

        to.Should().NotBeNull();
        to.Clip.Should().Be(clipB);

        from.Should().NotBeNull();
        from.Clip.Should().Be(clipA);
        from.TargetDuration = duration;

        node.Step(step);

        from.IsEnded.Should().Be(shouldReplace);
        if (shouldReplace)
        {
            node.FromNode.Should().NotBe(from);
            node.FromNode.Should().BeOfType<ClipAnimNode>();
            ClipAnimNode newFrom = (node.FromNode as ClipAnimNode)!;
            newFrom.Clip.Should().BeOfType<SnapshotAnimClip>();

            using var sample = newFrom.Sample(new SamplerInput { MissingPropertyBehaviour = MissingPropertyBehaviour.UseKnownValue });
            bool found = sample.TryGetProperty("A", out var prop);
            found.Should().BeTrue();
            prop.Value.Should().Be(123f);
        }
        else
        {
            node.FromNode.Should().Be(from);
        }

        node.AllChildren.Should().HaveCount(2);
        node.ToNode.Should().Be(to);
    }

    [Theory]
    [InlineData(0f, null, false, TransitionEndBehaviour.ReplaceInParent)]
    [InlineData(1f, 1f, true, TransitionEndBehaviour.ReplaceInParent)]
    [InlineData(1.1f, 1f, true, TransitionEndBehaviour.ReplaceInParent)]
    [InlineData(1f, 1f, true, TransitionEndBehaviour.Nothing)]
    [InlineData(0.99f, 1f, false, TransitionEndBehaviour.ReplaceInParent)]
    [InlineData(2f, 1f, true, TransitionEndBehaviour.ReplaceInParent)]
    [InlineData(2f, 0.1f, true, TransitionEndBehaviour.ReplaceInParent)]
    public void TestTransitionEndBehaviour(float step, float? duration, bool shouldEnd, TransitionEndBehaviour endBehaviour)
    {
        var clipA = new TestClip("ClipA");
        var clipB = new TestClip("ClipB");

        TransitionNode node = new TransitionNode(clipA, clipB)
        {
            TransitionDuration = duration,
            EndBehaviour = endBehaviour
        };

        AnimNode parent = new AnimNode("Parent");
        parent.Add(node);

        var from = node.FromNode;
        var to = node.ToNode;

        node.Parent.Should().Be(parent);
        from.Parent.Should().Be(node);
        from.Depth.Should().Be(2);
        to.Depth.Should().Be(2);

        using var monitor = node.Monitor();

        parent.Step(step);

        from.LocalTime.Should().Be(step);
        to.LocalTime.Should().Be(step);
        node.IsEnded.Should().Be(shouldEnd);

        if (shouldEnd)
            monitor.Should().Raise(nameof(TransitionNode.OnTransitionEnd));
        else
            monitor.Should().NotRaise(nameof(TransitionNode.OnTransitionEnd));

        switch (endBehaviour)
        {
            case TransitionEndBehaviour.ReplaceInParent when shouldEnd:
                parent.DirectChildren.Should().ContainSingle();
                parent.AllChildren.Should().ContainSingle();
                parent.DirectChildren.First().Should().Be(to);
                to.Parent.Should().Be(parent);
                node.Parent.Should().BeNull();
                break;

            case TransitionEndBehaviour.Nothing:
                parent.DirectChildren.Should().ContainSingle();
                parent.DirectChildren.First().Should().Be(node);
                node.Parent.Should().Be(parent);
                break;
        }
    }

    [Fact]
    public void TransitionStopTests()
    {
        var clipA = new TestClip("ClipA");
        clipA.Samples.Add(new AnimPropertySample("A", 10f));
        clipA.Samples.Add(new AnimPropertySample("B", 5f));
        var clipB = new TestClip("ClipB");
        clipB.Samples.Add(new AnimPropertySample("A", 20f));

        TransitionNode node = new TransitionNode(clipA, clipB)
        {
            TransitionDuration = 1f,
            EndBehaviour = TransitionEndBehaviour.ReplaceInParent
        };

        AnimNode parent = new AnimNode("Parent");
        parent.Add(node);

        node.Step(0.5f);
        node.IsEnded.Should().BeFalse();

        node.Stop();
        node.IsEnded.Should().BeTrue();

        parent.AllChildren.Should().ContainSingle();
        var child = parent.DirectChildren[0];

        child.Should().BeOfType<ClipAnimNode>();
        var clip = (ClipAnimNode)child;

        clip.Clip.Should().BeOfType<SnapshotAnimClip>();

        using var sample = clip.Sample(new SamplerInput { MissingPropertyBehaviour = MissingPropertyBehaviour.UseKnownValue });
        sample.Samples.Should().HaveCount(2);

        sample.TryGetProperty("A", out var a).Should().BeTrue();
        sample.TryGetProperty("B", out var b).Should().BeTrue();

        a.Value.Should().Be(15f);
        b.Value.Should().Be(5f);
    }
}
