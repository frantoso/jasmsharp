// -----------------------------------------------------------------------
// <copyright file="StateContainerTest.cs">
//     Created by Frank Listing at 2025/10/02.
// </copyright>
// -----------------------------------------------------------------------

namespace jasmsharp.Tests;

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
[TestSubject(typeof(StateContainer))]
public class StateContainerTest
{
    private const string TestState1Name = "test-state-1";
    private const string TestState2Name = "test-state-2";
    private const string FsmName = "fsm";

    [TestMethod]
    public void TestInitialization()
    {
        var container = StateContainerTest.EmptyContainer(new State(StateContainerTest.TestState1Name));

        Assert.AreEqual(StateContainerTest.TestState1Name, container.State.Name);
        Assert.IsFalse(container.Transitions.Any());
        Assert.IsFalse(container.HasChildren);
    }

    [TestMethod]
    public void AddsAChild()
    {
        var fsm = FsmSync.Of(name: StateContainerTest.FsmName, startState: new State("Start").ToContainer());
        var container = StateContainerTest.EmptyContainer(new State(StateContainerTest.TestState1Name)).Child(fsm);

        Assert.IsFalse(container.Transitions.Any());
        Assert.IsTrue(container.HasChildren);
        Assert.AreEqual(1, container.Children.Count);
    }

    [TestMethod]
    public void AddsAListOfChildren()
    {
        var fsm1 = FsmSync.Of(name: StateContainerTest.FsmName, startState: new State("Start").ToContainer());
        var fsm2 = new FsmSync(name: "Otto", startState: new State("Start").ToContainer(), otherStates: []);
        var container =
            StateContainerTest.EmptyContainer(new State(StateContainerTest.TestState1Name))
                .ChildList(
                    new List<FsmSync>
                    {
                        fsm1,
                        fsm2
                    });

        Assert.IsFalse(container.Transitions.Any());
        Assert.IsTrue(container.HasChildren);
        Assert.AreEqual(2, container.Children.Count);
    }

    [TestMethod]
    public void AddsATransitionToAState()
    {
        var container1 = StateContainerTest.EmptyContainer(new State(StateContainerTest.TestState1Name))
            .Transition<TestEvent>(new State(StateContainerTest.TestState2Name));
        var container2 =
            StateContainerTest.EmptyContainer(new State(StateContainerTest.TestState1Name))
                .Transition<TestEvent>(new State(StateContainerTest.TestState2Name), () => true);
        var container3 =
            StateContainerTest.EmptyContainer(new State(StateContainerTest.TestState1Name))
                .Transition<TestEvent>(new State(StateContainerTest.TestState2Name), () => false);

        Assert.IsTrue(container1.Transitions.Any());
        Assert.IsTrue(container1.Transitions[0].IsAllowed(new TestEvent()));
        Assert.IsTrue(container2.Transitions[0].IsAllowed(new TestEvent()));
        Assert.IsFalse(container3.Transitions[0].IsAllowed(new TestEvent()));
    }

    [TestMethod]
    public void AddsATransitionToAnEndpoint()
    {
        var container1 =
            StateContainerTest.EmptyContainer(new State(StateContainerTest.TestState1Name))
                .Transition<TestEvent>(new State(StateContainerTest.TestState2Name).History);
        var container2 =
            StateContainerTest.EmptyContainer(new State(StateContainerTest.TestState1Name)).Transition<TestEvent>(
                new State(StateContainerTest.TestState2Name).History,
                () => true);
        var container3 =
            StateContainerTest.EmptyContainer(new State(StateContainerTest.TestState1Name)).Transition<TestEvent>(
                new State(StateContainerTest.TestState2Name).History,
                () => false);

        Assert.IsTrue(container1.Transitions.Any());
        Assert.IsTrue(container1.Transitions[0].IsAllowed(new TestEvent()));
        Assert.IsTrue(container2.Transitions[0].IsAllowed(new TestEvent()));
        Assert.IsFalse(container3.Transitions[0].IsAllowed(new TestEvent()));
    }

