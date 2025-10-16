// -----------------------------------------------------------------------
// <copyright file="StateContainerBaseTest.cs">
//     Created by Frank Listing at 2025/10/03.
// </copyright>
// -----------------------------------------------------------------------

namespace jasmsharp.Tests;

using JetBrains.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
[TestSubject(typeof(StateContainer))]
public class StateContainerBaseTest
{
    private sealed class TestEvent : Event;

    private sealed class StopEvent : Event;

    private sealed class FinishEvent : Event;

    private static StateContainer EmptyContainer(State state) =>
        new(
            state,
            [],
            [],
            new NoAction(),
            new NoAction(),
            new NoAction());

    private static StateContainer GetContainerWithChild(System.Action entryAction)
    {
        var fsm = FsmSync.Of(name: "fsmChild", startState: new State("Start").ToContainer());
        var container = new State(TestState1Name).Child(fsm).Entry(entryAction);
        container.Start();
        return container;
    }

    [TestMethod]
    public void StartingANormalStateWithParameters()
    {
        var counter = 1;
        var container = new State(TestState1Name).Entry<int>(data => counter += data);

        container.Start(42);

        Assert.AreEqual(43, counter);
    }

    [TestMethod]
    public void StartingANormalStateWithoutParameters()
    {
        var counter = 1;
        var container = new State(TestState1Name).Entry(() => counter = 42);
        Assert.AreEqual(1, counter);

        container.Start();

        Assert.AreEqual(42, counter);
    }

    [TestMethod]
    public void StartingANormalStateWithHistoryEndsInNormalStart()
    {
        var counter = 1;
        var container = new State(TestState1Name).ToContainer().Entry(() => counter = 42);
        Assert.AreEqual(1, counter);

        container.Start(new NoEvent(), History.H);

        Assert.AreEqual(42, counter);
    }

    [TestMethod]
    public void StartingAParentStateWithHistoryDoesNotCallEntry()
    {
        var counter = 1;
        var container = GetContainerWithChild(() => counter = 42);
        counter = 0; // reset counter

        container.Start(new NoEvent(), History.H);

        Assert.AreEqual(0, counter);
    }

    [TestMethod]
    public void StartingANormalStateWithDeepHistoryEndsInNormalStart()
    {
        var counter = 1;
        var container = new State(TestState1Name).ToContainer().Entry(() => counter = 42);
        Assert.AreEqual(1, counter);

        container.Start(new NoEvent(), History.Hd);

        Assert.AreEqual(42, counter);
    }

    [TestMethod]
    public void StartingAParentStateWithDeepHistoryDoesNotCallEntry()
    {
        var counter = 1;
        var container = GetContainerWithChild(() => counter = 42);
        counter = 0; // reset counter

        container.Start(new NoEvent(), History.Hd);

        Assert.AreEqual(0, counter);
    }

    [TestMethod]
    public void ProcessesTransitions()
    {
        var container =
            EmptyContainer(new State(TestState1Name))
                .Transition<TestEvent>(new State(TestState2Name))
                .Transition<StopEvent>(new State(TestState1Name));

        var result = container.Trigger(new TestEvent());

        Assert.IsTrue(result.Handled);
        Assert.AreEqual(TestState2Name, result.EndPoint?.State.Name);
    }

    [TestMethod]
    public void ProcessesTransitionsWithUnknownEvent()
    {
        var container =
            EmptyContainer(new State(TestState1Name))
                .Transition<StopEvent>(new State(TestState2Name));

        var result = container.Trigger(new TestEvent());

        Assert.IsFalse(result.Handled);
        Assert.IsNull(result.EndPoint);
    }

    [TestMethod]
    public void ProcessesTransitionsOnAChildMachine()
    {
        var container =
            EmptyContainer(new State(TestState1Name))
                .Transition<StopEvent>(new State(TestState2Name));

        var result = container.Trigger(new TestEvent());

        Assert.IsFalse(result.Handled);
        Assert.IsNull(result.EndPoint);
    }

    private static StateContainer GetComplexContainer()
    {
        var childContainer =
            new State("ChildStart")
                .Transition<TestEvent>(new State(TestState2Name))
                .TransitionToFinal<FinishEvent>();

        var fsm = FsmSync.Of("fsmChild", childContainer);

        var container =
            new State(TestState1Name)
                .Transition<StopEvent>(new State(TestState2Name))
                .Child(fsm);

        return container;
    }

    [TestMethod]
    public void TestTriggerChild_ChildHandlesTransition()
    {
        var container = StateContainerBaseTest.GetComplexContainer();
        container.Start(new NoEvent(), History.None);

        var result = container.Trigger(new TestEvent());

        Assert.IsTrue(result.Handled);
        Assert.IsNull(result.EndPoint);
    }

    [TestMethod]
    public void TestTriggerChild_ChildrenFinish()
    {
        var container = StateContainerBaseTest.GetComplexContainer();
        container.Start(new NoEvent(), History.None);

        var result = container.Trigger(new FinishEvent());

        Assert.IsFalse(result.Handled);
        Assert.IsNull(result.EndPoint);
    }

    [TestMethod]
    public void TestTriggerChildWithData_ChildrenFinish()
    {
        var container = StateContainerBaseTest.GetComplexContainer();
        container.Start(new NoEvent(), History.None);

        var result = container.Trigger(new DataEvent<FinishEvent, int>(42));

        Assert.IsFalse(result.Handled);
        Assert.IsNull(result.EndPoint);
    }

    [TestMethod]
    public void TestTriggerChild_ChildDoesNotHandleTransition()
    {
        var container = StateContainerBaseTest.GetComplexContainer();
        container.Start(new NoEvent(), History.None);

        var result = container.Trigger(new StopEvent());

        Assert.IsTrue(result.Handled);
        Assert.AreEqual(TestState2Name, result.EndPoint?.State.Name);
    }

    private const string TestState1Name = "test-state-1";
    private const string TestState2Name = "test-state-2";
}