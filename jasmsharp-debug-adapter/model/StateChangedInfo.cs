// -----------------------------------------------------------------------
// <copyright file="StateChangedInfo.cs">
//     Created by Frank Listing at 2025/12/21.
// </copyright>
// -----------------------------------------------------------------------

// ReSharper disable UnusedMember.Global

namespace jasmsharp_debug_adapter.model;

using jasmsharp;

/// <summary>
///     A class representing information about a state change event in a finite state machine (FSM).
/// </summary>
public class StateChangedInfo(string sender, StateChangedEventArgs e)
{
    /// <summary>
    ///     Gets the name of the sending state machine.
    /// </summary>
    public string Fsm { get; } = sender;

    /// <summary>
    ///     Gets the name of the state before the state change.
    /// </summary>
    public string OldStateName { get; } = e.OldState.Name;

    /// <summary>
    ///     Gets the id of the state before the state change.
    /// </summary>
    public string OldStateId { get; } = e.OldState.NormalizedId(sender);

    /// <summary>
    ///     Gets the name of the state after the state change (the current state).
    /// </summary>
    public string NewStateName { get; } = e.NewState.Name;

    /// <summary>
    ///     Gets the id of the state after the state change (the current state).
    /// </summary>
    public string NewStateId { get; } = e.NewState.NormalizedId(sender);
}