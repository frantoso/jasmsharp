// -----------------------------------------------------------------------
// <copyright file="HistoryTest.cs">
//     Created by Frank Listing at 2025/09/30.
// </copyright>
// -----------------------------------------------------------------------

namespace jasmsharp.Tests;

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using jasmsharp;

[TestClass]
[TestSubject(typeof(History))]
public class HistoryTest
{
    public static IEnumerable<object?[]> TestData =>
    [
        [History.None,  false, false],
        [History.H,  true, false],
        [History.Hd,  false, true],
    ];

    [TestMethod]
    [DynamicData(nameof(TestData))]
    public void TestHistoryChecks(History history, bool isHistory, bool isDeepHistory)
    {
        Assert.AreEqual(isHistory, history.IsHistory);
        Assert.AreEqual(isDeepHistory, history.IsDeepHistory);
    }
}