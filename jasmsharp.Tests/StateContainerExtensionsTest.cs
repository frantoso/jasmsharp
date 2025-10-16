// -----------------------------------------------------------------------
// <copyright file="StateContainerExtensionsTest.cs">
//     Created by Frank Listing at 2025/10/02.
// </copyright>
// -----------------------------------------------------------------------

namespace jasmsharp.Tests;

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
[TestSubject(typeof(StateContainerExtensions))]
public class StateContainerExtensionsTest
{
    private sealed class TestEvent : Event;

    private static FsmSync CreateFsm(string name, EndStateContainer start, List<EndStateContainer>? others = null)
        => new(name, start, others ?? []);

    [TestMethod]
    public void Adds_child()
    {
        var fsm = CreateFsm(StateContainerExtensionsTest.FsmName, new State("Start").ToContainer());
        var container = new State(StateContainerExtensionsTest.TestState1).Child(fsm);

        Assert.IsEmpty(container.Transitions);
        Assert.IsTrue(container.HasChildren);
        Assert.HasCount(1, container.Children);
    }

    [TestMethod]
    public void Adds_list_of_children()
    {
        var fsm1 = CreateFsm(StateContainerExtensionsTest.FsmName, new State("Start").ToContainer());
        var fsm2 = CreateFsm("Otto", new State("Start").ToContainer());
        var container = new State(StateContainerExtensionsTest.TestState1).ChildList([fsm1, fsm2]);

        Assert.IsEmpty(container.Transitions);
        Assert.IsTrue(container.HasChildren);
        Assert.HasCount(2, container.Children);
    }

    [TestMethod]
    public void Adds_transition_to_state()
    {
        var container1 = new State(StateContainerExtensionsTest.TestState1)
            .Transition<TestEvent>(new State(StateContainerExtensionsTest.TestState2));
        var container2 = new State(StateContainerExtensionsTest.TestState1).Transition<TestEvent>(
            new State(StateContainerExtensionsTest.TestState2),
            () => true);
        var container3 = new State(StateContainerExtensionsTest.TestState1).Transition<TestEvent>(
            new State(StateContainerExtensionsTest.TestState2),
            () => false);

        Assert.HasCount(1, container1.Transitions);
        Assert.IsTrue(container1.Transitions[0].IsAllowed(new TestEvent()));
        Assert.IsTrue(container2.Transitions[0].IsAllowed(new TestEvent()));
        Assert.IsFalse(container3.Transitions[0].IsAllowed(new TestEvent()));
    }

    [TestMethod]
    public void Adds_transition_to_endpoint()
    {
        var endpoint = new State(StateContainerExtensionsTest.TestState2).History;
        var container1 = new State(StateContainerExtensionsTest.TestState1)
            .Transition<TestEvent>(endpoint);
        var container2 = new State(StateContainerExtensionsTest.TestState1).Transition<TestEvent>(endpoint, () => true);
        var container3 =
            new State(StateContainerExtensionsTest.TestState1).Transition<TestEvent>(endpoint, () => false);

        Assert.HasCount(1, container1.Transitions);
        Assert.IsTrue(container1.Transitions[0].IsAllowed(new TestEvent()));
        Assert.IsTrue(container2.Transitions[0].IsAllowed(new TestEvent()));
        Assert.IsFalse(container3.Transitions[0].IsAllowed(new TestEvent()));
    }

    [TestMethod]
    public void Adds_transition_to_state_with_parameter_guard()
    {
        var container1 = new State(StateContainerExtensionsTest.TestState1)
            .Transition<TestEvent, int>(new State(StateContainerExtensionsTest.TestState2));
        var container2 = new State(StateContainerExtensionsTest.TestState1)
            .Transition<TestEvent, int>(new State(StateContainerExtensionsTest.TestState2), data => data == 1);
        var container3 = new State(StateContainerExtensionsTest.TestState1)
            .Transition<TestEvent, int>(new State(StateContainerExtensionsTest.TestState2), data => data == 1);

        Assert.HasCount(1, container1.Transitions);
        Assert.IsInstanceOfType<Transition<TestEvent, int>>(container1.Transitions[0]);
        Assert.IsTrue(container1.Transitions[0].IsAllowed(new DataEvent<TestEvent, int>(1)));
        Assert.IsTrue(container2.Transitions[0].IsAllowed(new DataEvent<TestEvent, int>(1)));
        Assert.IsFalse(container3.Transitions[0].IsAllowed(new DataEvent<TestEvent, int>(2)));
    }

