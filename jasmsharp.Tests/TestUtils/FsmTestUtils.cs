// -----------------------------------------------------------------------
// <copyright file="FsmTestUtils.cs">
//     Created by Frank Listing at 2025/10/04.
// </copyright>
// -----------------------------------------------------------------------

namespace jasmsharp.Tests.TestUtils;

using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

internal class TestEvent : Event;

internal class OtherTestEvent : Event;

internal class DerivedTestEvent1 : TestEvent;

internal class DerivedTestEvent2 : TestEvent;

public class TestData(
    State startState,
    IEvent @event,
    IEndState endState,
    bool wasHandled
)
{
    public State StartState { get; } = startState;
    public IEvent Event { get; } = @event;
    public IEndState EndState { get; } = endState;
    public bool WasHandled { get; } = wasHandled;
}

internal static class FsmTestUtils
{
    public static void TestStateChange(this Fsm fsm, IEnumerable<TestData> testData)
    {
        var debugInterface = fsm.DebugInterface;

        foreach (var it in testData.ToList())
        {
            debugInterface.SetState(it.StartState);

            var handled = fsm.Trigger(it.Event);

            Assert.AreEqual(it.EndState, fsm.CurrentState);
            Assert.AreEqual(it.WasHandled, handled);

            if (it.EndState is FinalState)
                break;
        }
    }
}