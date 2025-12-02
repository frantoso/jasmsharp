// -----------------------------------------------------------------------
// <copyright file="FsmSync.cs">
//     Created by Frank Listing at 2025/11/06.
// </copyright>
// -----------------------------------------------------------------------

namespace jasmsharp;

/// <summary>
///     Class managing the states of a synchronous FSM (finite state machine).
/// </summary>
/// <param name="name">The name of the FSM.</param>
/// <param name="startState">The start state (first state) of the FSM.</param>
/// <param name="otherStates">The other states of the FSM.</param>
public class FsmSync(
    string name,
    EndStateContainer startState,
    List<EndStateContainer> otherStates
) : Fsm(name, startState, otherStates)
{
    /// <summary>
    ///     Creates a synchronous FSM from the provided data.
    /// </summary>
    /// <param name="name">The name of the FSM.</param>
    /// <param name="startState">The start state (first state) of the FSM.</param>
    /// <param name="otherStates">The other states of the FSM.</param>
    /// <returns>A new instance of FsmSync.</returns>
    public static FsmSync Of(
        string name,
        EndStateContainer startState,
        params EndStateContainer[] otherStates)
        => new(name, startState, [.. otherStates]);

    /// <summary>
    ///     Triggers a transition.
    /// </summary>
    /// <param name="event">The event occurred.</param>
    /// <returns>Returns true if the event was handled; false otherwise.</returns>
    public override bool Trigger(IEvent @event) => this.TriggerEvent(@event);

    /// <summary>
    ///     Triggers a transition.
    /// </summary>
    /// <typeparam name="TData">The type of the data parameter.</typeparam>
    /// <param name="event">The event occurred.</param>
    /// <param name="data">The data to send with the event.</param>
    /// <returns>Returns true if the event was handled; false otherwise.</returns>
    public bool Trigger<TData>(Event @event, TData data) => this.Trigger(@event.ToDataEvent(data));
}