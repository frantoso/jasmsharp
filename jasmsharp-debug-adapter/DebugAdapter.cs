namespace jasmsharp_debug_adapter;

using System.Text.Json;
using jasmsharp;

public class DebugAdapter
{
    private const string GetFsmCommand = "get-fsm";
    private const string SetFsmCommand = "set-fsm";

    private readonly TcpAdapter tcpAdapter = new();

    public DebugAdapter(Fsm fsm)
    {
        this.Fsm = fsm;
        this.FsmInfo = fsm.Convert();
        this.tcpAdapter.AddCommand(GetFsmCommand, OnGetFsm);

        _ = Task.Run(() => this.tcpAdapter.ConnectAsync("127.0.0.1", 4000));
    }

    private void OnGetFsm(JsonElement obj)
    {
        var json = this.FsmInfo.Serialize();
        this.tcpAdapter.SendAsync(SetFsmCommand, json);
    }

    private Fsm Fsm { get; }

    public FsmInfo FsmInfo { get; }
}