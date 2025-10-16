// -----------------------------------------------------------------------
// <copyright file="TransitionTest.cs">
//     Created by Frank Listing at 2025/09/30.
// </copyright>
// -----------------------------------------------------------------------

namespace jasmsharp.Tests;

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using jasmsharp;
using TestUtils;

[TestClass]
[TestSubject(typeof(Transition<>))]
public class TransitionTest
{
    [TestMethod]
    public void InitializationViaStandardConstructor()
    {
        var transition = new Transition<TestEvent>(new TransitionEndPoint(new State("abc")), () => true);

        Assert.AreEqual(typeof(TestEvent), transition.EventType);
        Assert.AreEqual("abc", transition.EndPoint.State.Name);
    }

    [TestMethod]
    public void InitializationViaAlternativeConstructor()
    {
        var transition = new Transition<TestEvent>(new State("abc"), () => true);

        Assert.AreEqual(typeof(TestEvent), transition.EventType);
        Assert.AreEqual("abc", transition.EndPoint.State.Name);
    }

    [TestMethod]
    public void IsToFinal()
    {
        var transition1 = new Transition<TestEvent>(new State("abc"), () => true);
        var transition2 = new Transition<TestEvent>(new FinalState(), () => true);

        Assert.IsFalse(transition1.IsToFinal);
        Assert.IsTrue(transition2.IsToFinal);
    }

    [TestMethod]
    public void GetCondition()
    {
        var transition = new Transition<TestEvent>(new State("abc"), () => true);

        Assert.IsTrue(transition.Guard());
    }

    [TestMethod]
    public void GetEndPoint()
    {
        var transition = new Transition<TestEvent>(new State("abc"), () => true);

        Assert.AreEqual("abc", transition.EndPoint.State.Name);
        Assert.AreSame(History.None, transition.EndPoint.History);
    }

    [TestMethod]
    public void UsesDefaultConstructionWithHistory()
    {
        var state = new State("abc");
        var transition = new Transition<TestEvent>(state.History, () => true);

        Assert.AreSame(state, transition.EndPoint.State);
        Assert.AreSame(History.H, transition.EndPoint.History);
    }

    private static Transition<TEvent> Creator<TEvent>(bool returnOfGuard) where TEvent : IEvent =>
        new(new State("abc"), () => returnOfGuard);

    public static IEnumerable<object?[]> IsAllowedData =>
    [
        [Creator<TestEvent>(true), new TestEvent(), true],
        [Creator<TestEvent>(false), new TestEvent(), false],
        [Creator<TestEvent>(true), new NoEvent(), false],
        [Creator<TestEvent>(false), new NoEvent(), false],
        [Creator<TestEvent>(true), new DerivedTestEvent1(), true],
        [Creator<TestEvent>(true), new DerivedTestEvent2(), true],
        [Creator<DerivedTestEvent1>(true), new DerivedTestEvent1(), true],
        [Creator<DerivedTestEvent1>(true), new DerivedTestEvent2(), false],
        [Creator<DerivedTestEvent1>(true), new TestEvent(), false]
    ];

    [TestMethod]
    [DynamicData(nameof(IsAllowedData))]
    public void TestsWhetherATransitionIsAllowed(ITransition transition, IEvent @event, bool expected)
    {
        Assert.AreEqual(expected, transition.IsAllowed(@event));
    }
}