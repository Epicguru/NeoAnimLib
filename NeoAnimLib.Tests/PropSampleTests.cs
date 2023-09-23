namespace NeoAnimLib.Tests;

public class PropSampleTests : TestBase
{
    [Theory]
    [InlineData(0, 1, 0)]
    [InlineData(0, 1, 1)]
    [InlineData(0, 1, 0.5f)]
    [InlineData(0, 1, 0.01f)]
    [InlineData(0, 1, 0.99f)]
    [InlineData(-123f, 612f, 0f)]
    [InlineData(-123f, 612f, 1f)]
    [InlineData(-123f, 612f, 0.99f)]
    [InlineData(-123f, 612f, 0.01f)]
    [InlineData(-123f, 612f, -1f)]
    [InlineData(-123f, 612f, 2f)]
    [InlineData(-123f, 612f, 12f)]
    public void TestLerp(float a, float b, float t)
    {
        const string PATH = "Sample.Path";

        var sampleA = new AnimPropertySample(PATH, a);
        var sampleB = new AnimPropertySample(PATH, b);
        var sampleC = AnimPropertySample.Lerp(sampleA, sampleB, t);

        sampleC.Path.Should().Be(PATH);
        sampleC.Value.Should().BeApproximately(a + (b - a) * t, 0.0001f);
    }
}
