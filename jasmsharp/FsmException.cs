// -----------------------------------------------------------------------
// <copyright file="FsmException.cs">
//     Created by Frank Listing at 2025/11/06.
// </copyright>
// -----------------------------------------------------------------------

namespace jasmsharp;

/// <summary>
///     Represents an exception that occurs in the Finite State Machine (FSM) implementation.
/// </summary>
/// <param name="message">The error message that explains the reason for the exception.</param>
/// <param name="stateName">The name of the state where the exception occurred.</param>
/// <param name="innerException">The exception that is the cause of the current exception.</param>
public class FsmException(
    string? message = null,
    string stateName = "",
    Exception? innerException = null
) : Exception(message, innerException)
{
    /// <summary>
    ///     Gets the name of the state where the exception occurred.
    /// </summary>
    public string StateName { get; } = stateName;
}