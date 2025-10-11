// -----------------------------------------------------------------------
// <copyright file="HistoryExample.cs">
//     Created by Frank Listing at 2025/10/05.
// </copyright>
// -----------------------------------------------------------------------

namespace jasmsharp.Tests.Examples;

using System;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
[TestSubject(typeof(FsmSync))]
public class HistoryExample
{
    private class Break : Event;

    private class Continue : Event;

    private class ContinueDeep : Event;

    private class Next : Event;

    private class Restart : Event;

    /**
    * The composite state machine of the L2-Preparing state.
    */
    private class L2Preparing
    {
        public State StateL3P1 { get; } = new("L3-P1");
        public State StateL3P2 { get; } = new("L3-P2");
        public State StateL3P3 { get; } = new("L3-P3");
        private State StateL3P4 { get; } = new("L3-P4");

        public FsmSync Machine { get; }

        public L2Preparing()
        {
            this.Machine = FsmSync.Of(
                "L2-Preparing",
                this.StateL3P1
                    .Transition<Next>(this.StateL3P2)
                    .Entry(P1Entry),
                this.StateL3P2
                    .Transition<Next>(this.StateL3P3)
                    .Entry(P2Entry),
                this.StateL3P3
                    .Transition<Next>(this.StateL3P4)
                    .Entry(P3Entry),
                this.StateL3P4
                    .TransitionToFinal<Next>()
                    .Entry(P4Entry)
            );
        }

        private static void P1Entry() => Console.WriteLine("L3-P1Entry");
        private static void P2Entry() => Console.WriteLine("L3-P2Entry");
        private static void P3Entry() => Console.WriteLine("L3-P3Entry");
        private static void P4Entry() => Console.WriteLine("L3-P4Entry");
    }

    private class L2Working
    {
        private State StateL3W1 { get; } = new("L3-W1");
        private State StateL3W2 { get; } = new("L3-W2");
        public State StateL3W3 { get; } = new("L3-W3");
        public State StateL3W4 { get; } = new("L3-W4");
        private State StateL3W5 { get; } = new("L3-W5");

        public FsmSync Machine { get; }

        public L2Working()
        {
            this.Machine =
                FsmSync.Of(
                    "L2-Working",
                    this.StateL3W1
                        .Transition<Next>(this.StateL3W2)
                        .Entry(W1Entry),
                    this.StateL3W2
                        .Transition<Next>(this.StateL3W3)
                        .Entry(W2Entry),
                    this.StateL3W3
                        .Transition<Next>(this.StateL3W4)
                        .Entry(W3Entry),
                    this.StateL3W4
                        .Transition<Next>(this.StateL3W5)
                        .Entry(W4Entry),
                    this.StateL3W5
                        .TransitionToFinal<Next>()
                        .Entry(W5Entry)
                );
        }

        private static void W5Entry() => Console.WriteLine("L3-W5Entry");
        private static void W4Entry() => Console.WriteLine("L3-W4Entry");
        private static void W3Entry() => Console.WriteLine("L3W-3Entry");
        private static void W2Entry() => Console.WriteLine("L3W-2Entry");
        private static void W1Entry() => Console.WriteLine("L3W-1Entry");
    }

    class Working
    {
        private State StateL2Initializing { get; } = new("L2-Initializing");
        public State StateL2Preparing { get; } = new("L2-Preparing");
        public State StateL2Working { get; } = new("L2-Working");
        public L2Preparing L2Preparing { get; } = new();
        public L2Working L2Working { get; } = new();

        public FsmSync Machine { get; }

        public Working()
        {
            this.Machine =
                FsmSync.Of(
                    "Working",
                    this.StateL2Initializing
                        .Transition<Next>(this.StateL2Preparing)
                        .Entry(L2InitializingEntry),
                    this.StateL2Preparing
                        .Transition(this.StateL2Working)
                        .Entry(L2PreparingEntry)
                        .Child(this.L2Preparing.Machine),
                    this.StateL2Working
                        .TransitionToFinal<Next>()
                        .Entry(L2WorkingEntry)
                        .Child(this.L2Working.Machine)
                );
        }

        private static void L2InitializingEntry() => Console.WriteLine("L2-InitializingEntry");
        private static void L2PreparingEntry() => Console.WriteLine("L2-PreparingEntry");
        private static void L2WorkingEntry() => Console.WriteLine("L2-WorkingEntry");
    }

    class Main
    {
        private State StateInitializing { get; } = new("Main-Initializing");
        public State StateWorking { get; } = new("Main-Working");
        public State StateHandlingError { get; } = new("Main-HandlingError");
        private State StateFinalizing { get; } = new("Main-Finalizing");

        public Working Working { get; } = new();

        public FsmSync Machine { get; }

        /**
         * Starts the behavior of the state machine.
         */
        public void Start() => this.Machine.Start();

        /**
         * Triggers the specified event.
         * @param event The trigger.
         */
        public void Trigger(Event @event) => this.Machine.Trigger(@event);

        public Main()
        {
            this.Machine =
                FsmSync.Of(
                    "Main",
                    this.StateInitializing
                        .Transition<Next>(this.StateWorking),
                    this.StateWorking
                        .Transition<Break>(this.StateHandlingError)
                        .Transition(this.StateFinalizing)
                        .Entry(WorkingEntry)
                        .Child(this.Working.Machine),
                    this.StateHandlingError
                        .Transition<Restart>(this.StateWorking)
                        .Transition<Continue>(this.StateWorking.History)
                        .Transition<ContinueDeep>(this.StateWorking.DeepHistory)
                        .Transition<Next>(this.StateFinalizing)
                        .TransitionToFinal<Break>()
                        .Entry(HandlingErrorEntry),
                    this.StateFinalizing
                        .Transition<Next>(this.StateInitializing)
                        .Entry(FinalizingEntry)
                );
        }

