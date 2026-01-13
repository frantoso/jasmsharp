// -----------------------------------------------------------------------
// <copyright file="Controller.cs">
//     Created by Frank Listing at 2026/01/13.
// </copyright>
// -----------------------------------------------------------------------

namespace HowToUseTheDebugAdapter;

using jasmsharp_debug_adapter;

/// <summary>
///     The controller class manages the main finite state machine (FSM) for the traffic light system.
/// </summary>
internal class Controller
{
    /// <summary>
    ///     Initializes a new instance of the Controller class.
    /// </summary>
    /// <remarks>
    ///     This constructor creates a new MainMachine instance and registers the necessary handlers for
    ///     the controller.
    /// </remarks>
    public Controller()
    {
        this.TrafficLight = new TrafficLight();
        DebugAdapter.Of(this.TrafficLight.FsmMain);
        this.TrafficLight.FsmMain.Start();
    }

    /// <summary>
    ///     Gets the main finite state machine (FSM) that controls the primary workflow of the system.
    /// </summary>
    public TrafficLight TrafficLight { get; }

    /// <summary>
    ///     Gets a value indicating whether the controller is in day mode.
    /// </summary>
    public bool IsDayMode { get; private set; } = true;

    /// <summary>
    ///     Runs the main interactive loop, processing user input to control the finite state machine.
    /// </summary>
    /// <remarks>
    ///     Pressing 's' starts the main finite state machine. Pressing 'q' exits the loop. Any other key
    ///     triggers an event mapped to that key, or a default event if no mapping exists. This method blocks until the user
    ///     chooses to exit.
    /// </remarks>
    public void Run()
    {
        ShowHelp();

        while (true)
        {
            var input = char.ToLower(Console.ReadKey().KeyChar);
            Console.WriteLine();
            switch (input)
            {
                case 'h':
                    ShowHelp();
                    break;
                case 'q':
                    return;
                case 's':
                    this.IsDayMode = !this.IsDayMode;
                    break;
                default:
                    this.TrafficLight.FsmMain.Trigger(new Tick(), this.IsDayMode);
                    break;
            }
        }
    }

    /// <summary>
    ///     Displays a list of available keyboard commands and their descriptions to the console.
    /// </summary>
    private static void ShowHelp()
    {
        Console.WriteLine("Press a key to control the state machine.");
        Console.WriteLine("q: Quit the application");
        Console.WriteLine("h: Shows help (this text)");
        Console.WriteLine("s: Switches between day- and night-mode");
        Console.WriteLine("Any other key: Trigger Tick-Event");
        Console.WriteLine();
    }
}