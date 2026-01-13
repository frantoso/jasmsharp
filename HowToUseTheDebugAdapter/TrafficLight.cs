// -----------------------------------------------------------------------
// <copyright file="TrafficLight.cs">
//     Created by Frank Listing at 2026/01/13.
// </copyright>
// -----------------------------------------------------------------------

using jasmsharp;

namespace HowToUseTheDebugAdapter;

internal class Tick : Event;

/// <summary>
///     The traffic light finite state machine (FSM) controller.
/// </summary>
internal class TrafficLight
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="TrafficLight" /> class.
    /// </summary>
    public TrafficLight()
    {
        this.FsmMain =
            FsmSync.Of(
                "traffic light controller",
                this.ControllingDayMode
                    .Entry<bool>(p => Console.WriteLine($"starting day mode    {p}"))
                    .Transition<NoEvent>(this.ControllingNightMode),
                this.ControllingNightMode
                    .Entry<bool>(p => Console.WriteLine($"starting night mode    {p}"))
                    .Transition<NoEvent>(this.ControllingDayMode));
    }

    public FsmSync FsmMain { get; }
    public FsmNightMode ControllingNightMode { get; } = new();
    public FsmDayMode ControllingDayMode { get; } = new();
}