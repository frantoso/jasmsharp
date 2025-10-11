// -----------------------------------------------------------------------
// <copyright file="FsmAsyncTest.cs">
//     Created by Frank Listing at 2025/10/07.
// </copyright>
// -----------------------------------------------------------------------

namespace jasmsharp.Tests;

using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
[TestSubject(typeof(FsmAsync))]
public class FsmAsyncTest
{
    private sealed class Event1 : Event;

    private sealed class Event2 : Event;

    [TestMethod]
    public void CreatesANewStateMachineWithOnTrigger()
    {
        var state1 = new State("first");
        var fsm =
            FsmAsync.Of(
                "myFsm",
                state1.ToContainer()
            );
        Assert.IsFalse(fsm.IsRunning);
        Assert.IsFalse(fsm.HasFinished);
        Assert.IsTrue(fsm.CurrentState is InitialState);
        Assert.IsFalse(fsm.CurrentState is FinalState);
        Assert.AreEqual("myFsm", fsm.Name);
    }

    [TestMethod]
    public async Task TriggersEventsAsynchronous()
    {
        var state1 = new State("first");
        var state2 = new State("second");

        var fsm =
            FsmAsync.Of(
                "myFsm",
                state1
                    .Transition<Event1>(state2)
                    .Entry<int>(i =>
                    {
                        Console.WriteLine(i);
                        Thread.Sleep(100);
                    }),
                state2
                    .Transition<Event1>(state2)
                    .Entry<int>(i =>
                    {
                        Console.WriteLine(i);
                        Thread.Sleep(100);
                    })
                    .Transition<Event2>(new FinalState())
            );

        fsm.StateChanged += (sender, args) => Console.WriteLine(
            $"FSM {(sender as Fsm)?.Name}  changed from ${args.OldState.Name} to ${args.NewState.Name}");

        fsm.Start();

        var task1 = Task.Run(
            () => Measure.TimeMillis(() =>
            {
                while (fsm.IsRunning)
                {
                    Thread.Sleep(10);
                }

                Console.WriteLine("fsm task (${Thread.currentThread().threadId()}) has run.");
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
                        Console.WriteLine($"triggering {i}.");
                        fsm.Trigger(evt);
                        Thread.Sleep(10);
                    }
                });

                fsm.Trigger(new Event2());
                Console.WriteLine($"trigger task 1 ({Environment.CurrentManagedThreadId}) has run.");
                return millis;
            },
            this.TestContext.CancellationTokenSource.Token);

        var task3 = Task.Run(
            () => Measure.TimeMillis(() =>
            {
                for (var i = 10; i <= 15; i++)
                {
                    Console.WriteLine($"triggering {i}.");
                    fsm.Trigger(new Event1(), i);
                    Thread.Sleep(1);
                }

                Console.WriteLine($"trigger task 2 ({Environment.CurrentManagedThreadId}) has run.");
            }),
            this.TestContext.CancellationTokenSource.Token);

        Assert.IsGreaterThan(1200, await task1);
        Assert.IsLessThan(500, await task2);
        Assert.IsLessThan(500, await task3);
        Console.WriteLine("test over");
    }

    [TestMethod]
    public void AsyncMachineIsTriggeredSynchronouslyViaDebugInterface()
    {
        var state1 = new State("first");
        var state2 = new State("second");

        var fsm =
            FsmAsync.Of(
                "myFsm",
                state1
                    .Transition<Event1>(state2),
                state2
                    .Transition<Event1>(state1)
                    .Transition<Event2>(new FinalState())
            );

        fsm.Start();

        fsm.DebugInterface.TriggerSync(new Event1());
        Assert.AreEqual(state2, fsm.CurrentState);

        fsm.DebugInterface.TriggerSync(new Event1());
        Assert.AreEqual(state1, fsm.CurrentState);

        fsm.DebugInterface.TriggerSync(new Event1(), 42);
        Assert.AreEqual(state2, fsm.CurrentState);

        fsm.DebugInterface.TriggerSync(new Event2());
        Assert.AreEqual(new FinalState(), fsm.CurrentState);
    }

    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    public TestContext TestContext { get; set; }
}