// -----------------------------------------------------------------------
// <copyright file="JasmCommand.cs">
//     Created by Frank Listing at 2025/12/21.
// </copyright>
// -----------------------------------------------------------------------

namespace jasmsharp_debug_adapter.model;

using System.Diagnostics.CodeAnalysis;

/// <summary>
///     Helper class representing a JASM command. Used for commands exchanged with the debug adapter.
/// </summary>
[SuppressMessage("ReSharper", "UnusedMember.Global")]
public class JasmCommand(string fsm, string command, string payload)
{
    /// <summary>
    ///     Gets the name of the state machine related to this command.
    /// </summary>
    public string Fsm { get; set; } = fsm;

    /// <summary>
    ///     Gets or sets the command.
    /// </summary>
    public string Command { get; set; } = command;

    /// <summary>
    ///     Gets or sets the payload.
    /// </summary>
    public string Payload { get; set; } = payload;
}