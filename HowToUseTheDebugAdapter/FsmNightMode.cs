// -----------------------------------------------------------------------
// <copyright file="FsmNightMode.cs">
//     Created by Frank Listing at 2026/01/13.
// </copyright>
// -----------------------------------------------------------------------

namespace HowToUseTheDebugAdapter;

using jasmsharp;

/// <summary>
///     The finite state machine (FSM) for controlling the traffic light in night mode.
/// </summary>
/// <seealso cref="jasmsharp.CompositeState" />
internal class FsmNightMode : CompositeState
{
    private readonly State showingNothing = new("ShowingNothing");
    private readonly State showingYellow = new("ShowingYellow");

    /// <summary>
    ///     Initializes a new instance of the <see cref="FsmNightMode" /> class.
    /// </summary>
    public FsmNightMode() : base("ControllingNightMode")
    {
        this.SubMachines =
        [
            FsmSync.Of(
                "traffic light night mode",
                this.showingYellow
                    .Entry<bool>(p => Console.WriteLine($"x--    {p}"))
                    .Transition<Tick, bool>(this.showingNothing, p => !p)
                    .Transition<Tick, bool>(new FinalState(), p => p),
                this.showingNothing
                    .Entry<bool>(p => Console.WriteLine($"xx-    {p}"))
                    .Transition<Tick>(this.showingYellow)
            )
        ];
    }

    /// <summary>
    ///     Gets the sub state-machines of this composite state.
    /// </summary>
    public override IReadOnlyList<FsmSync> SubMachines { get; }
}