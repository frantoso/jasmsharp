// -----------------------------------------------------------------------
// <copyright file="Events.cs">
//     Created by Frank Listing at 2025/10/07.
// </copyright>
// -----------------------------------------------------------------------

namespace HistoryExampleWpf.Model;

using jasmsharp;

public class BreakEvent : Event;

public class ContinueEvent : Event;

public class ContinueDeepEvent : Event;

public class NextEvent : Event;

public class RestartEvent : Event;

/// <summary>
/// A class containing all events used by the state machines of this application.
/// </summary>
public static class Events
{
    /// <summary> The break event. </summary>
    public static readonly BreakEvent Break = new();

    /// <summary> The continue-event for flat history. </summary>
    public static readonly ContinueEvent Continue = new();

    /// <summary> The continue-event for deep history. </summary>
    public static readonly ContinueDeepEvent ContinueDeep = new();

    /// <summary> The next event. </summary>
    public static readonly NextEvent Next = new();

    /// <summary> The restart event. </summary>
    public static readonly RestartEvent Restart = new();
}