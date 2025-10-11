// -----------------------------------------------------------------------
// <copyright file="Main.cs">
//     Created by Frank Listing at 2025/10/07.
// </copyright>
// -----------------------------------------------------------------------

namespace HistoryExampleWpf.Model;

using System.Diagnostics;
using jasmsharp;

/// <summary>
/// The main state machine.
/// </summary>
public class Main
{
    /// <summary> The state 'initializing'. </summary>
    public readonly State StateInitializing = new("Main-Initializing");

    /// <summary> The state 'working'. </summary>
    public readonly State StateWorking = new("Main-Working");

    /// <summary> The state 'handling error'. </summary>
    public readonly State StateHandlingError = new("Main-HandlingError");

    /// <summary> The state 'finalizing'. </summary>
    public readonly State StateFinalizing = new("Main-Finalizing");

    /// <summary> The child state machine of the working state. </summary>
    public readonly Working Working = new();

    /// <summary> Initializes a new instance of the <see cref="Main" /> class. </summary>
    public Main()
    {
        this.Machine = this.CreateFsm();
    }

    /// <summary> Gets the embedded state machine. </summary>
    public FsmSync Machine { get; }

    /// <summary> Starts the behavior of the state machine. </summary>
    public void Start() => this.Machine.Start();

    /// <summary> Triggers the specified event. </summary>
    /// <param name="event">The trigger.</param>
    public void Trigger(IEvent @event) => this.Machine.Trigger(@event);

    /// <summary> Creates the state machine. </summary>
    private FsmSync CreateFsm()
    {
        return FsmSync.Of(
            "Main",
            this.StateInitializing
                .Transition<NextEvent>(this.StateWorking),
            this.StateWorking
                .Transition<BreakEvent>(this.StateHandlingError)
                .Transition<NoEvent>(this.StateFinalizing)
                .Entry(this.WorkingEntry)
                .Child(this.Working.Machine),
            this.StateHandlingError
                .Transition<RestartEvent>(this.StateWorking)
                .Transition<ContinueEvent>(this.StateWorking.History)
                .Transition<ContinueDeepEvent>(this.StateWorking.DeepHistory)
                .Transition<NextEvent>(this.StateFinalizing)
                .Transition<BreakEvent>(new FinalState())
                .Entry(this.HandlingErrorEntry),
            this.StateFinalizing
                .Transition<NextEvent>(this.StateInitializing)
                .Entry(this.FinalizingEntry));
    }

    /// <summary> Handles the entry action of the Finalizing state. </summary>
    private void FinalizingEntry() => Debug.WriteLine(nameof(this.FinalizingEntry));

    /// <summary> Handles the entry action of the HandlingError state. </summary>
    private void HandlingErrorEntry() => Debug.WriteLine(nameof(this.HandlingErrorEntry));

    /// <summary> Handles the entry action of the Working state. </summary>
    private void WorkingEntry() => Debug.WriteLine(nameof(this.WorkingEntry));
}