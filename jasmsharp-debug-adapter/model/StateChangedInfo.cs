// -----------------------------------------------------------------------
// <copyright file="StateChangedInfo.cs">
//     Created by Frank Listing at 2025/12/16.
// </copyright>
// -----------------------------------------------------------------------

namespace jasmsharp_debug_adapter.model;

using jasmsharp;

// ReSharper disable UnusedMember.Global

public class StateChangedInfo(Fsm sender, StateChangedEventArgs e)
{
    public string Fsm { get; } = sender.Name;

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