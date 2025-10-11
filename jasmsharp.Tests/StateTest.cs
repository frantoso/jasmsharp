// -----------------------------------------------------------------------
// <copyright file="StateTest.cs">
//     Created by Frank Listing at 2025/09/30.
// </copyright>
// -----------------------------------------------------------------------

namespace jasmsharp.Tests;

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using jasmsharp;

[TestClass]
[TestSubject(typeof(State))]
public class StateTest
{
    [TestMethod]
    public void VerifyNormalState()
    {
        var state = new State(StateTest.TestState1Name);

        Assert.AreEqual(StateTest.TestState1Name, state.Name);
        Assert.AreEqual(StateTest.TestState1Name, state.ToString());
        Assert.Contains("State_0", state.Id);
    }

    [TestMethod]
    public void GetsTheEndPointForHistory()
    {
        var state = new State(StateTest.TestState1Name);

        var endPoint = state.History;

        Assert.IsTrue(endPoint.History.IsHistory);
        Assert.IsFalse(endPoint.History.IsDeepHistory);
        Assert.AreSame(state, endPoint.State);
    }

    [TestMethod]
    public void GetsTheEndPointForDeepHistory()
    {
        var state = new State(StateTest.TestState1Name);

        var endPoint = state.DeepHistory;

        Assert.IsFalse(endPoint.History.IsHistory);
        Assert.IsTrue(endPoint.History.IsDeepHistory);
        Assert.AreSame(state, endPoint.State);
    }

    public static IEnumerable<object[]> NameTestData =>
    [
        [new State("test-state-1"), "test-state-1"],
        [new State(), "State"],
        [new State(" "), "State"],
        [new State("  "), "State"],
        [new State("test-state-2"), "test-state-2"],
        [new StateNo1(), "StateNo1"],
    ];

    [TestMethod]
    [DynamicData(nameof(NameTestData))]
    public void CreatesTheRightName(IState state, string expected)
    {
        Assert.AreEqual(expected, state.Name, $"name should be {expected}");
    }

    private const string TestState1Name = "test-state-1";

    // Test-only subclass to verify default naming when no explicit name is provided
    private sealed class StateNo1 : State;

    private bool Tr<TEvent>( Func<bool> guard) where TEvent : Event
    {
        return false;
    }

    private bool Tr<TEvent, T>( Func<T, bool> guard) where TEvent : Event
    {
        return false;
    }

    private bool Tr<TEvent>() where TEvent : Event
    {
        return false;
    }

    private bool Tr<T>(T guard)
    {
        return true;
    }

    private bool Testi(int i)
    {
        return i > 0;
    }
    
    private bool Test()
    {
        return true;
    }
    
    [TestMethod]
    public void TuWas()
    {
        var x = Tr(Testi);
        var y = Tr<NoEvent>(Test);
    }
}