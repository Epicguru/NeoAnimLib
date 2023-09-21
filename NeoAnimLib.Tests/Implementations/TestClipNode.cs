using NeoAnimLib.Nodes;

namespace NeoAnimLib.Tests.Implementations;

internal class TestClipNode : ClipAnimNode
{
    public readonly List<AnimPropertySample> Samples = new List<AnimPropertySample>();

    public TestClipNode(string name) : base(new DummyClip(name)) { }

    public override AnimSample Sample(in SamplerInput input)
    {
        var sample = AnimSample.Create(LocalTime);

        foreach (var s in Samples)
        {
            sample.SetSample(s);
        }

        return sample;
    }

    private class DummyClip : IAnimClip
    {
        public string Name { get; }
        public float Length => 1;

        public DummyClip(string name)
        {
            Name = name; 
        }

        public void Sample(AnimSample sample, float time)
        {
            // Does nothing, samples are provided in the node class above.
        }
    }
}