// -----------------------------------------------------------------------
// <copyright file="StateInfoTest.cs">
//     Created by Frank Listing at 2025/12/21.
// </copyright>
// -----------------------------------------------------------------------

namespace jasmsharp_debug_adapter.Tests.model;

using System.Collections.Generic;
using jasmsharp_debug_adapter.model;
using JetBrains.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
[TestSubject(typeof(StateInfo))]
public class StateInfoTest
{
    public static IEnumerable<object[]> UpdateTestData =>
    [
        [false, false, false, false, false, false],
        [false, false, true, false, true, false],
        [false, false, false, true, false, true],
        [false, false, true, true, true, true],
        [true, false, false, false, true, false],
        [false, true, false, false, false, true],
        [true, true, false, false, true, true],
        [true, false, true, false, true, false],
        [false, true, false, true, false, true],
        [true, false, false, true, true, true],
        [false, true, true, false, true, true],
        [true, true, true, true, true, true]
    ];

    [TestMethod]
    [DynamicData(nameof(UpdateTestData))]
    public void UpdateTest(
        bool hasHistoryInit,
        bool hasDeepHistoryInit,
        bool hasHistoryUpdate,
        bool hasDeepHistoryUpdate,
        bool hasHistoryExpected,
        bool hasDeepHistoryExpected)
    {
        var origin = new StateInfo("id", "name", false, false, [], [], hasHistoryInit, hasDeepHistoryInit);
        var updated = origin.Update(hasHistoryUpdate, hasDeepHistoryUpdate);

        // Verify that the original instance remains unchanged
        Assert.AreEqual(hasHistoryInit, origin.HasHistory);
        Assert.AreEqual(hasDeepHistoryInit, origin.HasDeepHistory);

        // Verify that the updated instance has the expected values
        Assert.AreEqual(hasHistoryExpected, updated.HasHistory);
        Assert.AreEqual(hasDeepHistoryExpected, updated.HasDeepHistory);
    }
}