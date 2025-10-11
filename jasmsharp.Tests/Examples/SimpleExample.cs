// -----------------------------------------------------------------------
// <copyright file="SimpleExample.cs">
//     Created by Frank Listing at 2025/10/04.
// </copyright>
// -----------------------------------------------------------------------

namespace jasmsharp.Tests.Examples;

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestUtils;

[TestClass]
[TestSubject(typeof(FsmSync))]
public class SimpleExample
{
    private class Tick : Event;

    private class OtherEvent : Event;

    private static readonly TrafficLight TrafficLightInstance = new();

    private class TrafficLight
    {
        public FsmSync Fsm { get; }

        internal State ShowingRed { get; } = new("ShowingRed");
        internal State ShowingRedYellow { get; } = new("ShowingRedYellow");
        internal State ShowingYellow { get; } = new("ShowingYellow");
        internal State ShowingGreen { get; } = new("ShowingGreen");

        public TrafficLight()
        {
            this.Fsm =
                FsmSync.Of(
                    "simple traffic light",
                    this.ShowingRed
                        .Entry<int>(i => Console.WriteLine($"x--    {i}"))
                        .Transition<Tick>(this.ShowingRedYellow),
                    this.ShowingRedYellow
                        .Entry<int>(i => Console.WriteLine($"xx-    {i}"))
                        .Transition<Tick>(this.ShowingGreen),
                    this.ShowingGreen
                        .Entry<int>(i => Console.WriteLine($"--x    {i}"))
                        .Transition<Tick>(this.ShowingYellow),
                    this.ShowingYellow
                        .Entry(() => Console.WriteLine("-x-    ry"))
                        .Transition<Tick>(this.ShowingRed)
                );
        }
    }

    [TestMethod]
    public void StepsThroughTheStates()
    {
        var fsm = SimpleExample.TrafficLightInstance.Fsm;
        fsm.Start(42);

        Assert.AreEqual("ShowingRed", fsm.CurrentState.Name);

        fsm.Trigger(new DataEvent<Tick, int>(1));
        Assert.AreEqual("ShowingRedYellow", fsm.CurrentState.Name);

        fsm.Trigger(new Tick());
        Assert.AreEqual("ShowingGreen", fsm.CurrentState.Name);

        fsm.Trigger(new DataEvent<Tick, int>(3));
        Assert.AreEqual("ShowingYellow", fsm.CurrentState.Name);

        fsm.Trigger(new Tick(), 2);
        Assert.AreEqual("ShowingRed", fsm.CurrentState.Name);

        fsm.Trigger(new Tick());
        Assert.AreEqual("ShowingRedYellow", fsm.CurrentState.Name);

        fsm.Trigger(new Tick());
        Assert.AreEqual("ShowingGreen", fsm.CurrentState.Name);

        fsm.Trigger(new Tick());
        Assert.AreEqual("ShowingYellow", fsm.CurrentState.Name);
    }

    // @formatter:off
    private static List<List<TestData>> TestData0Switch =>
    [
        [new TestData(startState: SimpleExample.TrafficLightInstance.ShowingRed, @event: new Tick(), endState: SimpleExample.TrafficLightInstance.ShowingRedYellow, wasHandled: true)],
        [new TestData(startState: SimpleExample.TrafficLightInstance.ShowingRedYellow, @event: new Tick(), endState: SimpleExample.TrafficLightInstance.ShowingGreen, wasHandled: true)],
        [new TestData(startState: SimpleExample.TrafficLightInstance.ShowingGreen, @event: new Tick(), endState: SimpleExample.TrafficLightInstance.ShowingYellow, wasHandled: true)],
        [new TestData(startState: SimpleExample.TrafficLightInstance.ShowingYellow, @event: new Tick(), endState: SimpleExample.TrafficLightInstance.ShowingRed, wasHandled: true)],
        [new TestData(startState: SimpleExample.TrafficLightInstance.ShowingRed, @event: new OtherEvent(), endState: SimpleExample.TrafficLightInstance.ShowingRed,wasHandled: false)]
    ];
    // @formatter:on

    [TestMethod]
    [DynamicData(nameof(TestData0Switch))]
    public void ChangesToTheRightStateWith0SwitchCoverage(List<TestData> data)
    {
        SimpleExample.TrafficLightInstance.Fsm.TestStateChange(data);
    }

    // @formatter:off
    private static List<List<TestData>> TestData1Switch =>
    [
        [new TestData(startState: SimpleExample.TrafficLightInstance.ShowingRed, @event: new Tick(), endState: SimpleExample.TrafficLightInstance.ShowingRedYellow, wasHandled: true),
         new TestData(startState: SimpleExample.TrafficLightInstance.ShowingRedYellow, @event: new Tick(), endState: SimpleExample.TrafficLightInstance.ShowingGreen, wasHandled: true)],
        [new TestData(startState: SimpleExample.TrafficLightInstance.ShowingRedYellow, @event: new Tick(), endState: SimpleExample.TrafficLightInstance.ShowingGreen, wasHandled: true),
         new TestData(startState: SimpleExample.TrafficLightInstance.ShowingGreen, @event: new Tick(), endState: SimpleExample.TrafficLightInstance.ShowingYellow, wasHandled: true)],
        [new TestData(startState: SimpleExample.TrafficLightInstance.ShowingGreen, @event: new Tick(), endState: SimpleExample.TrafficLightInstance.ShowingYellow, wasHandled: true),
         new TestData(startState: SimpleExample.TrafficLightInstance.ShowingYellow, @event: new Tick(), endState: SimpleExample.TrafficLightInstance.ShowingRed, wasHandled: true)],
        [new TestData(startState: SimpleExample.TrafficLightInstance.ShowingYellow, @event: new Tick(), endState: SimpleExample.TrafficLightInstance.ShowingRed, wasHandled: true),
         new TestData(startState: SimpleExample.TrafficLightInstance.ShowingRed, @event: new Tick(), endState: SimpleExample.TrafficLightInstance.ShowingRedYellow, wasHandled: true)]
    ];
    // @formatter:on

    [TestMethod]
    [DynamicData(nameof(TestData1Switch))]
    public void ChangesToTheRightStateWith1SwitchCoverage(List<TestData> data)
    {
        SimpleExample.TrafficLightInstance.Fsm.TestStateChange(data);
    }
}