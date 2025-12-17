// -----------------------------------------------------------------------
// <copyright file="JasmCommand.cs">
//     Created by Frank Listing at 2025/12/17.
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
    public string Fsm { get; set; } = fsm;
    public string Command { get; set; } = command;
    public string Payload { get; set; } = payload;
}