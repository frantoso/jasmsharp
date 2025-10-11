// -----------------------------------------------------------------------
// <copyright file="ChangeStateDataTest.cs">
//     Created by Frank Listing at 2025/09/30.
// </copyright>
// -----------------------------------------------------------------------

namespace jasmsharp.Tests;

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using jasmsharp;

[TestClass]
[TestSubject(typeof(ChangeStateData))]
public class ChangeStateDataTest
{
    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public void GetHandledNoEndpoint(bool input)
    {
        var data = new ChangeStateData(input);

        Assert.AreEqual(input, data.Handled);
        Assert.IsNull(data.EndPoint);
    }

    public static IEnumerable<object?[]> TestData =>
    [
        [true, new TransitionEndPoint(new FinalState())],
        [true, new TransitionEndPoint(new State("Test"))],
        [true, null],
    ];

    [TestMethod]
    [DynamicData(nameof(TestData))]
    public void GetHandled3(bool input, TransitionEndPoint endPoint)
    {
        var data = new ChangeStateData(input, endPoint);

        Assert.AreEqual(input, data.Handled);
        Assert.AreSame(endPoint, data.EndPoint);
    }
}