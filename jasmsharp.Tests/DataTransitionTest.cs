// -----------------------------------------------------------------------
// <copyright file="DataTransitionTest.cs">
//     Created by Frank Listing at 2025/10/05.
// </copyright>
// -----------------------------------------------------------------------

namespace jasmsharp.Tests;

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestUtils;

[TestClass]
[TestSubject(typeof(Transition<,>))]
public class DataTransitionTest
{
    [TestMethod]
    public void InitializationViaStandardConstructor()
    {
        var transition = new Transition<TestEvent, int>(new TransitionEndPoint(new State("abc")), _ => true);

        Assert.AreEqual(typeof(TestEvent), transition.EventType);
        Assert.AreEqual("abc", transition.EndPoint.State.Name);
    }

    [TestMethod]
    public void InitializationViaAlternativeConstructor()
    {
        var transition = new Transition<TestEvent, int>(new State("abc"), _ => true);

        Assert.AreEqual(typeof(TestEvent), transition.EventType);
        Assert.AreEqual("abc", transition.EndPoint.State.Name);
    }

    [TestMethod]
    public void IsToFinal()
    {
        var transition1 = new Transition<TestEvent, int>(new State("abc"), _ => true);
        var transition2 = new Transition<TestEvent, int>(new FinalState(), _ => true);

        Assert.IsFalse(transition1.IsToFinal);
        Assert.IsTrue(transition2.IsToFinal);
    }

    [TestMethod]
    public void GetCondition()
    {
        var transition = new Transition<TestEvent, int>(new TransitionEndPoint(new State("abc")), i => i < 20);

        Assert.IsFalse(transition.Guard(20));
        Assert.IsTrue(transition.Guard(19));
    }

    [TestMethod]
    public void UsesDefaultConstructionWithHistory()
    {
        var state = new State("abc");
        var transition = new Transition<TestEvent, int>(state.History, _ => true);

        Assert.AreSame(state, transition.EndPoint.State);
        Assert.AreSame(History.H, transition.EndPoint.History);
    }

    private static Transition<TEvent, int> Creator<TEvent>(bool returnOfGuard) where TEvent : IEvent =>
        new(new State("abc"), _ => returnOfGuard);

    public static IEnumerable<object?[]> IsAllowedData =>
    [
        [Creator<TestEvent>(true), new DataEvent<TestEvent, int>(1), true],
        [Creator<TestEvent>(true), new DataEvent<TestEvent, short>(1), false],
        [Creator<TestEvent>(true), new DataEvent<TestEvent, double>(1.0), false],
        [Creator<TestEvent>(false), new DataEvent<TestEvent, int>(1), false],
        [Creator<TestEvent>(true), new TestEvent(), false],
        [Creator<TestEvent>(false), new TestEvent(), false],
        [Creator<TestEvent>(true), new NoEvent(), false],
        [Creator<TestEvent>(false), new NoEvent(), false],
        [Creator<TestEvent>(true), new DataEvent<DerivedTestEvent1, int>(42), true],
        [Creator<TestEvent>(true), new DataEvent<DerivedTestEvent2, int>(42), true],
        [Creator<DerivedTestEvent1>(true), new DataEvent<DerivedTestEvent1, int>(42), true],
        [Creator<DerivedTestEvent1>(true), new DataEvent<DerivedTestEvent2, int>(42), false],
        [Creator<DerivedTestEvent1>(true), new DataEvent<TestEvent, int>(42), false]
    ];

    [TestMethod]
    [DynamicData(nameof(DataTransitionTest.IsAllowedData))]
    public void TestsWhetherADataTransitionIsAllowed(ITransition transition, IEvent @event, bool expected)
    {
        Assert.AreEqual(expected, transition.IsAllowed(@event));
    }
}