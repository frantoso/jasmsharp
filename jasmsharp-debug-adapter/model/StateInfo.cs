// -----------------------------------------------------------------------
// <copyright file="StateInfo.cs">
//     Created by Frank Listing at 2025/12/17.
// </copyright>
// -----------------------------------------------------------------------

// ReSharper disable UnusedMember.Global

namespace jasmsharp_debug_adapter.model;

/// <summary>
///     A class representing a state in the diagram generator.
/// </summary>
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
    ///     Initializes a new instance of the <see cref="StateInfo" /> class.
    /// </summary>
    /// <param name="toCopy">The state to copy all information except the history stuff from.</param>
    /// <param name="hasHistory">A value indicating whether this state contains a history end point.</param>
    /// <param name="hasDeepHistory">A value indicating whether this state contains a deep history end point.</param>
    public StateInfo(StateInfo toCopy, bool hasHistory, bool hasDeepHistory) :
        this(
            toCopy.Id,
            toCopy.Name,
            toCopy.IsInitial,
            toCopy.IsFinal,
            toCopy.Transitions,
            toCopy.Children,
            hasHistory,
            hasDeepHistory)
    {
    }

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
    ///     Returns a string that represents the current object.
    /// </summary>
    /// <returns>A string that represents the current object.</returns>
    public override string ToString() => $"{this.Name}";
}