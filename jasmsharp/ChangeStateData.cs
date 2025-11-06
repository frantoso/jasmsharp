// -----------------------------------------------------------------------
// <copyright file="ChangeStateData.cs">
//     Created by Frank Listing at 2025/11/06.
// </copyright>
// -----------------------------------------------------------------------

namespace jasmsharp;

/// <summary>
///     Stores data about a state change.
/// </summary>
/// <param name="Handled">A value indicating whether an event was handled.</param>
/// <param name="EndPoint">The new end point to change to.</param>
public sealed record ChangeStateData(bool Handled, TransitionEndPoint? EndPoint = null);