    [TestMethod]
    public void AddsATransitionToAStateAndParameterGuard()
    {
        var container1 =
            StateContainerTest.EmptyContainer(new State(StateContainerTest.TestState1Name))
                .Transition<TestEvent, int>(new State(StateContainerTest.TestState2Name));
        var container2 =
            StateContainerTest.EmptyContainer(new State(StateContainerTest.TestState1Name)).Transition<TestEvent, int>(
                new State(StateContainerTest.TestState2Name),
                data => data == 1);
        var container3 =
            StateContainerTest.EmptyContainer(new State(StateContainerTest.TestState1Name)).Transition<TestEvent, int>(
                new State(StateContainerTest.TestState2Name),
                data => data == 1);

        Assert.IsTrue(container1.Transitions.Any());
        Assert.IsInstanceOfType<Transition<TestEvent, int>>(container1.Transitions[0]);
        Assert.IsTrue(container1.Transitions[0].IsAllowed(new DataEvent<TestEvent, int>(1)));
        Assert.IsTrue(container2.Transitions[0].IsAllowed(new DataEvent<TestEvent, int>(1)));
        Assert.IsFalse(container3.Transitions[0].IsAllowed(new DataEvent<TestEvent, int>(2)));
    }

    [TestMethod]
    public void AddsATransitionToAnEndpointAndParameterGuard()
    {
        var container1 =
            StateContainerTest.EmptyContainer(new State(StateContainerTest.TestState1Name))
                .Transition<TestEvent, int>(new State(StateContainerTest.TestState2Name).History);
        var container2 =
            StateContainerTest.EmptyContainer(new State(StateContainerTest.TestState1Name)).Transition<TestEvent, int>(
                new State(StateContainerTest.TestState2Name).History,
                data => data == 1);
        var container3 =
            StateContainerTest.EmptyContainer(new State(StateContainerTest.TestState1Name)).Transition<TestEvent, int>(
                new State(StateContainerTest.TestState2Name).History,
                data => data == 1);

        Assert.IsTrue(container1.Transitions.Any());
        Assert.IsInstanceOfType<Transition<TestEvent, int>>(container1.Transitions[0]);
        Assert.IsTrue(container1.Transitions[0].IsAllowed(new DataEvent<TestEvent, int>(1)));
        Assert.IsTrue(container2.Transitions[0].IsAllowed(new DataEvent<TestEvent, int>(1)));
        Assert.IsFalse(container3.Transitions[0].IsAllowed(new DataEvent<TestEvent, int>(2)));
    }

    [TestMethod]
    public void AddsATransitionWithoutEventToANestedState()
    {
        var fsm = FsmSync.Of(name: "fsm", startState: new State("Start").ToContainer());
        var container =
            StateContainerTest.EmptyContainer(new State(StateContainerTest.TestState1Name))
                .Child(fsm)
                .Transition(new State(StateContainerTest.TestState2Name).History);

        Assert.IsTrue(container.Transitions.Any());
        Assert.IsTrue(container.HasChildren);
        Assert.AreEqual(typeof(NoEvent), container.Transitions.First().EventType);
    }

    [TestMethod]
    public void AddsATransitionWithoutEvent()
    {
        var container1 = StateContainerTest.EmptyContainer(new State(StateContainerTest.TestState1Name))
            .Transition(new State(StateContainerTest.TestState2Name));
        var container2 =
            StateContainerTest.EmptyContainer(new State(StateContainerTest.TestState1Name))
                .Transition(new State(StateContainerTest.TestState2Name), () => true);
        var container3 =
            StateContainerTest.EmptyContainer(new State(StateContainerTest.TestState1Name))
                .Transition(new State(StateContainerTest.TestState2Name), () => false);
        var container4 =
            StateContainerTest.EmptyContainer(new State(StateContainerTest.TestState1Name))
                .Transition(new State(StateContainerTest.TestState2Name).History);
        var container5 =
            StateContainerTest.EmptyContainer(new State(StateContainerTest.TestState1Name))
                .Transition(new State(StateContainerTest.TestState2Name).History, () => true);
        var container6 =
            StateContainerTest.EmptyContainer(new State(StateContainerTest.TestState1Name))
                .Transition(new State(StateContainerTest.TestState2Name).History, () => false);

        Assert.IsTrue(container1.Transitions.Any());
        Assert.IsTrue(container1.Transitions[0].IsAllowed(new NoEvent()));
        Assert.IsTrue(container2.Transitions[0].IsAllowed(new NoEvent()));
        Assert.IsFalse(container3.Transitions[0].IsAllowed(new NoEvent()));
        Assert.IsTrue(container4.Transitions[0].IsAllowed(new NoEvent()));
        Assert.IsTrue(container5.Transitions[0].IsAllowed(new NoEvent()));
        Assert.IsFalse(container6.Transitions[0].IsAllowed(new NoEvent()));
    }

