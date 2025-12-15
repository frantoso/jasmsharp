// -----------------------------------------------------------------------
// <copyright file="MainMachine.cs">
//     Created by Frank Listing at 2025/12/15.
// </copyright>
// -----------------------------------------------------------------------

namespace HistoryExample;

using jasmsharp;

internal class MainMachine
{
    private readonly State stateFinalizing = new("Finalizing");
    private readonly State stateHandlingError = new("Handling-Error");
    private readonly State stateInitializing = new("Initializing");
    private readonly Working stateWorking = new();

    public MainMachine()
    {
        this.Machine = this.CreateFsm();
    }

    public Fsm Machine { get; }

    public void Start()
    {
        this.Machine.Start();
    }

    public void Trigger(Event @event)
    {
        this.Machine.Trigger(@event);
    }

    protected Fsm CreateFsm()
    {
        return FsmSync.Of(
            "m-machine",
            this.stateInitializing
                .Transition<NextEvent>(this.stateWorking),
            this.stateWorking
                .Transition<BreakEvent>(this.stateHandlingError)
                .Transition(this.stateFinalizing),
            this.stateHandlingError
                .Transition<RestartEvent>(this.stateWorking)
                .Transition<ContinueEvent>(this.stateWorking.History)
                .Transition<ContinueDeepEvent>(this.stateWorking.DeepHistory)
                .Transition<NextEvent>(this.stateFinalizing)
                .Transition<BreakEvent>(new FinalState()),
            this.stateFinalizing
                .Transition<NextEvent>(this.stateInitializing));
    }
}