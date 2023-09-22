using NeoAnimLib.Nodes;
using System.Diagnostics;

namespace NeoAnimLib.Tests.Implementations;

internal class TestClipNode : ClipAnimNode
{
    public readonly List<AnimPropertySample> Samples = new List<AnimPropertySample>();
    public readonly List<AnimEvent> Events = new List<AnimEvent>();

    public TestClipNode(string name, float length = 1) : base(new DummyClip(name)
    {
        Length = length,
    })
    {
        ((Clip as DummyClip)!).GetEvents = GetEventsInRange;
    }

    private IEnumerable<AnimEvent> GetEventsInRange(float start, float end)
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
        int startChunkIndex = (int) (start / Length);
        int endChunkIndex = (int) (end / Length);

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

    public override AnimSample Sample(in SamplerInput input)
    {
        var sample = AnimSample.Create(LocalTime);

        foreach (var s in Samples)
        {
            sample.SetProperty(s);
        }

        return sample;
    }

    public void Reset()
    {
        LocalTime = 0;
        LoopCount = 0;
        LastLoopIndex = 0;
    }

    public void SetLocalTime(float time) => LocalTime = time;

    private class DummyClip : IAnimClip
    {
        public string Name { get; }
        public float Length { get; init; } = 1f;
        public Func<float, float, IEnumerable<AnimEvent>> GetEvents { get; set; }

        public DummyClip(string name)
        {
            Name = name; 
        }

        public void Sample(AnimSample sample, float time)
        {
            // Does nothing, samples are provided in the node class above.
        }

        public IEnumerable<AnimEvent> GetEventsInRange(float startTime, float endTime) => GetEvents(startTime, endTime);
    }
}