    [TestMethod]
    public void AddsATransitionWithoutEventAndParameterGuard()
    {
        var container1 =
            StateContainerTest.EmptyContainer(new State(StateContainerTest.TestState1Name))
                .Transition(new State(StateContainerTest.TestState2Name));
        var container2 =
            StateContainerTest.EmptyContainer(new State(StateContainerTest.TestState1Name))
                .Transition<int>(new State(StateContainerTest.TestState2Name), data => data == 1);
        var container3 =
            StateContainerTest.EmptyContainer(new State(StateContainerTest.TestState1Name))
                .Transition<int>(new State(StateContainerTest.TestState2Name), data => data == 1);
        var container4 =
            StateContainerTest.EmptyContainer(new State(StateContainerTest.TestState1Name))
                .Transition(new State(StateContainerTest.TestState2Name).History);
        var container5 =
            StateContainerTest.EmptyContainer(new State(StateContainerTest.TestState1Name))
                .Transition<int>(new State(StateContainerTest.TestState2Name).History, data => data == 1);
        var container6 =
            StateContainerTest.EmptyContainer(new State(StateContainerTest.TestState1Name))
                .Transition<int>(new State(StateContainerTest.TestState2Name).History, data => data == 1);

        Assert.IsTrue(container1.Transitions.Any());
        Assert.IsInstanceOfType<Transition<NoEvent>>(container1.Transitions[0]);
        Assert.IsTrue(container1.Transitions[0].IsAllowed(new DataEvent<NoEvent, int>(1)));
        Assert.IsTrue(container2.Transitions[0].IsAllowed(new DataEvent<NoEvent, int>(1)));
        Assert.IsFalse(container3.Transitions[0].IsAllowed(new DataEvent<NoEvent, int>(2)));
        Assert.IsTrue(container4.Transitions[0].IsAllowed(new DataEvent<NoEvent, int>(1)));
        Assert.IsTrue(container5.Transitions[0].IsAllowed(new DataEvent<NoEvent, int>(1)));
        Assert.IsFalse(container6.Transitions[0].IsAllowed(new DataEvent<NoEvent, int>(2)));
    }

    [TestMethod]
    public void AddsATransitionToFinalState()
    {
        var container1 = StateContainerTest.EmptyContainer(new State(StateContainerTest.TestState1Name))
            .TransitionToFinal<TestEvent>(() => true);
        var container2 = StateContainerTest.EmptyContainer(new State(StateContainerTest.TestState1Name))
            .TransitionToFinal<TestEvent>(() => true);
        var container3 = StateContainerTest.EmptyContainer(new State(StateContainerTest.TestState1Name))
            .TransitionToFinal<TestEvent>(() => false);

        Assert.IsTrue(container1.Transitions.Any());
        Assert.IsTrue(container1.Transitions[0].IsAllowed(new TestEvent()));
        Assert.IsTrue(container2.Transitions[0].IsAllowed(new TestEvent()));
        Assert.IsFalse(container3.Transitions[0].IsAllowed(new TestEvent()));
    }

    [TestMethod]
    public void AddsATransitionToFinalStateWithParameterGuard()
    {
        var container1 = StateContainerTest.EmptyContainer(new State(StateContainerTest.TestState1Name))
            .TransitionToFinal<TestEvent, int>(_ => true);
        var container2 =
            StateContainerTest.EmptyContainer(new State(StateContainerTest.TestState1Name))
                .TransitionToFinal<TestEvent, int>(data => data == 1);
        var container3 =
            StateContainerTest.EmptyContainer(new State(StateContainerTest.TestState1Name))
                .TransitionToFinal<TestEvent, int>(data => data == 1);

        Assert.IsTrue(container1.Transitions.Any());
        Assert.IsInstanceOfType<Transition<TestEvent, int>>(container1.Transitions[0]);
        Assert.IsTrue(container1.Transitions[0].IsAllowed(new DataEvent<TestEvent, int>(1)));
        Assert.IsTrue(container2.Transitions[0].IsAllowed(new DataEvent<TestEvent, int>(1)));
        Assert.IsFalse(container3.Transitions[0].IsAllowed(new DataEvent<TestEvent, int>(2)));
    }

    [TestMethod]
    public void AddsAnEntryActionWithIAction()
    {
        var counter = 5;
        var container =
            StateContainerTest.EmptyContainer(new State(StateContainerTest.TestState1Name))
                .Entry(new jasmsharp.Action<int>(data => counter += data));

        container.OnEntry.Fire(new DataEvent<NoEvent, int>(10));

        Assert.AreEqual(15, counter);
    }

