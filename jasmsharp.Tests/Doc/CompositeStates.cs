// -----------------------------------------------------------------------
// <copyright file="CompositeStates.cs">
//     Created by Frank Listing at 2025/10/16.
// </copyright>
// -----------------------------------------------------------------------

namespace jasmsharp.Tests.Doc;

using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class CompositeStates
{
    [TestMethod]
    public void UseCompositeStates()
    {
        // BEGIN_COMPOSITE_STATE_MANUALLY_CODE_SNIPPET
        var showingNothing = new State("ShowingNothing");
        var showingYellow = new State("ShowingYellow");

        var fsmNight =
            FsmSync.Of(
                "ControllingNightMode",
                showingYellow
                    .Transition<Tick, bool>(showingNothing, b => !b)
                    .Transition<Tick, bool>(new FinalState(), b => b),
                showingNothing
                    .Transition<Tick>(showingYellow)
            );
        // END_COMPOSITE_STATE_MANUALLY_CODE_SNIPPET

        // BEGIN_USE_COMPOSITE_STATE_CODE_SNIPPET
        var controllingDayMode = new ControllingDayMode();
        var controllingNightMode = new State("ControllingNightMode");

        var trafficLight =
            FsmSync.Of(
                "TrafficLight",
                controllingDayMode // is a composite state - child is added automatically
                    .Transition(controllingNightMode),
                controllingNightMode // normal state to use as a composite state
                    .Child(fsmNight) // child must be added manually
                    .Transition(controllingDayMode)
            );
        // END_USE_COMPOSITE_STATE_CODE_SNIPPET

        var isDayMode = true;

        trafficLight.Start();
        Assert.AreEqual("ControllingDayMode", trafficLight.CurrentState.Name);
        Assert.AreEqual("ShowingRed", controllingDayMode.SubMachines[0].CurrentState.Name);

        trafficLight.Trigger(new Tick(), isDayMode);
        Assert.AreEqual("ShowingRedYellow", controllingDayMode.SubMachines[0].CurrentState.Name);

        trafficLight.Trigger(new Tick(), isDayMode);
        Assert.AreEqual("ShowingGreen", controllingDayMode.SubMachines[0].CurrentState.Name);

        trafficLight.Trigger(new Tick(), isDayMode);
        Assert.AreEqual("ShowingYellow", controllingDayMode.SubMachines[0].CurrentState.Name);

        trafficLight.Trigger(new Tick(), isDayMode);
        Assert.AreEqual("ShowingRed", controllingDayMode.SubMachines[0].CurrentState.Name);

        isDayMode = false;
        trafficLight.Trigger(new Tick(), isDayMode);
        Assert.AreEqual("ShowingRedYellow", controllingDayMode.SubMachines[0].CurrentState.Name);

        trafficLight.Trigger(new Tick(), isDayMode);
        Assert.AreEqual("ShowingGreen", controllingDayMode.SubMachines[0].CurrentState.Name);
        Assert.IsTrue(controllingDayMode.SubMachines[0].IsRunning);
        Assert.IsFalse(fsmNight.IsRunning);

        trafficLight.Trigger(new Tick(), isDayMode);
        Assert.AreEqual("ControllingDayMode", trafficLight.CurrentState.Name);
        Assert.AreEqual("ShowingYellow", controllingDayMode.SubMachines[0].CurrentState.Name);

        trafficLight.Trigger(new Tick(), isDayMode);
        Assert.AreEqual("ControllingNightMode", trafficLight.CurrentState.Name);
        Assert.AreEqual("Final", controllingDayMode.SubMachines[0].CurrentState.Name);
        Assert.AreEqual("ShowingYellow", fsmNight.CurrentState.Name);

        trafficLight.Trigger(new Tick(), isDayMode);
        Assert.AreEqual("ShowingNothing", fsmNight.CurrentState.Name);

        trafficLight.Trigger(new Tick(), isDayMode);
        Assert.AreEqual("ShowingYellow", fsmNight.CurrentState.Name);

        trafficLight.Trigger(new Tick(), isDayMode);
        Assert.AreEqual("ShowingNothing", fsmNight.CurrentState.Name);

        isDayMode = true;
        trafficLight.Trigger(new Tick(), isDayMode);
        Assert.AreEqual("ControllingNightMode", trafficLight.CurrentState.Name);
        Assert.AreEqual("ShowingYellow", fsmNight.CurrentState.Name);

        trafficLight.Trigger(new Tick(), isDayMode);
        Assert.AreEqual("ControllingDayMode", trafficLight.CurrentState.Name);
        Assert.AreEqual("ShowingRed", controllingDayMode.SubMachines[0].CurrentState.Name);
        Assert.AreEqual("Final", fsmNight.CurrentState.Name);
    }

    private class Tick : Event;

    public class ControllingDayMode : CompositeState
    {
        private readonly State showingGreen = new("ShowingGreen");
        private readonly State showingRed = new("ShowingRed");
        private readonly State showingRedYellow = new("ShowingRedYellow");
        private readonly State showingYellow = new("ShowingYellow");

        public ControllingDayMode()
        {
            this.SubMachines =
            [
                FsmSync.Of(
                    this.Name,
                    this.showingRed
                        .Transition<Tick>(this.showingRedYellow),
                    this.showingRedYellow
                        .Transition<Tick>(this.showingGreen),
                    this.showingGreen
                        .Transition<Tick>(this.showingYellow),
                    this.showingYellow
                        .Transition<Tick, bool>(this.showingRed, b => b)
                        .Transition<Tick, bool>(new FinalState(), b => !b)
                )
            ];
        }

        public override IReadOnlyList<FsmSync> SubMachines { get; }
    }
}