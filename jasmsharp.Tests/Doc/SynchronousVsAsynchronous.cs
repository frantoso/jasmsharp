// -----------------------------------------------------------------------
// <copyright file="SynchronousVsAsynchronous.cs">
//     Created by Frank Listing at 2025/10/16.
// </copyright>
// -----------------------------------------------------------------------

namespace jasmsharp.Tests.Doc;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class SynchronousVsAsynchronous
{
    private readonly ConcurrentQueue<string> output = [];
    private readonly State state1 = new("first");
    private readonly State state2 = new("second");

    public required TestContext TestContext { get; set; }

    [TestMethod]
    public async Task RunSyncVsAsync()
    {
        var outputAsync = await this.RunFsm(this.CreateFsmAsync());
        Assert.HasCount(0, outputAsync.TakeLast(10).Where(i => i.StartsWith('+')));

        Console.WriteLine();

        var outputSync = await this.RunFsm(this.CreateFsmSync());
        Assert.IsGreaterThanOrEqualTo(2, outputSync.TakeLast(5).Count(i => i.StartsWith('+')));
    }

    private FsmSync CreateFsmSync() =>
        FsmSync.Of(
            "MySyncFsm",
            this.state1
                .Transition<Event1>(this.state2)
                .Entry<int>(i =>
                {
                    this.output.Enqueue($"- {i}");
                    Thread.Sleep(100);
                }),
            this.state2
                .Entry<int>(i =>
                {
                    this.output.Enqueue($"- {i}");
                    Thread.Sleep(100);
                })
                .Transition<Event1>(this.state2)
                .TransitionToFinal<Event2>()
        );

    private FsmAsync CreateFsmAsync() =>
        FsmAsync.Of(
            "MySyncFsm",
            this.state1
                .Transition<Event1>(this.state2)
                .Entry<int>(i =>
                {
                    this.output.Enqueue($"- {i}");
                    Thread.Sleep(100);
                }),
            this.state2
                .Entry<int>(i =>
                {
                    this.output.Enqueue($"- {i}");
                    Thread.Sleep(100);
                })
                .Transition<Event1>(this.state2)
                .TransitionToFinal<Event2>()
        );

    private async Task<List<string>> RunFsm(Fsm fsm)
    {
        this.output.Clear();

        fsm.Start(42);

        var task1 = Task.Run(
            () =>
            {
                while (fsm.IsRunning)
                {
                    Thread.Sleep(100);
                }
            },
            this.TestContext.CancellationTokenSource.Token);

        var task2 = Task.Run(
            () =>
            {
                for (var i = 0; i <= 5; i++)
                {
                    this.output.Enqueue($"+ {i}");
                    fsm.Trigger(new DataEvent<Event1, int>(i));
                    Thread.Sleep(10);
                }

                fsm.Trigger(new Event2());
            },
            this.TestContext.CancellationTokenSource.Token);

        var task3 = Task.Run(
            () =>
            {
                for (var i = 10; i <= 15; i++)
                {
                    this.output.Enqueue($"+ {i}");
                    fsm.Trigger(new DataEvent<Event1, int>(i));
                    Thread.Sleep(1);
                }
            },
            this.TestContext.CancellationTokenSource.Token);

        await task1;
        await task2;
        await task3;

        this.output.ForEach(Console.WriteLine);

        return [.. this.output];
    }

    // ReSharper disable once ClassNeverInstantiated.Local
    private class Event1 : Event;

    private class Event2 : Event;
}