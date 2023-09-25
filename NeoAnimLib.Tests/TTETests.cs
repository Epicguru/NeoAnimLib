using NeoAnimLib.Nodes;
using NeoAnimLib.Tests.Implementations;

namespace NeoAnimLib.Tests;

public class TTETests
{
    [Fact]
    public void AnimClip_DurationTTE()
    {
        var clip = new TestClipNode("TestClip");

        for (float step = 0f; step <= 10f; step += 0.1f)
        {
            for (float dur = 0.1f; dur < 20f; dur += 0.25f)
            {
                for (float speed = -2f; speed <= 2f; speed += 0.2f)
                {
                    clip.Reset();
                    clip.TargetDuration = dur;
                    clip.LocalSpeed = speed;

                    clip.Step(step);

                    float? tte = clip.GetTimeToEnd();
                    tte.Should().NotBeNull();
                    tte.Should().BeGreaterThanOrEqualTo(0);

                    float expectedTTE = MathF.Max(0f, dur - step * MathF.Abs(speed));
                    tte.Should().BeApproximately(expectedTTE, 0.0001f, $"Dur: {dur}, Spd: {speed}, Stp: {step}");
                }
            }
        }
    }

    [Fact]
    public void AnimClip_LoopTTE()
    {
        var clip = new TestClipNode("TestClip");

        for (int loopCount = 1; loopCount <= 10; loopCount++)
        {
            for (float step = 0; step <= 10f; step += 0.2f)
            {
                for (float speed = -2f; speed <= 2f; speed += 0.2f)
                {
                    clip.Reset();
                    clip.TargetLoopCount = loopCount;
                    clip.LocalSpeed = speed;

                    clip.Step(step);

                    clip.LocalTime.Should().Be(step * speed);

                    float? tte = clip.GetTimeToEnd();
                    tte.Should().NotBeNull();
                    tte.Should().BeGreaterThanOrEqualTo(0);

                    float expectedTTE = MathF.Max(0f, loopCount * clip.Length - MathF.Abs(speed) * step);
                    tte.Should().BeApproximately(expectedTTE, 0.0001f, $"LC: {loopCount}, step: {step}, spd: {speed}");
                }
            }
        }
    }

    [Fact]
    public void TransitionTTE()
    {
        var clipA = new TestClipNode("TestClipA");
        var clipB = new TestClipNode("TestClipB");
        var trs = new TransitionNode(clipA, clipB);

        for (float duration = 0.1f; duration <= 2f; duration += 0.1f)
        {
            for (float step = 0f; step <= 3f; step += 0.1f)
            {
                trs.Blend = 0;
                trs.TransitionDuration = duration;

                trs.Step(step);

                float? tte = trs.GetTimeToEnd();
                tte.Should().NotBeNull();
                tte.Should().BeGreaterThanOrEqualTo(0);

                float expectedTTE = MathF.Max(0f, duration - step);

                tte.Should().BeApproximately(expectedTTE, 0.0001f, $"Dur: {duration}, Stp: {step}");
            }
        }
    }
}
