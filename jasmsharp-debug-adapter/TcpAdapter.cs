using System.Net.Sockets;
using System.Text.Json;
using static jasmsharp.Extensions;

namespace jasmsharp_debug_adapter;

public class TcpAdapter : IDisposable
{
    private readonly TcpClient client = new();

    public async Task ConnectAsync(string host, int port)
    {
        while (true)
        {
            try
            {
                await this.client.ConnectAsync(host, port);
                _ = Task.Run(this.ReceiveLoopAsync);
                break;
            }
            catch
            {
                // ignored, server may be not ready yet
                Thread.Sleep(5000);
            }
        }
    }

    public async Task SendAsync(string message)
    {
        if (!this.client.Connected)
        {
            return;
        }

        var stream = this.client.GetStream();
        await stream.WriteAsync(message.Compress());
        Console.WriteLine($"Sent: {message}");
    }

    public Task SendAsync(string method, string data)
    {
        var message = method.ToCommand(data).Serialize();
        return this.SendAsync(message);
    }

    /// <summary>
    ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public void Dispose()
    {
        this.client.Dispose();
        GC.SuppressFinalize(this);
    }

    private async Task ReceiveLoopAsync()
    {
        var stream = this.client.GetStream();
        var buffer = new byte[1024];

        while (this.client.Connected)
        {
            var bytesRead = await stream.ReadAsync(buffer);
            if (bytesRead == 0)
            {
                // connection closed
                break;
            }

            var message = buffer.ToUtf8(bytesRead);
            Console.WriteLine($"Received: {message}");

            message.Deserialize()?.Run(obj => this.ProcessMessage(obj.Command, obj.Payload));
        }
    }

    private Dictionary<string, Action<JsonElement>> CommandHandlers { get; } = new();

    public void AddCommand(string command, Action<JsonElement> handler)
    {
        this.CommandHandlers[command] = handler;
    }

    private void ProcessMessage(string command, JsonElement payload)
    {
        Console.WriteLine($"Search Handler: {command}");
        if (!this.CommandHandlers.TryGetValue(command, out var handler))
        {
            return;
        }

        Console.WriteLine($"Found Handler: {command}");
        handler(payload);
    }
}

public class JasmCommand(string command, JsonElement payload)
{
    public string Command { get; set; } = command;
    public JsonElement Payload { get; set; } = payload;
}

public class JasmCommandForString(string command, string payload)
{
    public string Command { get; set; } = command;
    public string Payload { get; set; } = payload;
}