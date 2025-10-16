// -----------------------------------------------------------------------
// <copyright file="NestedExample.cs">
//     Created by Frank Listing at 2025/10/05.
// </copyright>
// -----------------------------------------------------------------------

namespace jasmsharp.Tests.Examples;

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
[TestSubject(typeof(FsmSync))]
public class NestedExample
{
    private class Tick : Event;

    private class Parameters(bool isDayMode = true)
    {
        public bool IsDayMode { get; set; } = isDayMode;
    }

    private readonly Parameters parameters = new();

    private class FsmDayMode : CompositeState
    {
        private readonly State showingRed = new("ShowingRed");
        private readonly State showingRedYellow = new("ShowingRedYellow");
        private readonly State showingYellow = new("ShowingYellow");
        private readonly State showingGreen = new("ShowingGreen");

        public override IReadOnlyList<FsmSync> SubMachines { get; }

        public FsmDayMode() : base("ControllingDayMode")
        {
            this.SubMachines =
            [
                FsmSync.Of(
                    "traffic light day mode",
                    this.showingRed
                        .Entry<Parameters>(p => Console.WriteLine($"x--    {p}"))
                        .Transition<Tick>(this.showingRedYellow),
                    this.showingRedYellow
                        .Entry<Parameters>(p => Console.WriteLine($"xx-    {p}"))
                        .Transition<Tick>(this.showingGreen),
                    this.showingGreen
                        .Entry<Parameters>(p => Console.WriteLine($"--x    {p}"))
                        .Transition<Tick>(this.showingYellow),
                    this.showingYellow
                        .Entry<Parameters>(p => Console.WriteLine($"-x-    {p}"))
                        .Transition<Tick, Parameters>(this.showingRed, p => p?.IsDayMode ?? false)
                        .Transition<Tick, Parameters>(new FinalState(), p => !(p?.IsDayMode ?? true))
                )
            ];
        }
    }

    private class TrafficLight
    {
        public FsmSync FsmMain { get; }
        public FsmSync FsmNight { get; }
        public FsmDayMode ControllingDayMode { get; } = new();

        public TrafficLight()
        {
            this.FsmNight = TrafficLight.CreateFsmNightMode();

            var controllingNightMode = new State("ControllingNightMode");

            this.FsmMain =
                FsmSync.Of(
                    "traffic light controller",
                    this.ControllingDayMode
                        .Entry<Parameters>(p => Console.WriteLine($"starting day mode    {p}"))
                        .Transition<NoEvent>(controllingNightMode),
                    controllingNightMode
                        .Entry<Parameters>(p => Console.WriteLine($"starting night mode    {p}"))
                        .Transition<NoEvent>(this.ControllingDayMode)
                        .Child(this.FsmNight));
        }

        private static FsmSync CreateFsmNightMode()
        {
            var showingNothing = new State("ShowingNothing");
            var showingYellow = new State("ShowingYellow");

            return FsmSync.Of(
                "traffic light night mode",
                showingYellow
                    .Entry<Parameters>(p => Console.WriteLine($"x--    {p}"))
                    .Transition<Tick, Parameters>(showingNothing, p => !(p?.IsDayMode ?? true))
                    .Transition<Tick, Parameters>(new FinalState(), p => p?.IsDayMode ?? false),
                showingNothing
                    .Entry<Parameters>(p => Console.WriteLine($"xx-    {p}"))
                    .Transition<Tick>(showingYellow)
            );
        }
    }

