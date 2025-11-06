// -----------------------------------------------------------------------
// <copyright file="StateChangedEventArgs.cs">
//     Created by Frank Listing at 2025/10/07.
// </copyright>
// -----------------------------------------------------------------------

namespace jasmsharp;

/// <summary>
///     Argument data used to inform about a state change.
/// </summary>
/// <param name="oldState">The state before the state change.</param>
/// <param name="newState">The state after the state change (the current state).</param>
public class StateChangedEventArgs(IState oldState, IState newState) : EventArgs
{
    /// <summary>Gets the state before the state change.</summary>
    public IState OldState { get; } = oldState;

    /// <summary>Gets the state after the state change (the current state).</summary>
    public IState NewState { get; } = newState;
}