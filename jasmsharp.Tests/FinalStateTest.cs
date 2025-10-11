// -----------------------------------------------------------------------
// <copyright file="FinalStateTest.cs">
//     Created by Frank Listing at 2025/10/03.
// </copyright>
// -----------------------------------------------------------------------

namespace jasmsharp.Tests;

using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class FinalStateTest
{
    [TestMethod]
    public void VerifyFinalState()
    {
        var state = new FinalState();

        Assert.AreEqual("Final", state.Name);
    }

    public static IEnumerable<object?[]> TestData =>
    [
        [new FinalState(), true],
        [new InitialState(), false],
        [null, false],
    ];

    [TestMethod]
    [DynamicData(nameof(FinalStateTest.TestData))]
    public void EqualsReturnsTrueForDifferentObjectsOfFinalState(IState? toCompare, bool expected)
    {
        var state = new FinalState();

        Assert.AreEqual(expected, state.Equals(toCompare));
        Assert.AreEqual(expected, state.GetHashCode() == toCompare?.GetHashCode());
    }
}