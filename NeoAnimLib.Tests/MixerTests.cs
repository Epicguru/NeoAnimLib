using NeoAnimLib.Nodes;
using NeoAnimLib.Tests.Implementations;

namespace NeoAnimLib.Tests;

public class MixerTests : TestBase
{
    private const float DEFAULT_PROP_VALUE = 20f;

    [Fact]
    public void TestSingleSampler()
    {
        const string NAME = "dev.path";
        const float VALUE = 5;

        var node = new TestClipNode("TestClipNode");
        node.Samples.Add(new AnimPropertySample(NAME, VALUE));

        var sampler = new SamplerInput
        {
            MissingPropertyBehaviour = MissingPropertyBehaviour.UseDefaultValue,
            DefaultValueSource = DefaultValueProvider
        };

        using var sample = node.Sample(sampler);

        sample.Samples.Should().ContainSingle();
        sample.Samples.First().Path.Should().Be(NAME);
        sample.Samples.First().Value.Should().Be(VALUE);
    }

    [Fact]
    public void TestMixerWithSingleSampler()
    {
        const string NAME = "dev.path";
        const float VALUE = 5;

        var mixer = new MixAnimNode("TestMixNode");
        var clip = new TestClipNode("TestClipNode");

        mixer.Add(clip);

        // Parent-child checks:
        mixer.DirectChildren.Should().ContainSingle();
        mixer.AllChildren.Should().ContainSingle();
        mixer.DirectChildren.Should().Contain(clip);
        clip.Parent.Should().Be(mixer);

        clip.Samples.Add(new AnimPropertySample(NAME, VALUE));

        var sampler = new SamplerInput
        {
            MissingPropertyBehaviour = MissingPropertyBehaviour.UseDefaultValue,
            DefaultValueSource = DefaultValueProvider
        };

        void CheckWithDefault(string pass, float w, bool n)
        {
            using var sample = mixer.Sample(sampler);
            sample.Should().NotBeNull();

            sample!.Samples.Should().ContainSingle();
            sample.Samples.First().Path.Should().Be(NAME, pass);

            float expectedValue;
            if (w == 0f) // If weight is 0, it will always take the default value.
                expectedValue = DEFAULT_PROP_VALUE;
            else if (n) // If weight is not 0 and normalizing, the value will always be the clip value.
                expectedValue = VALUE;
            else // Otherwise (weight is not 0 and not normalizing) then the value will be a lerp between the default value and the clip value.
                expectedValue = Lerp(DEFAULT_PROP_VALUE, VALUE, w);

            sample.Samples.First().Value.Should().Be(expectedValue, pass);
        }

        void CheckWithKnownValue(string pass)
        {
            using var sample = mixer.Sample(sampler);
            sample.Samples.Should().ContainSingle();
            sample.Samples.First().Path.Should().Be(NAME, pass);

            // When using the 'Known Value' mode,
            // regardless of weight, the clip value is used.
            float expectedValue = VALUE;

            sample.Samples.First().Value.Should().Be(expectedValue, pass);
        }

        mixer.NormalizeWeights = true;
        CheckWithDefault("normalized weights enabled, w=1", clip.LocalWeight, mixer.NormalizeWeights);

        clip.LocalWeight = 0.5f;
        CheckWithDefault("normalized weights enabled, w=0.5", clip.LocalWeight, mixer.NormalizeWeights);

        clip.LocalWeight = 0f;
        CheckWithDefault("normalized weights enabled, w=0", clip.LocalWeight, mixer.NormalizeWeights);

        mixer.NormalizeWeights = false;
        clip.LocalWeight = 0f;
        CheckWithDefault("normalized weights disabled, w=0", clip.LocalWeight, mixer.NormalizeWeights);

        clip.LocalWeight = 1f;
        CheckWithDefault("normalized weights disabled, w=1", clip.LocalWeight, mixer.NormalizeWeights);

        sampler = new SamplerInput
        {
            MissingPropertyBehaviour = MissingPropertyBehaviour.UseKnownValue
        };

        mixer.NormalizeWeights = true;
        CheckWithKnownValue("normalized weights enabled, w=1");

        clip.LocalWeight = 0.5f;
        CheckWithKnownValue("normalized weights enabled, w=0.5");

        clip.LocalWeight = 0f;
        CheckWithKnownValue("normalized weights enabled, w=0");

        mixer.NormalizeWeights = false;
        clip.LocalWeight = 0f;
        CheckWithKnownValue("normalized weights disabled, w=0");

        clip.LocalWeight = 1f;
        CheckWithKnownValue("normalized weights disabled, w=1");
    }

    [Fact]
    public void TestMixerWithTwoInputs()
    {
        var mixer = new MixAnimNode("TestMixer");
        var clipA = new TestClipNode("ClipA");
        var clipB = new TestClipNode("ClipB");

        mixer.Add(clipA);
        mixer.Add(clipB);

        mixer.AllChildren.Should().Contain(clipA);
        mixer.AllChildren.Should().Contain(clipB);
        mixer.AllChildren.Count().Should().Be(2);

        const string PROP_NAME   = "PropName";
        const string PROP_NAME_2 = "PropName2";
        const float VALUE_A = 123f;
        const float VALUE_B = -5f;
        const float VALUE_B_2 = 512f;

        clipA.Samples.Add(new AnimPropertySample(PROP_NAME, VALUE_A));
        clipB.Samples.Add(new AnimPropertySample(PROP_NAME, VALUE_B));
        clipB.Samples.Add(new AnimPropertySample(PROP_NAME_2, VALUE_B_2));

        // When normalized, samples should have equally blended:
        mixer.NormalizeWeights = true;

        using (var output = mixer.Sample(new SamplerInput { MissingPropertyBehaviour = MissingPropertyBehaviour.UseKnownValue } ))
        {
            output.Samples.Count.Should().Be(2);

            bool found = output.TryGetProperty(PROP_NAME, out var outputProp);
            found.Should().Be(true);
            outputProp.Value.Should().Be(Lerp(VALUE_A, VALUE_B, 0.5f));

            found = output.TryGetProperty(PROP_NAME_2, out outputProp);
            found.Should().Be(true);
            outputProp.Value.Should().Be(VALUE_B_2);
        }

        // When not normalized, and both weights set to 1, the second clip should override the first one.
        mixer.NormalizeWeights = false;
        clipA.LocalWeight = 1f;
        clipB.LocalWeight = 1f;

        using (var output = mixer.Sample(new SamplerInput { MissingPropertyBehaviour = MissingPropertyBehaviour.UseKnownValue }))
        {
            output.Samples.Count.Should().Be(2);
            bool found = output.TryGetProperty(PROP_NAME, out var outputProp);
            found.Should().Be(true);

            outputProp.Value.Should().Be(VALUE_B);

            found = output.TryGetProperty(PROP_NAME_2, out outputProp);
            found.Should().Be(true);
            outputProp.Value.Should().Be(VALUE_B_2);
        }

        AnimSample.BorrowedCount.Should().Be(0);
    }

    private static float DefaultValueProvider(string propName) => DEFAULT_PROP_VALUE;
}