    [TestMethod]
    public void AddsAnEntryActionWithParameter()
    {
        var counter = 5;
        var container = StateContainerTest.EmptyContainer(new State(StateContainerTest.TestState1Name))
            .Entry<int>(data => counter += data);

        container.OnEntry.Fire(new DataEvent<NoEvent, int>(10));

        Assert.AreEqual(15, counter);
    }

    [TestMethod]
    public void AddsAnEntryAction()
    {
        var counter = 5;
        var container = StateContainerTest.EmptyContainer(new State(StateContainerTest.TestState1Name))
            .Entry(() => counter = 42);

        container.OnEntry.Fire(new NoEvent());

        Assert.AreEqual(42, counter);
    }

    [TestMethod]
    public void AddsAnExitActionWithIAction()
    {
        var counter = 5;
        var container =
            StateContainerTest.EmptyContainer(new State(StateContainerTest.TestState1Name))
                .Exit(new jasmsharp.Action<int>(data => counter += data));

        container.OnExit.Fire(new DataEvent<NoEvent, int>(10));

        Assert.AreEqual(15, counter);
    }

    [TestMethod]
    public void AddsAnExitActionWithParameter()
    {
        var counter = 2;
        var container = StateContainerTest.EmptyContainer(new State(StateContainerTest.TestState1Name))
            .Exit<int>(data => counter += data);

        container.OnExit.Fire(new DataEvent<NoEvent, int>(19));

        Assert.AreEqual(21, counter);
    }

    [TestMethod]
    public void AddsAnExitAction()
    {
        var counter = 2;
        var container = StateContainerTest.EmptyContainer(new State(StateContainerTest.TestState1Name))
            .Exit(() => counter = 42);

        container.OnExit.Fire(new NoEvent());

        Assert.AreEqual(42, counter);
    }

    [TestMethod]
    public void AddsADoActionWithIAction()
    {
        var counter = 5;
        var container =
            StateContainerTest.EmptyContainer(new State(StateContainerTest.TestState1Name))
                .DoInState(new jasmsharp.Action<int>(data => counter += data));

        container.OnDoInState.Fire(new DataEvent<NoEvent, int>(10));

        Assert.AreEqual(15, counter);
    }

    [TestMethod]
    public void AddsADoActionWithParameter()
    {
        var counter = 2;
        var container = StateContainerTest.EmptyContainer(new State(StateContainerTest.TestState1Name))
            .DoInState<int>(data => counter += data);

        container.OnDoInState.Fire(new DataEvent<NoEvent, int>(19));

        Assert.AreEqual(21, counter);
    }

    [TestMethod]
    public void AddsADoAction()
    {
        var counter = 2;
        var container = StateContainerTest.EmptyContainer(new State(StateContainerTest.TestState1Name))
            .DoInState(() => counter = 42);

        container.OnDoInState.Fire(new NoEvent());

        Assert.AreEqual(42, counter);
    }

    [TestMethod]
    public void AddsWithChaining()
    {
        var entryCounter = 1;
        var exitCounter = 1;
        var fsm = new FsmSync(name: "fsm", startState: new State("Start").ToContainer(), otherStates: []);
        var container =
            new State(StateContainerTest.TestState1Name)
                .Entry<int>(data => entryCounter += data)
                .Exit<int>(data => exitCounter += data)
                .Child(fsm)
                .Transition<TestEvent>(new State(StateContainerTest.TestState2Name));

        container.OnEntry.Fire(new DataEvent<NoEvent, int>(1));
        container.OnExit.Fire(new DataEvent<NoEvent, int>(2));

        Assert.AreEqual(2, entryCounter);
        Assert.AreEqual(3, exitCounter);
        Assert.IsTrue(container.Transitions.Any());
        Assert.IsTrue(container.HasChildren);
    }

    [TestMethod]
    public void DefaultSettingOfActionsDoesNotThrowAnyException()
    {
        var container = new State("Otto").ToContainer();

        try
        {
            container.OnEntry.Fire(new DataEvent<NoEvent, int>(1));
            container.OnExit.Fire(new DataEvent<NoEvent, int>(3));
        }
        catch (Exception ex)
        {
            Assert.Fail($"Expected no exception, but {ex.GetType().Name} was thrown.");
        }
    }

    private static StateContainer EmptyContainer(State state) =>
        new(
            state,
            [],
            [],
            new NoAction(),
            new NoAction(),
            new NoAction());

    private sealed class TestEvent : Event;
}