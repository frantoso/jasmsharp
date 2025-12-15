// -----------------------------------------------------------------------
// <copyright file="Events.cs">
//     Created by Frank Listing at 2025/12/15.
// </copyright>
// -----------------------------------------------------------------------

using jasmsharp;

namespace HistoryExample;

internal class BreakEvent : Event;

internal class ContinueEvent : Event;

internal class ContinueDeepEvent : Event;

internal class NextEvent : Event;

internal class RestartEvent : Event;