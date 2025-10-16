// -----------------------------------------------------------------------
// <copyright file="TransitionEndPoint.cs">
//     Created by Frank Listing at 2025/09/30.
// </copyright>
// -----------------------------------------------------------------------

namespace jasmsharp;

/// <summary>
/// A class to store the end point of a transition (state and history).
/// </summary>
/// <param name="State">The destination state of the transition.</param>
/// <param name="History">The type of history to use.</param>
public sealed record TransitionEndPoint(IEndState State, History History)
{
    /// <summary>
    /// A class to store the end point of a transition (state and history).
    /// </summary>
    /// <param name="state">The destination state of the transition.</param>
    public TransitionEndPoint(IEndState state) : this(state, History.None)
    {
    }
}