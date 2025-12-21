// -----------------------------------------------------------------------
// <copyright file="TcpAdapter.cs">
//     Created by Frank Listing at 2025/12/20.
// </copyright>
// -----------------------------------------------------------------------

namespace jasmsharp_debug_adapter;

using System.Net.Sockets;
using Microsoft.Extensions.Configuration;
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
        _ = Task.Run(() => this.Connect(TcpSettings.Read()));
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
    /// <param name="tcpSettings">The host and port to connect to.</param>
    private async Task Connect(TcpSettings tcpSettings)
    {
        while (!this.isDisposed)
        {
            try
            {
                await this.client.ConnectAsync(tcpSettings.Host, tcpSettings.Port);
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

            var message = buffer.ToString(bytesRead);
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

/// <summary>
///     Represents configuration settings for establishing a TCP connection, including the host and port.
/// </summary>
/// <remarks>
///     The TcpSettings class provides methods to load connection settings from configuration files and
///     environment variables. If no explicit values are provided, default values are used for the host and port. This
///     class
///     is typically used to centralize and manage TCP connection parameters in applications that require configurable
///     network endpoints.
/// </remarks>
public class TcpSettings
{
    /// <summary>
    ///     The default host address (localhost).
    /// </summary>
    public const string DefaultHost = "127.0.0.1";

    /// <summary>
    ///     Represents the default port number used for network connections.
    /// </summary>
    public const int DefaultPort = 4000;

    private string? host;
    private int? port;

    /// <summary>
    ///     Gets or sets the host name or IP address used to establish a connection.
    /// </summary>
    public string Host { get => this.host ?? DefaultHost; set => this.host = value; }

    /// <summary>
    ///     Gets or sets the network port number used for the connection.
    /// </summary>
    public int Port { get => this.port ?? DefaultPort; set => this.port = value; }

    /// <summary>
    ///     Reads settings from a configuration file and/or environment variables.
    /// </summary>
    /// <returns>Returns a new <see cref="TcpSettings" /> instance, filled with the information read.</returns>
    /// <remarks>
    ///     The Host and/or Port properties may return the default values if there was no configuration, or it was not
    ///     complete.
    /// </remarks>
    public static TcpSettings Read() => FromConfiguration().FromEnvironment();

    /// <summary>
    ///     Reads settings from a configuration file.
    /// </summary>
    /// <returns>Returns a new <see cref="TcpSettings" /> instance, filled with the information read.</returns>
    /// <remarks>
    ///     The Host and/or Port properties may return the default values if there was no configuration, or it was not
    ///     complete.
    /// </remarks>
    public static TcpSettings FromConfiguration()
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("debug-settings.json", optional: true)
            .Build()
            .GetSection("JasmDebug:TcpSettings");

        var tcpSettings = new TcpSettings
        {
            host = config.GetValue<string?>("Host")?.Trim(' ', '"', '\t'),
            port = config.GetValue<int?>("Port")
        };

        return tcpSettings;
    }

    /// <summary>
    ///     Reads settings from environment variables, if they are not already set.
    /// </summary>
    /// <returns>Returns a reference to this to allow chaining.</returns>
    public TcpSettings FromEnvironment()
    {
        this.host ??= Environment.GetEnvironmentVariable("JASM_DEBUG_HOST")?.Trim(' ', '"', '\t');
        this.port ??= Environment.GetEnvironmentVariable("JASM_DEBUG_PORT")?.ToInt32(DefaultPort);

        return this;
    }

    /// <summary>
    ///     Returns a string that represents the current object.
    /// </summary>
    /// <returns>A string that represents the current object.</returns>
    public override string ToString() => $"Host: {this.Host}, Port: {this.Port}";
}