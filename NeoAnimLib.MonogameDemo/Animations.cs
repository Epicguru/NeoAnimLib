using System;
using System.Collections.Generic;

namespace NeoAnimLib.MonogameDemo;

public static class Animations
{
    public static readonly MonoClip VerticalHover = new MonoClip
    {
        Name = "Vertical Hover",
        Length = 1f,
        SampleAction = (sample, time) =>
        {
            float y = MathF.Sin(time * MathF.PI) * 200f + 200f;
            sample.SetProperty(new AnimPropertySample("Y", y));
        }
    };

    public static readonly MonoClip HorizontalHover = new MonoClip
    {
        Name = "Horizontal Hover",
        Length = 1f,
        SampleAction = (sample, time) =>
        {
            float x = MathF.Cos(time * MathF.PI) * 200f + 200f;
            sample.SetProperty(new AnimPropertySample("X", x));
        }
    };

    public static float DefaultValueSource(string propName) => propName switch
    {
        "X" => 200,
        "Y" => 200,
        _ => 0
    };
}

public class MonoClip : IAnimClip
{
    public required Action<AnimSample, float> SampleAction { get; init; }
    public required string Name { get; init; }
    public required float Length { get; init; }

    public void Sample(AnimSample sample, float time) => SampleAction(sample, time);

    public IEnumerable<AnimEvent> GetEventsInRange(float startTime, float endTime)
    {
        yield break;
    }
}
