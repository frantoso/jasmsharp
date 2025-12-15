// -----------------------------------------------------------------------
// <copyright file="L2Working.cs">
//     Created by Frank Listing at 2025/12/15.
// </copyright>
// -----------------------------------------------------------------------

namespace HistoryExample;

using jasmsharp;

internal class L2Working : CompositeState
{
    private readonly L3WaStates l3WaStates = new(
        new State("L3wa-State1"),
        new State("L3wa-State2"),
        new State("L3wa-State3")
    );

    private readonly L3WStates l3WStates = new(
        new State("L3w-State 1"),
        new State("L3w-State 2"),
        new State("L3w-State 3"),
        new State("L3w-State 4"),
        new State("L3w-State 5")
    );


    public L2Working() : base("L2-Working")
    {
        this.SubMachines =
        [
            FsmSync.Of(
                "l3w-machine",
                this.l3WStates.State1
                    .Transition<NextEvent>(this.l3WStates.State2),
                this.l3WStates.State2
                    .Transition<NextEvent>(this.l3WStates.State3),
                this.l3WStates.State3
                    .Transition<NextEvent>(this.l3WStates.State4),
                this.l3WStates.State4
                    .Transition<NextEvent>(this.l3WStates.State5),
                this.l3WStates.State5
                    .Transition<NextEvent>(new FinalState())),
            FsmSync.Of(
                "l3wa-machine",
                this.l3WaStates.State1
                    .Transition<NextEvent>(this.l3WaStates.State2),
                this.l3WaStates.State2
                    .Transition<NextEvent>(this.l3WaStates.State3),
                this.l3WaStates.State3
                    .Transition<NextEvent>(new FinalState()))
        ];
    }

    public override IReadOnlyList<FsmSync> SubMachines { get; }

    private record L3WStates(State State1, State State2, State State3, State State4, State State5);

    private record L3WaStates(State State1, State State2, State State3);
}