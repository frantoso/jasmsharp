// -----------------------------------------------------------------------
// <copyright file="TcpAdapter.cs">
//     Created by Frank Listing at 2025/12/17.
// </copyright>
// -----------------------------------------------------------------------

namespace jasmsharp_debug_adapter;

using System.Net.Sockets;
using static jasmsharp.Extensions;

/// <summary>
///     Singleton TCP adapter for communication with the debug server.
/// </summary>
/// <seealso cref="System.IDisposable" />
public class TcpAdapter : IDisposable
{
    private static readonly TimeSpan NextConnectWaitingTime = TimeSpan.FromSeconds(3);

    private static readonly Lazy<TcpAdapter> LazyInstance = new(() => new TcpAdapter());

    private readonly TcpClient client = new();

    private bool isDisposed;

    /// <summary>
    ///     Prevents a default instance of the <see cref="TcpAdapter" /> class from being created.
    /// </summary>
    private TcpAdapter()
    {
        _ = Task.Run(() => this.Connect("127.0.0.1", 4000));
    }

    /// <summary>
    ///     Gets the one and only instance of the <see cref="TcpAdapter" />.
    /// </summary>
    public static TcpAdapter Instance => LazyInstance.Value;

    private Dictionary<string, Action<string>> CommandHandlers { get; } = new();

    /// <summary>
    ///     Sends the provided command and data to the server.
    /// </summary>
    /// <param name="fsm">The state machine addressed by the command.</param>
    /// <param name="command">The command to send.</param>
    /// <param name="data">The data associated with the command.</param>
    public static Task SendAsync(string fsm, string command, string data) =>
        Instance.SendAsync(fsm.ToCommand(command, data).Serialize());

    /// <summary>
    ///     Adds the provided command to the list of handlers.
    /// </summary>
    /// <param name="fsm">The state machine addressed by the command.</param>
    /// <param name="command">The command.</param>
    /// <param name="handler">The handler.</param>
    public static void AddCommand(string fsm, string command, Action<string> handler) =>
        Instance.CommandHandlers.Add(fsm.MakeKey(command), handler);

    /// <summary>
    ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public void Dispose()
    {
        this.Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    ///     Releases unmanaged and - optionally - managed resources.
    /// </summary>
    /// <param name="disposing">
    ///     <c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only
    ///     unmanaged resources.
    /// </param>
    protected virtual void Dispose(bool disposing)
    {
        if (this.isDisposed)
        {
            return;
        }

        if (disposing)
        {
            this.client.Dispose();
        }

        this.isDisposed = true;
    }

    /// <summary>
    ///     Connects to the server and starts the receiver loop.
    ///     If the connection fails, it will retry every 3 seconds until successful.
    /// </summary>
    /// <param name="host">The host to connect to.</param>
    /// <param name="port">The port to use.</param>
    private async Task Connect(string host, int port)
    {
        while (!this.isDisposed)
        {
            try
            {
                await this.client.ConnectAsync(host, port);
                _ = Task.Run(this.ReceiveLoop);
                break;
            }
            catch
            {
                // ignored, server may be not ready yet
                Thread.Sleep(NextConnectWaitingTime);
            }
        }
    }

    /// <summary>
    ///     Sends the provided message to the server.
    /// </summary>
    /// <param name="message">The message to send.</param>
    private async Task SendAsync(string message)
    {
        if (!this.client.Connected)
        {
            return;
        }

        var stream = this.client.GetStream();
        await stream.WriteAsync(message.Compress());
        Console.WriteLine($"Sent: {message}");
    }

    /// <summary>
    ///     Receiver loop to handle messages from the server.
    /// </summary>
    private async Task ReceiveLoop()
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

            message.Deserialize()?.Run(obj => this.ProcessMessage(obj.Fsm, obj.Command, obj.Payload));
        }
    }

    /// <summary>
    ///     Processes a message received from the server.
    /// </summary>
    /// <param name="fsm">The state machine addressed by the command.</param>
    /// <param name="command">The command to execute.</param>
    /// <param name="payload">The payload associated with the command.</param>
    private void ProcessMessage(string fsm, string command, string payload)
    {
        if (!this.CommandHandlers.TryGetValue(fsm.MakeKey(command), out var handler))
        {
            return;
        }

        Console.WriteLine($"Found Handler: {command}");
        handler(payload);
    }

    /// <summary>
    ///     Finalizes an instance of the <see cref="TcpAdapter" /> class.
    /// </summary>
    ~TcpAdapter()
    {
        this.Dispose(false);
    }
}