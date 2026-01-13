// -----------------------------------------------------------------------
// <copyright file="FsmDayMode.cs">
//     Created by Frank Listing at 2026/01/13.
// </copyright>
// -----------------------------------------------------------------------

namespace HowToUseTheDebugAdapter;

using jasmsharp;

/// <summary>
///     The finite state machine (FSM) for controlling the traffic light in day mode.
/// </summary>
/// <seealso cref="jasmsharp.CompositeState" />
internal class FsmDayMode : CompositeState
{
    private readonly State showingGreen = new("ShowingGreen");
    private readonly State showingRed = new("ShowingRed");
    private readonly State showingRedYellow = new("ShowingRedYellow");
    private readonly State showingYellow = new("ShowingYellow");

    /// <summary>
    ///     Initializes a new instance of the <see cref="FsmDayMode" /> class.
    /// </summary>
    public FsmDayMode() : base("ControllingDayMode")
    {
        this.SubMachines =
        [
            FsmSync.Of(
                "traffic light day mode",
                this.showingRed
                    .Entry<bool>(p => Console.WriteLine($"x--    {p}"))
                    .Transition<Tick>(this.showingRedYellow),
                this.showingRedYellow
                    .Entry<bool>(p => Console.WriteLine($"xx-    {p}"))
                    .Transition<Tick>(this.showingGreen),
                this.showingGreen
                    .Entry<bool>(p => Console.WriteLine($"--x    {p}"))
                    .Transition<Tick>(this.showingYellow),
                this.showingYellow
                    .Entry<bool>(p => Console.WriteLine($"-x-    {p}"))
                    .Transition<Tick, bool>(this.showingRed, p => p)
                    .Transition<Tick, bool>(new FinalState(), p => !p)
            )
        ];
    }

    public override IReadOnlyList<FsmSync> SubMachines { get; }
}