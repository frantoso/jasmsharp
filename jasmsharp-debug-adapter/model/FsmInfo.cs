// -----------------------------------------------------------------------
// <copyright file="FsmInfo.cs">
//     Created by Frank Listing at 2025/12/17.
// </copyright>
// -----------------------------------------------------------------------

// ReSharper disable UnusedMember.Global

namespace jasmsharp_debug_adapter.model;

/// <summary>
///     A class representing a finite state machine in the diagram generator.
/// </summary>
public class FsmInfo(string name, IList<StateInfo> states)
{
    /// <summary>
    ///     Gets the name of the state machine.
    /// </summary>
    public string Name { get; } = name;

    /// <summary>
    ///     Gets the states contained in this machine.
    /// </summary>
    public IList<StateInfo> States { get; } = states;

    /// <summary>
    ///     Returns a string that represents the current object.
    /// </summary>
    /// <returns>A string that represents the current object.</returns>
    public override string ToString() => $"{this.Name}";
}