    [TestMethod]
    public void Adds_transition_to_endpoint_with_parameter_guard()
    {
        var endpoint = new State(StateContainerExtensionsTest.TestState2).History;
        var container1 = new State(StateContainerExtensionsTest.TestState1)
            .Transition<TestEvent, int>(endpoint);
        var container2 =
            new State(StateContainerExtensionsTest.TestState1)
                .Transition<TestEvent, int>(endpoint, data => data == 1);
        var container3 =
            new State(StateContainerExtensionsTest.TestState1)
                .Transition<TestEvent, int>(endpoint, data => data == 1);

        Assert.HasCount(1, container1.Transitions);
        Assert.IsInstanceOfType<Transition<TestEvent, int>>(container1.Transitions[0]);
        Assert.IsTrue(container1.Transitions[0].IsAllowed(new DataEvent<TestEvent, int>(1)));
        Assert.IsTrue(container2.Transitions[0].IsAllowed(new DataEvent<TestEvent, int>(1)));
        Assert.IsFalse(container3.Transitions[0].IsAllowed(new DataEvent<TestEvent, int>(2)));
    }

    [TestMethod]
    public void Adds_transition_without_event_to_nested_state()
    {
        var fsm = CreateFsm("fsm", new State("Start").ToContainer());
        var container = new State(StateContainerExtensionsTest.TestState1)
            .Child(fsm)
            .Transition(new State(StateContainerExtensionsTest.TestState2).History);

        Assert.HasCount(1, container.Transitions);
        Assert.IsTrue(container.HasChildren);
        Assert.AreEqual(typeof(NoEvent), container.Transitions[0].EventType);
    }

    [TestMethod]
    public void Adds_transition_without_event()
    {
        var endpoint = new State(StateContainerExtensionsTest.TestState2).History;
        var container1 = new State(StateContainerExtensionsTest.TestState1)
            .Transition(new State(StateContainerExtensionsTest.TestState2));
        var container2 = new State(StateContainerExtensionsTest.TestState1)
            .Transition(new State(StateContainerExtensionsTest.TestState2), () => true);
        var container3 = new State(StateContainerExtensionsTest.TestState1)
            .Transition(new State(StateContainerExtensionsTest.TestState2), () => false);
        var container4 = new State(StateContainerExtensionsTest.TestState1)
            .Transition(endpoint);
        var container5 =
            new State(StateContainerExtensionsTest.TestState1)
                .Transition(endpoint, () => true);
        var container6 =
            new State(StateContainerExtensionsTest.TestState1)
                .Transition(endpoint, () => false);

        Assert.IsTrue(container1.Transitions[0].IsAllowed(new NoEvent()));
        Assert.IsTrue(container2.Transitions[0].IsAllowed(new NoEvent()));
        Assert.IsFalse(container3.Transitions[0].IsAllowed(new NoEvent()));
        Assert.IsTrue(container4.Transitions[0].IsAllowed(new NoEvent()));
        Assert.IsTrue(container5.Transitions[0].IsAllowed(new NoEvent()));
        Assert.IsFalse(container6.Transitions[0].IsAllowed(new NoEvent()));
    }

    [TestMethod]
    public void Adds_transition_without_event_with_parameter_guard()
    {
        var endpoint = new State(StateContainerExtensionsTest.TestState2).History;
        var container1 = new State(StateContainerExtensionsTest.TestState1)
            .Transition(new State(StateContainerExtensionsTest.TestState2));
        var container2 = new State(StateContainerExtensionsTest.TestState1)
            .Transition<int>(new State(StateContainerExtensionsTest.TestState2), data => data == 1);
        var container3 = new State(StateContainerExtensionsTest.TestState1)
            .Transition<int>(new State(StateContainerExtensionsTest.TestState2), data => data == 1);
        var container4 = new State(StateContainerExtensionsTest.TestState1)
            .Transition(endpoint);
        var container5 =
            new State(StateContainerExtensionsTest.TestState1)
                .Transition<int>(endpoint, data => data == 1);
        var container6 =
            new State(StateContainerExtensionsTest.TestState1)
                .Transition<int>(endpoint, data => data == 1);

        Assert.IsInstanceOfType<Transition<NoEvent>>(container1.Transitions[0]);
        Assert.IsTrue(container1.Transitions[0].IsAllowed(new DataEvent<NoEvent, int>(1)));
        Assert.IsTrue(container2.Transitions[0].IsAllowed(new DataEvent<NoEvent, int>(1)));
        Assert.IsFalse(container3.Transitions[0].IsAllowed(new DataEvent<NoEvent, int>(2)));
        Assert.IsTrue(container4.Transitions[0].IsAllowed(new DataEvent<NoEvent, int>(1)));
        Assert.IsTrue(container5.Transitions[0].IsAllowed(new DataEvent<NoEvent, int>(1)));
        Assert.IsFalse(container6.Transitions[0].IsAllowed(new DataEvent<NoEvent, int>(2)));
    }

