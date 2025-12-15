// -----------------------------------------------------------------------
// <copyright file="Working.cs">
//     Created by Frank Listing at 2025/12/15.
// </copyright>
// -----------------------------------------------------------------------

namespace HistoryExample;

using jasmsharp;

internal class Working : CompositeState
{
    private readonly L2Preparing l2Preparing = new();
    private readonly L2Working l2Working = new();
    private readonly State stateInitializing = new State("L2-Initializing");

    public Working() : base("Working")
    {
        this.SubMachines =
        [
            FsmSync.Of(
                "l2-machine",
                this.stateInitializing
                    .Transition<NextEvent>(this.l2Preparing),
                this.l2Preparing
                    .Transition(this.l2Working),
                this.l2Working
                    .Transition(new FinalState()))
        ];
    }

    public override IReadOnlyList<FsmSync> SubMachines { get; }
}