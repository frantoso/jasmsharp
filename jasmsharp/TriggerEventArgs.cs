// -----------------------------------------------------------------------
// <copyright file="TriggerEventArgs.cs">
//     Created by Frank Listing at 2025/10/07.
// </copyright>
// -----------------------------------------------------------------------

namespace jasmsharp;

/// <summary>
///     Argument data used to inform about a state change.
/// </summary>
/// <param name="currentState">The current state of the state machine.</param>
/// <param name="event">The event which was processed.</param>
/// <param name="handled">A value indicating whether the event was handled (true) or not (false).</param>
public class TriggerEventArgs(IState currentState, IEvent @event, bool handled) : EventArgs
{
    /// <summary> Gets the current state of the state machine. </summary>
    public IState CurrentState { get; } = currentState;

    /// <summary> Gets the event which was processed. </summary>
    public IEvent Event { get; } = @event;

    /// <summary> Gets a value indicating whether the event was handled (true) or not (false). </summary>
    public bool Handled { get; } = handled;
}