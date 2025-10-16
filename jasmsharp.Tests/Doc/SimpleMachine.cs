// -----------------------------------------------------------------------
// <copyright file="SimpleMachine.cs">
//     Created by Frank Listing at 2025/10/16.
// </copyright>
// -----------------------------------------------------------------------

namespace jasmsharp.Tests.Doc;

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class SimpleMachine
{
    [TestMethod]
    public void StepsThroughTheStates()
    {
        // create the states...
        var showingRed = new State("ShowingRed");
        var showingRedYellow = new State("ShowingRedYellow");
        var showingYellow = new State("ShowingYellow");
        var showingGreen = new State("ShowingGreen");

        // create the state machine...
        var fsm =
            FsmSync.Of(
                "simple traffic light",
                // define initial state with transitions and other parameters...
                showingRed
                    .Entry(() => Console.WriteLine("x--")) // add an entry function
                    .Transition<Tick>(showingRedYellow), // add one or more transitions
                // define other states with transitions and other parameters...
                showingRedYellow
                    .Entry(() => Console.WriteLine("xx-"))
                    .Transition<Tick>(showingGreen),
                showingGreen
                    .Entry(() => Console.WriteLine("--x"))
                    .Transition<Tick>(showingYellow),
                showingYellow
                    .Entry(() => Console.WriteLine("-x-"))
                    .Transition<Tick>(showingRed)
            );

        // start the state machine
        fsm.Start();

        Assert.IsTrue(fsm.IsRunning);

        // trigger an event
        fsm.Trigger(new Tick());

        Assert.AreEqual(showingRedYellow, fsm.CurrentState);
    }

    [TestMethod]
    public void SynchronousFsm()
    {
        var state = new State("MyState");
        var fsm =
            FsmSync.Of(
                "MyFsm",
                // add at minimum one state
                state
                    .TransitionToFinal<Tick>()
            );

        fsm.Start();
    }


    [TestMethod]
    public void AsynchronousFsm()
    {
        var state = new State("MyState");
        var fsm =
            FsmAsync.Of(
                "MyFsm",
                // add at minimum one state
                state
                    .TransitionToFinal<Tick>()
            );

        fsm.Start();
    }

    public class Tick : Event;
}