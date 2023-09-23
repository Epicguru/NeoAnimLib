
[assembly: CollectionBehavior(CollectionBehavior.CollectionPerAssembly)]

namespace NeoAnimLib.Tests;

[CleanUpBorrowedAnimSamples]
public class TestBase
{
    protected static float Lerp(float a, float b, float t) => a + (b - a) * t;
}
