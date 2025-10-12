// -----------------------------------------------------------------------
// <copyright file="FsmSyncTest.cs">
//     Created by Frank Listing at 2025/10/02.
// </copyright>
// -----------------------------------------------------------------------

namespace jasmsharp.Tests;

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class FsmSyncTest
{
    private sealed class Tick : Event;

    private sealed class Event1 : Event;

    private sealed class Event2 : Event;

    private sealed class FirstState() : State("first");

    private sealed class SecondState() : State("second");

    private readonly FirstState theFirstState = new();
    private readonly SecondState theSecondState = new();

    private FsmSync CreateFsm()
    {
        var state1 = this.theFirstState;
        var state2 = this.theSecondState;
        var stateContainer1 = state1.Transition<Event1>(state2);
        var stateContainer2 = state2.Transition<Event1>(new FinalState());

        return new FsmSync(
            "fsm",
            stateContainer1,
            [stateContainer2]);
    }

    private static FsmSync CreateSyncFsm(
        Action<Fsm, IState, IState> onStateChanged,
        Action<Fsm, IState, IEvent, bool> onTriggered)
    {
        var state1 = new State("first");
        var state2 = new State("second");

        var fsm = FsmSync.Of(
            "myFsm",
            state1.Transition<Event1>(state2).Entry<int>(data =>
            {
                Debug.WriteLine(data);
                Thread.Sleep(100);
            }),
            state2
                .Transition<Event1>(state2)
                .Entry<int>(data =>
                {
                    Debug.WriteLine(data);
                    Thread.Sleep(100);
                })
                .Transition<Event2>(new FinalState()));

        fsm.StateChanged += (sender, args) => onStateChanged((sender as Fsm)!, args.OldState, args.NewState);
        fsm.Triggered += (sender, args) => onTriggered((sender as Fsm)!, args.CurrentState, args.Event, args.Handled);

        return fsm;
    }

    [TestMethod]
    public void CreatesANewStateMachine()
    {
        var fsm = new FsmSync(
            "myFsm",
            new FirstState().ToContainer(),
            []);

        Assert.IsFalse(fsm.IsRunning);
        Assert.IsFalse(fsm.HasFinished);
        Assert.IsTrue(fsm.CurrentState is InitialState);
        Assert.IsFalse(fsm.CurrentState is FinalState);
        Assert.AreEqual("myFsm", fsm.Name);
    }

    [TestMethod]
    public void CreatesANewStateMachineWithOnTrigger()
    {
        var fsm = FsmSync.Of(
            "myFsm",
            new FirstState().ToContainer());

        Assert.IsFalse(fsm.IsRunning);
        Assert.IsFalse(fsm.HasFinished);
        Assert.IsTrue(fsm.CurrentState is InitialState);
        Assert.IsFalse(fsm.CurrentState is FinalState);
        Assert.AreEqual("myFsm", fsm.Name);
    }

    [TestMethod]
    public void StartsTheStateMachine()
    {
        var fsm = this.CreateFsm();
        Assert.IsFalse(fsm.IsRunning);

        fsm.Start();

        Assert.IsTrue(fsm.IsRunning);
        Assert.IsFalse(fsm.HasFinished);
        Assert.AreSame(this.theFirstState, fsm.CurrentState);
        Assert.IsFalse(fsm.CurrentState is InitialState);
        Assert.IsFalse(fsm.CurrentState is FinalState);
    }

    [TestMethod]
    public void TriggerChangesToTheNextState()
    {
        var fsm = this.CreateFsm();
        fsm.Start();
        fsm.Trigger(new Event1());

        Assert.IsTrue(fsm.IsRunning);
        Assert.IsFalse(fsm.HasFinished);
        Assert.AreSame(this.theSecondState, fsm.CurrentState);
        Assert.IsFalse(fsm.CurrentState is InitialState);
        Assert.IsFalse(fsm.CurrentState is FinalState);
    }

    [TestMethod]
    public void TriggerChangesToTheNextStateWithData()
    {
        var fsm = this.CreateFsm();
        fsm.Start();
        fsm.Trigger(new Event1(), 2);

        Assert.IsTrue(fsm.IsRunning);
        Assert.IsFalse(fsm.HasFinished);
        Assert.AreSame(this.theSecondState, fsm.CurrentState);
        Assert.IsFalse(fsm.CurrentState is InitialState);
        Assert.IsFalse(fsm.CurrentState is FinalState);
    }

    [TestMethod]
    public void TriggerToFinalStopsTheStateMachine()
    {
        var fsm = this.CreateFsm();
        fsm.Start();
        fsm.Trigger(new Event1());
        fsm.Trigger(new Event1());

        Assert.IsFalse(fsm.IsRunning);
        Assert.IsTrue(fsm.HasFinished);
        Assert.IsFalse(fsm.CurrentState is InitialState);
        Assert.IsTrue(fsm.CurrentState is FinalState);
    }

    [TestMethod]
    public void UsingPredefinedNoEventOnNormalTransitionThrowsException()
    {
        var fsm = this.CreateFsm();

        fsm.Start();

        Assert.ThrowsExactly<FsmException>(() => fsm.Trigger(new NoEvent()));
    }

    [TestMethod]
    public void DoActionTriggersTheDoActionInStateWithParameter()
    {
        var state1 = new State("first");
        var doInStateResult = 0;
        var fsm = FsmSync.Of(
            "myFsm",
            state1.DoInState<int>(data => doInStateResult = data));

        fsm.Start(42);
        fsm.DoAction(22);

        Assert.AreEqual(22, doInStateResult);
    }

    [TestMethod]
    public void DoActionTriggersTheDoActionInStateWithoutParameter()
    {
        var state1 = new State("first");
        var doInStateResult = 0;
        var fsm = FsmSync.Of(
            "myFsm",
            state1.DoInState(() => doInStateResult = 13));

        fsm.Start(42);
        fsm.DoAction();

        Assert.AreEqual(13, doInStateResult);
    }

    [TestMethod]
    public void AddsADestinationOnlyStateToTheStatesList()
    {
        var state1 = new State("first");
        var state2 = new State("second");
        var state3 = new State("third");

        var fsm = FsmSync.Of(
            "myFsm",
            state1
                .Transition<Event1>(state2)
                .Entry<int>(data =>
                {
                    Console.WriteLine(data);
                    Thread.Sleep(100);
                }),
            state2
                .Transition<Event1>(state2)
                .Transition<Event2>(state3)
                .Entry<int>(data =>
                {
                    Console.WriteLine(data);
                    Thread.Sleep(100);
                }));

        Assert.IsTrue(fsm.States.Select(x => x.State).Contains(state3));
    }

    [TestMethod]
    public void AddsADestinationOnlyStateOnlyOnceToTheStatesList()
    {
        var state1 = new State("first");
        var state2 = new State("second");
        var state3 = new State("third");

        var fsm = FsmSync.Of(
            "myFsm",
            state1
                .Transition<Event1>(state2)
                .Transition<Event2>(state3)
                .Entry<int>(data =>
                {
                    Console.WriteLine(data);
                    Thread.Sleep(100);
                }),
            state2
                .Transition<Event1>(state2)
                .Transition<Event2>(state3)
                .Entry<int>(data =>
                {
                    Console.WriteLine(data);
                    Thread.Sleep(100);
                }));

        Assert.AreEqual(
            1,
            fsm.States
                .Select(x => x.State)
                .Count(x => x == state3));
    }

    [TestMethod]
    public void AddsMoreThanOneDestinationOnlyStateToTheStatesList()
    {
        var state1 = new State("first");
        var state2 = new State("second");
        var state3 = new State("third");
        var state4 = new State("fourth");

        var fsm = FsmSync.Of(
            "myFsm",
            state1
                .Transition<Event1>(state2)
                .Transition<Event2>(state4)
                .Entry<int>(data =>
                {
                    Console.WriteLine(data);
                    Thread.Sleep(100);
                }),
            state2
                .Transition<Event1>(state2)
                .Transition<Event2>(state3)
                .Entry<int>(data =>
                {
                    Console.WriteLine(data);
                    Thread.Sleep(100);
                }));

        Assert.IsTrue(fsm.States.Select(x => x.State).Contains(state3));
        Assert.IsTrue(fsm.States.Select(x => x.State).Contains(state4));
    }

    [TestMethod]
    public async Task TriggersEventsSynchronously()
    {
        var fsm = FsmSyncTest.CreateSyncFsm(
            (machine, from, to) => Debug.WriteLine($"FSM {machine.Name} changed from {from.Name} to {to.Name}"),
            (machine, state, evt, handled) => Debug.WriteLine($"{machine} - {state} - {evt} - {handled}"));

        fsm.Start(42);

        var task1 = Task.Run(
            () => Measure.TimeMillis(() =>
            {
                while (fsm.IsRunning)
                {
                    Thread.Sleep(10);
                }
            }),
            this.TestContext.CancellationTokenSource.Token);

        var evt = new Event1();
        var task2 = Task.Run(
            () =>
            {
                var millis = Measure.TimeMillis(() =>
                {
                    for (var i = 0; i <= 5; i++)
                    {
                        fsm.Trigger(evt, i);
                        Thread.Sleep(10);
                    }
                });

                fsm.Trigger(new Event2());
                return millis;
            },
            this.TestContext.CancellationTokenSource.Token);

        var task3 = Task.Run(
            () => Measure.TimeMillis(() =>
            {
                for (var i = 10; i <= 15; i++)
                {
                    fsm.Trigger(new Event1(), i);
                    Thread.Sleep(1);
                    Debug.WriteLine("Tick 3");
                }
            }),
            this.TestContext.CancellationTokenSource.Token);

        Assert.IsGreaterThan(1200, await task1);
        Assert.IsGreaterThan(1000, await task2);
        Assert.IsGreaterThan(1000, await task3);
    }

    [TestMethod]
    public void CallingInvalidStateHandlerThrowsFsmException()
    {
        var fsm = FsmSyncTest.CreateSyncFsm((_, _, _) => throw new InvalidCastException("Test"), (_, _, _, _) => { });

        Assert.ThrowsExactly<FsmException>(() => fsm.Start());
    }

    [TestMethod]
    public void CallingInvalidTriggerHandlerThrowsFsmException()
    {
        var fsm = FsmSyncTest.CreateSyncFsm((_, _, _) => { }, (_, _, _, _) => throw new InvalidDataException("Test"));

        Assert.ThrowsExactly<FsmException>(() => fsm.Start());
    }

    [TestMethod]
    public void UsingNoEventFromANonNestedStateThrowsException()
    {
        var state1 = new State("first");
        var state2 = new State("second");
        var state3 = new State("third");

        Assert.ThrowsExactly<FsmException>(() => FsmSync.Of(
            "myFsm",
            state1.Transition<Event1>(state2),
            state2
                .Transition(state2)
                .Transition<Event2>(state3),
            state3.Transition<Event1>(state1)));
    }

    [TestMethod]
    public void CallOfResumeStartsTheFsmWithTheSpecifiedState()
    {
        var state1 = new State("first");
        var state2 = new State("second");
        var state3 = new State("third");

        var fsm = FsmSync.Of(
            "myFsm",
            state1.Transition<Event1>(state2),
            state2
                .Transition<Event1>(state2)
                .Transition<Event2>(state3),
            state3.Transition<Event1>(state1));

        fsm.DebugInterface.Resume(state2);

        Assert.AreEqual(state2, fsm.CurrentState);
    }

    [TestClass]
    public class NestedFsmTests
    {
        private static readonly State State1Child1 = new("state1Child1");
        private static readonly State State2Child1 = new("state2Child1");

        private static readonly State State1Child2 = new("state1Child2");
        private static readonly State State2Child2 = new("state2Child2");
        private static readonly State State3Child2 = new("state3Child2");
        private static readonly State State4Child2 = new("state4Child2");

        private static readonly State State1Main = new("state1Main");
        private static readonly State State2Main = new("state2Main");
        private static readonly State State3Main = new("state3Main");

        private class TestData(int number)
        {
            public int Number { get; set; } = number;
        }

        private readonly FsmSync childMachine1 = FsmSync.Of(
            "child1",
            State1Child1
                .Entry<TestData>(data => data?.Let(d => d.Number = 42))
                .Transition<Tick>(State2Child1),
            State2Child1
                .TransitionToFinal<Tick>());

        private readonly FsmSync childMachine2 = FsmSync.Of(
            "child2",
            State1Child2
                .Transition<Tick>(State2Child2),
            State2Child2
                .Transition<Tick>(State3Child2),
            State3Child2
                .Transition<Tick>(State4Child2),
            State4Child2
                .TransitionToFinal<Tick>());

        private FsmSync GetMainMachine() => FsmSync.Of(
            "main",
            State1Main
                .Transition<Tick>(State2Main),
            State2Main
                .ChildList([this.childMachine1, this.childMachine2])
                .Transition(State3Main),
            State3Main
                .Transition<Tick>(State1Main));

        [TestMethod]
        public void TestUseOfChildren()
        {
            var mainMachine = this.GetMainMachine();
            mainMachine.Start();

            Assert.AreEqual(State1Main, mainMachine.CurrentState);
            Assert.IsFalse(this.childMachine1.IsRunning);
            Assert.IsFalse(this.childMachine2.IsRunning);

            var data = new TestData(0);
            mainMachine.Trigger(new Tick(), data);

            Assert.AreEqual(42, data.Number);
            Assert.AreEqual(State2Main, mainMachine.CurrentState);
            Assert.IsTrue(this.childMachine1.IsRunning);
            Assert.IsTrue(this.childMachine2.IsRunning);
            Assert.AreEqual(State1Child1, this.childMachine1.CurrentState);
            Assert.AreEqual(State1Child2, this.childMachine2.CurrentState);

            mainMachine.Trigger(new Tick());

            Assert.AreEqual(State2Main, mainMachine.CurrentState);
            Assert.AreEqual(State2Child1, this.childMachine1.CurrentState);
            Assert.AreEqual(State2Child2, this.childMachine2.CurrentState);

            mainMachine.Trigger(new Tick());

            Assert.AreEqual(State2Main, mainMachine.CurrentState);
            Assert.IsFalse(this.childMachine1.IsRunning);
            Assert.AreEqual(State3Child2, this.childMachine2.CurrentState);

            mainMachine.Trigger(new Tick());

            Assert.AreEqual(State2Main, mainMachine.CurrentState);
            Assert.IsFalse(this.childMachine1.IsRunning);
            Assert.AreEqual(State4Child2, this.childMachine2.CurrentState);

            mainMachine.Trigger(new Tick());

            Assert.AreEqual(State3Main, mainMachine.CurrentState);
            Assert.IsFalse(this.childMachine1.IsRunning);
            Assert.IsFalse(this.childMachine2.IsRunning);

            mainMachine.Trigger(new Tick());

            Assert.AreEqual(State1Main, mainMachine.CurrentState);
            Assert.IsFalse(this.childMachine1.IsRunning);
            Assert.IsFalse(this.childMachine2.IsRunning);

            mainMachine.Trigger(new Tick());

            Assert.AreEqual(State2Main, mainMachine.CurrentState);
            Assert.IsTrue(this.childMachine1.IsRunning);
            Assert.IsTrue(this.childMachine2.IsRunning);
            Assert.AreEqual(State1Child1, this.childMachine1.CurrentState);
            Assert.AreEqual(State1Child2, this.childMachine2.CurrentState);
        }

        [TestMethod]
        public void CreatesAStateTree()
        {
            var mainMachine = this.GetMainMachine();
            mainMachine.Start();
            mainMachine.Trigger(new Tick());
            mainMachine.Trigger(new Tick());

            var tree = mainMachine.CurrentStateTree;

            Assert.AreEqual(State2Main, tree.State);
            Assert.AreEqual(State2Child1, tree.Children[0].State);
            Assert.AreEqual(State2Child2, tree.Children[1].State);
            Assert.IsEmpty(tree.Children[0].Children);
            Assert.IsEmpty(tree.Children[1].Children);

            var containerTree = mainMachine.CurrentStateContainerTree;

            Assert.AreEqual(State2Main, containerTree.Container.State);
            Assert.AreEqual(State2Child1, containerTree.Children[0].Container.State);
            Assert.AreEqual(State2Child2, containerTree.Children[1].Container.State);
            Assert.IsEmpty(containerTree.Children[0].Children);
            Assert.IsEmpty(containerTree.Children[1].Children);
        }
    }

    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    public TestContext TestContext { get; set; }
}