// -----------------------------------------------------------------------
// <copyright file="L2Preparing.cs">
//     Created by Frank Listing at 2025/12/15.
// </copyright>
// -----------------------------------------------------------------------

namespace HistoryExample;

using jasmsharp;

internal class L2Preparing : CompositeState
{
    private readonly State state1 = new("L3p-State 1");
    private readonly State state2 = new("L3p-State 2");
    private readonly State state3 = new("L3p-State 3");
    private readonly State state4 = new("L3p-State 4");

    public L2Preparing() : base("L2-Preparing")
    {
        this.SubMachines =
        [
            FsmSync.Of(
                "l3p-machine",
                this.state1
                    .Transition<NextEvent>(this.state2),
                this.state2
                    .Transition<NextEvent>(this.state3),
                this.state3
                    .Transition<NextEvent>(this.state4),
                this.state4
                    .Transition<NextEvent>(new FinalState()))
        ];
    }

    public override IReadOnlyList<FsmSync> SubMachines { get; }
}