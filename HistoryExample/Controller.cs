// -----------------------------------------------------------------------
// <copyright file="Controller.cs">
//     Created by Frank Listing at 2025/12/15.
// </copyright>
// -----------------------------------------------------------------------

namespace HistoryExample;

using jasmsharp;

/// <summary>
///     Provides control logic for managing and interacting with the main finite state machine (FSM) and its associated
///     events in a console-based environment.
/// </summary>
/// <remarks>
///     The Controller class initializes the main FSM and sets up event handlers for state changes, including
///     those of nested FSMs. It processes user input from the console to trigger state transitions or control commands,
///     such as starting the FSM or exiting the application. This class is intended for internal use and is not
///     thread-safe.
/// </remarks>
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
        this.MainFsm = new MainMachine();
        this.AddHandlers(this.MainFsm.Machine);
    }

    /// <summary>
    ///     Gets the mapping of event key characters to their corresponding event instances.
    /// </summary>
    public Dictionary<char, Event> EventMappings { get; } = new()
    {
        { 'n', new NextEvent() },
        { 'b', new BreakEvent() },
        { 'c', new ContinueEvent() },
        { 'd', new ContinueDeepEvent() },
        { 'r', new RestartEvent() }
    };

    /// <summary>
    ///     Gets the main finite state machine (FSM) that controls the primary workflow of the system.
    /// </summary>
    public MainMachine MainFsm { get; }

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
                    this.MainFsm.Start();
                    break;
                default:
                    this.MainFsm.Trigger(
                        this.EventMappings.FirstOrDefault(m => m.Key == input).Value ?? new NextEvent());
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
        Console.WriteLine("s: Start the main state machine");
        Console.WriteLine("n: Trigger NextEvent");
        Console.WriteLine("b: Trigger BreakEvent");
        Console.WriteLine("c: Trigger ContinueEvent");
        Console.WriteLine("d: Trigger ContinueDeepEvent");
        Console.WriteLine("r: Trigger RestartEvent");
        Console.WriteLine("Any other key: Trigger NextEvent");
        Console.WriteLine();
    }

    /// <summary>
    ///     Handles the event that is raised when the state of a finite state machine changes.
    /// </summary>
    /// <param name="sender">The finite state machine instance whose state has changed.</param>
    /// <param name="e">An object that contains the event data, including the previous and new states.</param>
    private static void OnStateChanged(object? sender, StateChangedEventArgs e)
    {
        Console.WriteLine($"{(sender as Fsm)?.Name}: {e.OldState} ==> {e.NewState}");
    }

    /// <summary>
    ///     Attaches event handlers to the specified finite state machine (FSM) and all of its child states recursively.
    /// </summary>
    /// <param name="fsm">The finite state machine to which event handlers are added. Cannot be null.</param>
    private void AddHandlers(Fsm fsm)
    {
        fsm.StateChanged += OnStateChanged;
        fsm.States
            .Select(container => container.Children)
            .ForEach(children => children.ForEach(this.AddHandlers));
    }
}