using NeoAnimLib.Nodes;

namespace NeoAnimLib.Tests.Implementations;

internal class TestClipNode : ClipAnimNode
{
    public List<AnimPropertySample> Samples => (Clip as TestClip)!.Samples;
    public List<AnimEvent> Events => (Clip as TestClip)!.Events;

    public TestClipNode(string name, float length = 1) : base(new TestClip(name)
    {
        Length = length,
    }) { }

    public void Reset()
    {
        LocalTime = 0;
        LoopCount = 0;
        Duration = 0;
        LastLoopIndex = 0;
    }
}