    [TestMethod]
    public void StepsThroughTheStates()
    {
        var trafficLight = new TrafficLight();

        trafficLight.FsmMain.Start();
        Assert.AreEqual("ControllingDayMode", trafficLight.FsmMain.CurrentState.Name);
        Assert.AreEqual("ShowingRed", trafficLight.ControllingDayMode.SubMachines[0].CurrentState.Name);
        Assert.IsTrue(trafficLight.ControllingDayMode.SubMachines[0].IsRunning);
        Assert.IsFalse(trafficLight.FsmNight.IsRunning);

        trafficLight.FsmMain.Trigger(new Tick(), this.parameters);
        Assert.AreEqual("ShowingRedYellow", trafficLight.ControllingDayMode.SubMachines[0].CurrentState.Name);

        trafficLight.FsmMain.Trigger(new Tick(), this.parameters);
        Assert.AreEqual("ShowingGreen", trafficLight.ControllingDayMode.SubMachines[0].CurrentState.Name);

        trafficLight.FsmMain.Trigger(new Tick(), this.parameters);
        Assert.AreEqual("ShowingYellow", trafficLight.ControllingDayMode.SubMachines[0].CurrentState.Name);

        trafficLight.FsmMain.Trigger(new Tick(), this.parameters);
        Assert.AreEqual("ShowingRed", trafficLight.ControllingDayMode.SubMachines[0].CurrentState.Name);

        this.parameters.IsDayMode = false;
        trafficLight.FsmMain.Trigger(new Tick(), this.parameters);
        Assert.AreEqual("ShowingRedYellow", trafficLight.ControllingDayMode.SubMachines[0].CurrentState.Name);

        trafficLight.FsmMain.Trigger(new Tick(), this.parameters);
        Assert.AreEqual("ShowingGreen", trafficLight.ControllingDayMode.SubMachines[0].CurrentState.Name);
        Assert.IsTrue(trafficLight.ControllingDayMode.SubMachines[0].IsRunning);
        Assert.IsFalse(trafficLight.FsmNight.IsRunning);

        trafficLight.FsmMain.Trigger(new Tick(), this.parameters);
        Assert.AreEqual("ControllingDayMode", trafficLight.FsmMain.CurrentState.Name);
        Assert.AreEqual("ShowingYellow", trafficLight.ControllingDayMode.SubMachines[0].CurrentState.Name);

        trafficLight.FsmMain.Trigger(new Tick(), this.parameters);
        Assert.AreEqual("ControllingNightMode", trafficLight.FsmMain.CurrentState.Name);
        Assert.AreEqual("Final", trafficLight.ControllingDayMode.SubMachines[0].CurrentState.Name);
        Assert.AreEqual("ShowingYellow", trafficLight.FsmNight.CurrentState.Name);
        Assert.IsFalse(trafficLight.ControllingDayMode.SubMachines[0].IsRunning);
        Assert.IsTrue(trafficLight.FsmNight.IsRunning);

        trafficLight.FsmMain.Trigger(new Tick(), this.parameters);
        Assert.AreEqual("ShowingNothing", trafficLight.FsmNight.CurrentState.Name);
        Assert.IsFalse(trafficLight.ControllingDayMode.SubMachines[0].IsRunning);
        Assert.IsTrue(trafficLight.FsmNight.IsRunning);

        trafficLight.FsmMain.Trigger(new Tick(), this.parameters);
        Assert.AreEqual("ShowingYellow", trafficLight.FsmNight.CurrentState.Name);

        trafficLight.FsmMain.Trigger(new Tick(), this.parameters);
        Assert.AreEqual("ShowingNothing", trafficLight.FsmNight.CurrentState.Name);

        this.parameters.IsDayMode = true;
        trafficLight.FsmMain.Trigger(new Tick(), this.parameters);
        Assert.AreEqual("ControllingNightMode", trafficLight.FsmMain.CurrentState.Name);
        Assert.AreEqual("ShowingYellow", trafficLight.FsmNight.CurrentState.Name);
        Assert.IsFalse(trafficLight.ControllingDayMode.SubMachines[0].IsRunning);
        Assert.IsTrue(trafficLight.FsmNight.IsRunning);

        trafficLight.FsmMain.Trigger(new Tick(), this.parameters);
        Assert.AreEqual("ControllingDayMode", trafficLight.FsmMain.CurrentState.Name);
        Assert.AreEqual("ShowingRed", trafficLight.ControllingDayMode.SubMachines[0].CurrentState.Name);
        Assert.AreEqual("Final", trafficLight.FsmNight.CurrentState.Name);
        Assert.IsTrue(trafficLight.ControllingDayMode.SubMachines[0].IsRunning);
        Assert.IsFalse(trafficLight.FsmNight.IsRunning);
    }
}