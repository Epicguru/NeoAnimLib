using NeoAnimLib.Nodes;
using NeoAnimLib.Tests.Implementations;

namespace NeoAnimLib.Tests;

public class EventsTests
{
    [Theory]
    // Middle of clip event:
    [InlineData(0.5f, 0f, 0f, 0)]
    [InlineData(0.5f, 1f, 1f, 0)]
    [InlineData(0.5f, 0f, 0.1f, 0)]
    [InlineData(0.5f, 0f, 0.5f, 0)]
    [InlineData(0.5f, 0.5f, 1f, 1)]
    [InlineData(0.5f, 0.5f, 0.5f, 0)]
    [InlineData(0.5f, 0f, 0.51f, 1)]
    [InlineData(0.5f, 0f, 2f, 2)]
    [InlineData(0.5f, -2f, 0f, 2)]
    // Start of clip event:
    [InlineData(0f, 0f, 0f, 0)]
    [InlineData(0f, 0f, 0.1f, 1)]
    [InlineData(0f, 0f, 1f, 1)]
    [InlineData(0f, 0f, 2f, 2)]
    [InlineData(0f, -1f, 1f, 2)]
    // End of clip event:
    [InlineData(1f, 0f, 0f, 0)]
    [InlineData(1f, 0f, 1f, 0)]
    [InlineData(1f, 0f, 1.1f, 1)]
    [InlineData(1f, 1f, 1f, 0)]
    [InlineData(1f, 1f, 1.1f, 1)]
    [InlineData(1f, 1f, 2f, 1)]
    [InlineData(1f, 1f, 2.1f, 2)]
    public void SingleEventTest(float eventTime, float startTime, float endTime, int expectedCount)
    {
        startTime.Should().BeLessThanOrEqualTo(endTime);

        var clip = new TestClipNode("TestClip");

        int raiseCount = 0;

        void Increment(ClipAnimNode node)
        {
            node.Should().Be(clip);
            raiseCount++;
        }

        clip.Events.Add(new AnimEvent(eventTime, Increment));
        using var monitor = clip.Monitor();

        clip.SetLocalTime(startTime);
        clip.Step(endTime - startTime);

        clip.LocalTime.Should().Be(endTime);
        monitor.Should().Raise(nameof(ClipAnimNode.OnStartPlay));
        monitor.Should().Raise(nameof(ClipAnimNode.OnPlaying));
        monitor.Should().NotRaise(nameof(ClipAnimNode.OnEndPlay));
        clip.IsEnded.Should().BeFalse();

        raiseCount.Should().Be(expectedCount);
    }
}