        private static void FinalizingEntry() => Console.WriteLine("FinalizingEntry");
        private static void HandlingErrorEntry() => Console.WriteLine("HandlingErrorEntry");
        private static void WorkingEntry() => Console.WriteLine("WorkingEntry");
    }

    [TestMethod]
    public void TestHistory()
    {
        var main = new Main();
        var working = main.Working;
        var l2Preparing = working.L2Preparing;
        var l2Working = working.L2Working;

        main.Start();

        main.Trigger(new Next());
        main.Trigger(new Next());
        main.Trigger(new Next());
        main.Trigger(new Next());

        Assert.AreEqual(main.StateWorking, main.Machine.CurrentState);
        Assert.AreEqual(working.StateL2Preparing, working.Machine.CurrentState);
        Assert.AreEqual(l2Preparing.StateL3P3, l2Preparing.Machine.CurrentState);
        Assert.IsFalse(l2Working.Machine.IsRunning);

        main.Trigger(new Break());

        Assert.AreEqual(main.StateHandlingError, main.Machine.CurrentState);
        Assert.AreEqual(working.StateL2Preparing, working.Machine.CurrentState);
        Assert.AreEqual(l2Preparing.StateL3P3, l2Preparing.Machine.CurrentState);
        Assert.IsFalse(l2Working.Machine.IsRunning);

        main.Trigger(new Continue());

        // restore Working, but start L2Preparing from the beginning
        Assert.AreEqual(main.StateWorking, main.Machine.CurrentState);
        Assert.AreEqual(working.StateL2Preparing, working.Machine.CurrentState);
        Assert.AreEqual(l2Preparing.StateL3P1, l2Preparing.Machine.CurrentState);
        Assert.IsFalse(l2Working.Machine.IsRunning);

        main.Trigger(new Next());

        // normal continuation
        Assert.AreEqual(main.StateWorking, main.Machine.CurrentState);
        Assert.AreEqual(working.StateL2Preparing, working.Machine.CurrentState);
        Assert.AreEqual(l2Preparing.StateL3P2, l2Preparing.Machine.CurrentState);
        Assert.IsFalse(l2Working.Machine.IsRunning);
    }

    [TestMethod]
    public void TestDeepHistory()
    {
        var main = new Main();
        var working = main.Working;
        var l2Preparing = working.L2Preparing;
        var l2Working = working.L2Working;

        main.Start();

        main.Trigger(new Next());
        main.Trigger(new Next());
        main.Trigger(new Next());
        main.Trigger(new Next());
        main.Trigger(new Next());
        main.Trigger(new Next());
        main.Trigger(new Next());
        main.Trigger(new Next());

        Assert.AreEqual(main.StateWorking, main.Machine.CurrentState);
        Assert.AreEqual(working.StateL2Working, working.Machine.CurrentState);
        Assert.AreEqual(new FinalState(), l2Preparing.Machine.CurrentState);
        Assert.AreEqual(l2Working.StateL3W3, l2Working.Machine.CurrentState);
        Assert.IsFalse(l2Preparing.Machine.IsRunning);

        main.Trigger(new Break());

        Assert.AreEqual(main.StateHandlingError, main.Machine.CurrentState);
        Assert.AreEqual(working.StateL2Working, working.Machine.CurrentState);
        Assert.AreEqual(new FinalState(), l2Preparing.Machine.CurrentState);
        Assert.AreEqual(l2Working.StateL3W3, l2Working.Machine.CurrentState);
        Assert.IsFalse(l2Preparing.Machine.IsRunning);

        main.Trigger(new ContinueDeep());

        // restore the states of the whole hierarchy
        Assert.AreEqual(main.StateWorking, main.Machine.CurrentState);
        Assert.AreEqual(working.StateL2Working, working.Machine.CurrentState);
        Assert.AreEqual(new FinalState(), l2Preparing.Machine.CurrentState);
        Assert.AreEqual(l2Working.StateL3W3, l2Working.Machine.CurrentState);
        Assert.IsFalse(l2Preparing.Machine.IsRunning);

        main.Trigger(new Next());

        // normal continuation
        Assert.AreEqual(main.StateWorking, main.Machine.CurrentState);
        Assert.AreEqual(working.StateL2Working, working.Machine.CurrentState);
        Assert.AreEqual(new FinalState(), l2Preparing.Machine.CurrentState);
        Assert.AreEqual(l2Working.StateL3W4, l2Working.Machine.CurrentState);
        Assert.IsFalse(l2Preparing.Machine.IsRunning);
    }

    [TestMethod]
    public void CreatesAStateTree()
    {
        var main = new Main();
        var working = main.Working;
        var l2Working = working.L2Working;
        main.Start();
        main.Trigger(new Next());
        main.Trigger(new Next());
        main.Trigger(new Next());
        main.Trigger(new Next());
        main.Trigger(new Next());
        main.Trigger(new Next());
        main.Trigger(new Next());
        main.Trigger(new Next());

        var tree = main.Machine.CurrentStateTree;

        Assert.AreEqual(main.StateWorking, tree.State);
        Assert.AreEqual(working.StateL2Working, tree.Children.First().State);
        Assert.AreEqual(
            l2Working.StateL3W3,
            tree.Children
                .First()
                .Children
                .First()
                .State
        );

        var containerTree = main.Machine.CurrentStateContainerTree;

        Assert.AreEqual(main.StateWorking, containerTree.Container.State);
        Assert.AreEqual(
            working.StateL2Working,
            containerTree.Children
                .First()
                .Container.State
        );
        Assert.AreEqual(
            l2Working.StateL3W3,
            containerTree.Children
                .First()
                .Children
                .First()
                .Container.State
        );
    }
}