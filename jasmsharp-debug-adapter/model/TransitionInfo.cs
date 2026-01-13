// -----------------------------------------------------------------------
// <copyright file="TransitionInfo.cs">
//     Created by Frank Listing at 2025/12/17.
// </copyright>
// -----------------------------------------------------------------------

// ReSharper disable UnusedMember.Global

namespace jasmsharp_debug_adapter.model;

/// <summary>
///     A class representing a transition in the diagram generator.
/// </summary>
public class TransitionInfo(string id, bool isHistory, bool isDeepHistory, bool isToFinal)
{
    /// <summary>
    ///     Gets the end point identifier.
    /// </summary>
    public string EndPointId { get; } = id;

    /// <summary>
    ///     Gets a value indicating whether this transition ends in a history state.
    /// </summary>
    public bool IsHistory { get; } = isHistory;

    /// <summary>
    ///     Gets a value indicating whether this transition ends in a deep history state.
    /// </summary>
    public bool IsDeepHistory { get; } = isDeepHistory;

    /// <summary>
    ///     Gets a value indicating whether this transition ends in the final state.
    /// </summary>
    public bool IsToFinal { get; } = isToFinal;
}