// -----------------------------------------------------------------------
// <copyright file="TransitionEndPointTest.cs">
//     Created by Frank Listing at 2025/09/30.
// </copyright>
// -----------------------------------------------------------------------

namespace jasmsharp.Tests;

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using jasmsharp;

[TestClass]
[TestSubject(typeof(TransitionEndPoint))]
public class TransitionEndPointTest
{
    [TestMethod]
    public void CreteEndPointWithDefaultHistory()
    {
        var endPoint = new TransitionEndPoint(new State("xyz"));

        Assert.AreEqual("xyz", endPoint.State.Name);
        Assert.AreSame(History.None, endPoint.History);
    }

    public static IEnumerable<object[]> TestData =>
    [
        [new State("state1"), "state1", History.None],
        [new State("state"), "state", History.H],
        [new State("xyz"), "xyz", History.Hd],
    ];

    [TestMethod]
    [DynamicData(nameof(TestData))]
    public void CreteEndPointWithHistory(State state, string stateName, History history)
    {
        var endPoint = new TransitionEndPoint(state, history);

        Assert.AreEqual(stateName, endPoint.State.Name);
        Assert.AreSame(history, endPoint.History);
    }
}