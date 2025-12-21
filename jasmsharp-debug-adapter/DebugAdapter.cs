// -----------------------------------------------------------------------
// <copyright file="DebugAdapter.cs">
//     Created by Frank Listing at 2025/12/21.
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
    private const string GetStatesCommand = "get-states";
    private const string SetFsmCommand = "set-fsm";
    private const string UpdateStateCommand = "update-state";
    private const string ReceivedFsmCommand = "received-fsm";
    private static readonly TimeSpan NextSendWaitingTime = TimeSpan.FromSeconds(3);

    private bool fsmInfoAcknowledged;

    /// <summary>
    ///     Initializes a new instance of the DebugAdapter class and registers command handlers for the specified finite
    ///     state machine (FSM).
    /// </summary>
    /// <param name="fsm">The finite state machine to be debugged. Cannot be null.</param>
    public DebugAdapter(Fsm fsm)
    {
        this.FsmInfo = fsm.Convert();
        this.AllMachines = fsm.AllMachines();

        TcpAdapter.AddCommand(fsm.Name, GetStatesCommand, this.OnGetStates);
        TcpAdapter.AddCommand(fsm.Name, ReceivedFsmCommand, this.OnFsmReceived);
        this.AddStateChangedHandlers(fsm);

        _ = Task.Run(this.SendFsmInfo);
    }

    /// <summary>
    ///     Gets the FSM information.
    /// </summary>
    private FsmInfo FsmInfo { get; }

    /// <summary>
    ///     Gets a list of all machines, used to send the currently active states.
    /// </summary>
    private IList<Fsm> AllMachines { get; }

    /// <summary>
    ///     Helper to send the data to the server.
    /// </summary>
    /// <typeparam name="T">The type of the data to send</typeparam>
    /// <param name="command">The command.</param>
    /// <param name="data">The data.</param>
    private void SendAsync<T>(string command, T data) =>
        TcpAdapter.SendAsync(this.FsmInfo.Name, command, data.Serialize());

    /// <summary>
    ///     Handles the acknowledgement of the server. Stops periodically sending the FSM info.
    /// </summary>
    /// <param name="ignored">The command data (ignored).</param>
    private void OnFsmReceived(string ignored)
    {
        this.fsmInfoAcknowledged = true;
    }

    /// <summary>
    ///     Sends the FSM information until the server sent an acknowledgement.
    /// </summary>
    private void SendFsmInfo()
    {
        while (!this.fsmInfoAcknowledged)
        {
            this.SendAsync(SetFsmCommand, this.FsmInfo);
            Thread.Sleep(NextSendWaitingTime);
        }
    }

    /// <summary>
    ///     Sends the info about the current active of all state machines.
    /// </summary>
    /// <param name="ignored">The command data (ignored).</param>
    private void OnGetStates(string ignored) => this.AllMachines.ForEach(fsm =>
        this.OnStateChanged(fsm, new StateChangedEventArgs(fsm.Initial.State, fsm.CurrentState)));

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

    /// <summary>
    ///     Called when the state of the FSM has changed.
    /// </summary>
    /// <param name="sender">The sending state machine.</param>
    /// <param name="e">The <see cref="StateChangedEventArgs" /> instance containing the event data.</param>
    private void OnStateChanged(object? sender, StateChangedEventArgs e)
    {
        var stateChangedInfo = new StateChangedInfo((sender as Fsm)?.Name ?? this.FsmInfo.Name, e);
        this.SendAsync(UpdateStateCommand, stateChangedInfo);
    }
}