// -----------------------------------------------------------------------
// <copyright file="DebugAdapter.cs">
//     Created by Frank Listing at 2025/12/17.
// </copyright>
// -----------------------------------------------------------------------

namespace jasmsharp_debug_adapter;

using jasmsharp;
using model;

/// <summary>
///     Provides debugging support for a finite state machine (FSM) by enabling state inspection and synchronization over a
///     TCP connection.
/// </summary>
/// <remarks>
///     The DebugAdapter class registers commands with a TCP adapter to allow external tools to query FSM
///     information and receive state updates in real time. It is typically used to facilitate integration with debugging
///     or
///     visualization tools that require insight into the FSM's structure and current state. Thread safety and
///     responsiveness are managed internally; consumers do not need to handle synchronization when using this
///     class.
/// </remarks>
public class DebugAdapter
{
    private const string GetFsmCommand = "get-fsm";
    private const string SetFsmCommand = "set-fsm";
    private const string ReceivedFsmCommand = "received-fsm";
    private const string UpdateStateCommand = "update-state";
    private const string GetStatesCommand = "get-states";
    private static readonly TimeSpan NextSendWaitingTime = TimeSpan.FromSeconds(3);

    private bool fsmInfoReceived;

    /// <summary>
    ///     Initializes a new instance of the DebugAdapter class and registers command handlers for the specified finite
    ///     state machine (FSM).
    /// </summary>
    /// <param name="fsm">The finite state machine to be debugged. Cannot be null.</param>
    public DebugAdapter(Fsm fsm)
    {
        this.Fsm = fsm;
        this.FsmInfo = fsm.Convert();
        this.AllMachines = fsm.AllMachines();

        TcpAdapter.AddCommand(fsm.Name, GetFsmCommand, this.OnGetFsm);
        TcpAdapter.AddCommand(fsm.Name, GetStatesCommand, this.OnGetStates);
        TcpAdapter.AddCommand(fsm.Name, ReceivedFsmCommand, this.OnFsmReceived);
        this.AddStateChangedHandlers(fsm);

        _ = Task.Run(this.SendFsmInfo);
    }

    private FsmInfo FsmInfo { get; }

    private Fsm Fsm { get; }

    private IList<Fsm> AllMachines { get; }

    private void SendAsync<T>(string method, T data) => TcpAdapter.SendAsync(this.Fsm.Name, method, data.Serialize());

    private void OnFsmReceived(string obj)
    {
        this.fsmInfoReceived = true;
    }

    private void SendFsmInfo()
    {
        while (!this.fsmInfoReceived)
        {
            this.SendAsync(SetFsmCommand, this.FsmInfo);
            Thread.Sleep(NextSendWaitingTime);
        }
    }

    private void OnGetStates(string obj) => this.AllMachines.ForEach(fsm =>
        this.OnStateChanged(fsm, new StateChangedEventArgs(fsm.Initial.State, fsm.CurrentState)));

    private void OnGetFsm(string obj) => this.SendAsync(SetFsmCommand, this.FsmInfo);

    /// <summary>
    ///     Attaches event handlers to the specified finite state machine (FSM) and all of its child states recursively.
    /// </summary>
    /// <param name="fsm">The finite state machine to which event handlers are added. Cannot be null.</param>
    private void AddStateChangedHandlers(Fsm fsm)
    {
        fsm.StateChanged += this.OnStateChanged;
        fsm.States
            .Select(container => container.Children)
            .ForEach(children => children.ForEach(this.AddStateChangedHandlers));
    }

    private void OnStateChanged(object? sender, StateChangedEventArgs e)
    {
        var stateChangedInfo = new StateChangedInfo(sender as Fsm ?? this.Fsm, e);
        this.SendAsync(UpdateStateCommand, stateChangedInfo);
    }
}