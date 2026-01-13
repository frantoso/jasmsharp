// -----------------------------------------------------------------------
// <copyright file="ExtractorTests.cs">
//     Created by Frank Listing at 2025/12/21.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using jasmsharp_debug_adapter.model;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace jasmsharp_debug_adapter.Tests;

using System;
using System.Linq;
using jasmsharp;
using JetBrains.Annotations;

[TestClass]
public class ExtractorTests
{
    private const string Owner = "the-owner";

    public static IEnumerable<object[]> TransitionConvertTestData =>
    [
        [new TransitionEndPoint(new State("State")), false, false, false],
        [new State("State").History, true, false, false],
        [new State("State").DeepHistory, false, true, false],
        [new TransitionEndPoint(new FinalState()), false, false, true],
    ];

    public static IEnumerable<object[]> UpdateStateInfoData =>
    [
        [new List<Tuple<bool, bool>>([Tuple.Create(false, false)]), false, false],
        [new List<Tuple<bool, bool>>([Tuple.Create(true, false)]), true, false],
        [new List<Tuple<bool, bool>>([Tuple.Create(false, true)]), false, true],

        [new List<Tuple<bool, bool>>([Tuple.Create(false, false), Tuple.Create(false, false)]), false, false],
        [new List<Tuple<bool, bool>>([Tuple.Create(true, false), Tuple.Create(false, false)]), true, false],
        [new List<Tuple<bool, bool>>([Tuple.Create(false, false), Tuple.Create(true, false)]), true, false],
        [new List<Tuple<bool, bool>>([Tuple.Create(false, true), Tuple.Create(false, false)]), false, true],
        [new List<Tuple<bool, bool>>([Tuple.Create(false, false), Tuple.Create(false, true)]), false, true],
        [new List<Tuple<bool, bool>>([Tuple.Create(true, false), Tuple.Create(true, false)]), true, false],
        [new List<Tuple<bool, bool>>([Tuple.Create(true, false), Tuple.Create(true, false)]), true, false],
        [new List<Tuple<bool, bool>>([Tuple.Create(true, false), Tuple.Create(false, true)]), true, true],
        [new List<Tuple<bool, bool>>([Tuple.Create(false, true), Tuple.Create(true, false)]), true, true],
        [new List<Tuple<bool, bool>>([Tuple.Create(true, true), Tuple.Create(true, true)]), true, true],
    ];

    [TestMethod]
    public void NormalizedIdTestForNormalState()
    {
        var state = new State("state");

        var normalizedId = state.NormalizedId("owner");

        Assert.AreEqual(state.Id, normalizedId);
    }

    [TestMethod]
    public void NormalizedIdForFinalState()
    {
        var final = new FinalState();

        var normalizedId = final.NormalizedId(Owner);

        Assert.AreEqual($"{Owner}-{Extractor.FinalStateId}", normalizedId);
    }

    [TestMethod]
    [DynamicData(nameof(TransitionConvertTestData))]
    public void ConvertTransitionTest(TransitionEndPoint state, bool isToHistory, bool isToDeepHistory, bool isToFinal)
    {
        var transition = new Transition<TestEvent>(state, () => true);

        var transitionInfo = transition.Convert(Owner);

        Assert.AreEqual(state.State.NormalizedId(Owner), transitionInfo.EndPointId);
        Assert.AreEqual(isToHistory, transitionInfo.IsHistory);
        Assert.AreEqual(isToDeepHistory, transitionInfo.IsDeepHistory);
        Assert.AreEqual(isToFinal, transitionInfo.IsToFinal);
    }

    [TestMethod]
    public void ConvertStateForNormalState()
    {
        var container = new State("S1")
            .Transition<TestEvent>(new FinalState())
            .Transition<TestEvent>(new State("S2"));

        var info = container.Convert(Owner);

        Assert.AreEqual(container.State.NormalizedId(Owner), info.Id);
        Assert.AreEqual(container.State.Name, info.Name);
        Assert.IsFalse(info.IsInitial);
        Assert.IsFalse(info.IsFinal);
        Assert.HasCount(2, info.Transitions);
        Assert.IsTrue(info.Transitions[0].IsToFinal);
        Assert.IsFalse(info.Transitions[1].IsToFinal);
    }

    [TestMethod]
    public void ConvertStateForInitialState()
    {
        var container = new InitialStateContainer(new List<ITransition>());

        var info = container.Convert(Owner);

        Assert.IsTrue(info.IsInitial);
        Assert.IsFalse(info.IsFinal);
    }

    [TestMethod]
    public void ConvertStateForFinalState()
    {
        var container = new FinalStateContainer();

        var info = container.Convert(Owner);

        Assert.IsFalse(info.IsInitial);
        Assert.IsTrue(info.IsFinal);
    }

    [TestMethod]
    [DynamicData(nameof(UpdateStateInfoData))]
    public void UpdateStateInfoTest(
        IEnumerable<Tuple<bool, bool>> initData,
        bool expectedHistory,
        bool expectedDeepHistory)
    {
        const string stateId = "State_01";
        var state = new StateInfo(
            stateId,
            "State",
            isInitial: false,
            isFinal: false,
            transitions: new List<TransitionInfo>(),
            children: new List<FsmInfo>(),
            hasHistory: false,
            hasDeepHistory: false);
        var transitions = new List<TransitionInfo>
        {
            new("other", isHistory: false, isDeepHistory: true, isToFinal: false) // different target
        };
        initData.ForEach(data =>
            transitions.Add(new TransitionInfo(stateId, data.Item1, data.Item2, false)));

        var updated = state.Update(transitions);

        Assert.AreEqual(expectedHistory, updated.HasHistory);
        Assert.AreEqual(expectedDeepHistory, updated.HasDeepHistory);
    }

    [TestMethod]
    public void ConvertFsmTest()
    {
        var startContainer = new State("Start")
            .TransitionToFinal<TestEvent>();
        var fsm = FsmSync.Of("MyFsm", startContainer);

        var info = fsm.Convert();

        Assert.AreEqual("MyFsm", info.Name);
        Assert.IsTrue(info.States.Any(s => s.IsInitial));
        Assert.IsTrue(info.States.SelectMany(s => s.Transitions).Any(t => t.IsToFinal));
    }

    [TestMethod]
    public void AllMachinesTest()
    {
        var child = FsmSync.Of("ChildFsm", new State("CStart").ToContainer());
        var parent = FsmSync.Of(
            "ParentFsm",
            new State("PStart")
                .Child(child));

        var all = parent.AllMachines();

        Assert.IsTrue(all.Any(f => f.Name == "ParentFsm"));
        Assert.IsTrue(all.Any(f => f.Name == "ChildFsm"));
        Assert.IsGreaterThanOrEqualTo(2, all.Count);
    }

    [UsedImplicitly]
    private sealed class TestEvent : Event;
}