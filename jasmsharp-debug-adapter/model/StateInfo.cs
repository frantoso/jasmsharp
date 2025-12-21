// -----------------------------------------------------------------------
// <copyright file="StateInfo.cs">
//     Created by Frank Listing at 2025/12/21.
// </copyright>
// -----------------------------------------------------------------------

// ReSharper disable UnusedMember.Global

namespace jasmsharp_debug_adapter.model;

/// <summary>
///     A class representing a state in the diagram generator.
/// </summary>
/// <param name="id">The ID of the state.</param>
/// <param name="name">The name of the state.</param>
/// <param name="isInitial">A value indicating whether this is an initial state.</param>
/// <param name="isFinal">A value indicating whether this is a final state.</param>
/// <param name="transitions">A list of outgoing transitions.</param>
/// <param name="children">A list of sub-state-machines.</param>
/// <param name="hasHistory">A value indicating whether this state contains a history end point.</param>
/// <param name="hasDeepHistory">A value indicating whether this state contains a deep history end point.</param>
public class StateInfo(
    string id,
    string name,
    bool isInitial,
    bool isFinal,
    IList<TransitionInfo> transitions,
    IList<FsmInfo> children,
    bool hasHistory,
    bool hasDeepHistory)
{
    /// <summary>
    ///     Gets the name of the state.
    /// </summary>
    public string Name { get; } = name;

    /// <summary>
    ///     Gets the identifier of the state.
    /// </summary>
    public string Id { get; } = id;

    /// <summary>
    ///     Gets the transitions.
    /// </summary>
    public IList<TransitionInfo> Transitions { get; } = transitions;

    /// <summary>
    ///     Gets the children.
    /// </summary>
    public IList<FsmInfo> Children { get; } = children;

    /// <summary>
    ///     Gets a value indicating whether this instance is the initial state.
    /// </summary>
    public bool IsInitial { get; } = isInitial;

    /// <summary>
    ///     Gets a value indicating whether this instance is the final state.
    /// </summary>
    public bool IsFinal { get; } = isFinal;

    /// <summary>
    ///     Gets a value indicating whether this instance has a history end point.
    /// </summary>
    public bool HasHistory { get; } = hasHistory;

    /// <summary>
    ///     Gets a value indicating whether this instance has a deep history end point.
    /// </summary>
    public bool HasDeepHistory { get; } = hasDeepHistory;

    /// <summary>
    ///     Updates this <see cref="StateInfo" /> with history information.
    /// </summary>
    /// <param name="hasHistory">A value indicating whether this state contains a history end point.</param>
    /// <param name="hasDeepHistory">A value indicating whether this state contains a deep history end point.</param>
    /// <returns>Returns a new <see cref="StateInfo" /> instance with the updated information.</returns>
    public StateInfo Update(bool hasHistory, bool hasDeepHistory)
    {
        return new StateInfo(
            this.Id,
            this.Name,
            this.IsInitial,
            this.IsFinal,
            this.Transitions,
            this.Children,
            this.HasHistory || hasHistory,
            this.HasDeepHistory || hasDeepHistory);
    }

    /// <summary>
    ///     Returns a string that represents the current object.
    /// </summary>
    /// <returns>A string that represents the current object.</returns>
    public override string ToString() => $"{this.Name}";
}