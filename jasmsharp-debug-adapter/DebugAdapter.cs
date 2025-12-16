// -----------------------------------------------------------------------
// <copyright file="DebugAdapter.cs">
//     Created by Frank Listing at 2025/12/16.
// </copyright>
// -----------------------------------------------------------------------

namespace jasmsharp_debug_adapter;

using System.Text.Json;
using jasmsharp;
using model;

public class DebugAdapter
{
    private const string GetFsmCommand = "get-fsm";
    private const string SetFsmCommand = "set-fsm";
    private const string UpdateStateCommand = "update-state";
    private const string GetStatesCommand = "get-states";

    public DebugAdapter(Fsm fsm)
    {
        this.Fsm = fsm;
        this.FsmInfo = fsm.Convert();
        this.AllMachines = fsm.AllMachines();

        TcpAdapter.AddCommand(GetFsmCommand, this.OnGetFsm);
        TcpAdapter.AddCommand(GetStatesCommand, this.OnGetStates);
        this.AddStateChangedHandlers(fsm);
    }

    public FsmInfo FsmInfo { get; }

    private Fsm Fsm { get; }

    private IList<Fsm> AllMachines { get; }

    private static void SendAsync<T>(string method, T data) => TcpAdapter.SendAsync(method, data.Serialize());

    private void OnGetStates(JsonElement obj) => this.AllMachines.ForEach(fsm =>
        this.OnStateChanged(fsm, new StateChangedEventArgs(fsm.Initial.State, fsm.CurrentState)));

    private void OnGetFsm(JsonElement obj) => SendAsync(SetFsmCommand, this.FsmInfo);

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
        SendAsync(UpdateStateCommand, stateChangedInfo);
    }
}