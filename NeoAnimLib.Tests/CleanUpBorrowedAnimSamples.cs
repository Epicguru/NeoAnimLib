using System.Reflection;
using Xunit.Sdk;

namespace NeoAnimLib.Tests;

public class CleanUpBorrowedAnimSamples : BeforeAfterTestAttribute
{
    public override void After(MethodInfo methodUnderTest)
    {
        int borrowedAnimSampleCount = AnimSample.BorrowedCount;

        AnimSample.ResetBorrowedCount();

        if (borrowedAnimSampleCount != 0)
            Assert.Fail($"Test '{methodUnderTest.Name}' should dispose of all AnimSamples that they create. " +
                        $"There were {borrowedAnimSampleCount} active samples after the test finished running.");
    }
}