    [TestMethod]
    public void Adds_transition_to_final_state()
    {
        var container1 = new State(StateContainerExtensionsTest.TestState1).TransitionToFinal<TestEvent>(() => true);
        var container2 = new State(StateContainerExtensionsTest.TestState1).TransitionToFinal<TestEvent>(() => true);
        var container3 = new State(StateContainerExtensionsTest.TestState1).TransitionToFinal<TestEvent>(() => false);

        Assert.IsTrue(container1.Transitions[0].IsAllowed(new TestEvent()));
        Assert.IsTrue(container2.Transitions[0].IsAllowed(new TestEvent()));
        Assert.IsFalse(container3.Transitions[0].IsAllowed(new TestEvent()));
    }

    [TestMethod]
    public void Adds_transition_to_final_state_with_parameter_guard()
    {
        var container1 =
            new State(StateContainerExtensionsTest.TestState1).TransitionToFinal<TestEvent, int>(_ => true);
        var container2 =
            new State(StateContainerExtensionsTest.TestState1).TransitionToFinal<TestEvent, int>(data => data == 1);
        var container3 =
            new State(StateContainerExtensionsTest.TestState1).TransitionToFinal<TestEvent, int>(data => data == 1);

        Assert.IsInstanceOfType<Transition<TestEvent, int>>(container1.Transitions[0]);
        Assert.IsTrue(container1.Transitions[0].IsAllowed(new DataEvent<TestEvent, int>(1)));
        Assert.IsTrue(container2.Transitions[0].IsAllowed(new DataEvent<TestEvent, int>(1)));
        Assert.IsFalse(container3.Transitions[0].IsAllowed(new DataEvent<TestEvent, int>(2)));
    }

    [TestMethod]
    public void Adds_entry_action_with_parameter()
    {
        var counter = 5;
        var container = new State(StateContainerExtensionsTest.TestState1).Entry((int data) => counter += data);

        container.OnEntry.Fire(new DataEvent<NoEvent, int>(10));

        Assert.AreEqual(15, counter);
    }

    [TestMethod]
    public void Adds_entry_action()
    {
        var counter = 5;
        var container = new State(StateContainerExtensionsTest.TestState1).Entry(() => counter = 42);

        container.OnEntry.Fire(new NoEvent());

        Assert.AreEqual(42, counter);
    }

    [TestMethod]
    public void Adds_exit_action_with_parameter()
    {
        var counter = 2;
        var container = new State(StateContainerExtensionsTest.TestState1).Exit((int data) => counter += data);

        container.OnExit.Fire(new DataEvent<NoEvent, int>(19));

        Assert.AreEqual(21, counter);
    }

    [TestMethod]
    public void Adds_exit_action()
    {
        var counter = 2;
        var container = new State(StateContainerExtensionsTest.TestState1).Exit(() => counter = 42);

        container.OnExit.Fire(new NoEvent());

        Assert.AreEqual(42, counter);
    }

    [TestMethod]
    public void Adds_do_action_with_parameter()
    {
        var counter = 2;
        var container = new State(StateContainerExtensionsTest.TestState1).DoInState((int data) => counter += data);

        container.OnDoInState.Fire(new DataEvent<NoEvent, int>(19));

        Assert.AreEqual(21, counter);
    }

    [TestMethod]
    public void Adds_do_action()
    {
        var counter = 2;
        var container = new State(StateContainerExtensionsTest.TestState1).DoInState(() => counter = 42);

        container.OnDoInState.Fire(new NoEvent());

        Assert.AreEqual(42, counter);
    }

    private const string TestState1 = "test-state-1";
    private const string TestState2 = "test-state-2";
    private const string FsmName = "fsm";
}