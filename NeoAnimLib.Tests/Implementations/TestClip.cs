using System.Diagnostics;

namespace NeoAnimLib.Tests.Implementations;

public class TestClip : IAnimClip
{
    public string Name { get; }
    public float Length { get; init; } = 1f;
    public List<AnimEvent> Events { get; } = new List<AnimEvent>();
    public List<AnimPropertySample> Samples { get; } = new List<AnimPropertySample>();

    public TestClip(string name)
    {
        Name = name;
    }

    public void Sample(AnimSample sample, float time, in SamplerInput input)
    {
        foreach (var s in Samples)
        {
            sample.SetProperty(new AnimPropertySample(s.Path, s.Value));
        }
    }

    public IEnumerable<AnimEvent> GetEventsInRange(float start, float end)
    {
        Debug.Assert(start <= end);

        // ReSharper disable once CompareOfFloatsByEqualityOperator
        bool isSingle = start == end || Length == 0;

        // If start==end or this clip is a pose then just find all events at that exact time and return them:
        if (isSingle)
        {
            foreach (var e in Events)
            {
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (e.Time == start)
                    yield return e;
            }
            yield break;
        }

        // Needs to break up the events into chunks, where the chunk is [0, Length] long.
        int startChunkIndex = (int)(start / Length);
        int endChunkIndex = (int)(end / Length);

        for (int chunkIndex = startChunkIndex; chunkIndex <= endChunkIndex; chunkIndex++)
        {
            foreach (var e in Events)
            {
                float time = e.Time + (chunkIndex * Length);
                if (time >= start && time < end)
                    yield return e;
            }
        }
    }
}