// -----------------------------------------------------------------------
// <copyright file="FsmAsync.cs">
//     Created by Frank Listing at 2025/10/07.
// </copyright>
// -----------------------------------------------------------------------

namespace jasmsharp;

/// <summary>
/// A class managing the states of an asynchronous FSM (finite state machine).
/// </summary>
/// <param name="name">The name of the FSM.</param>
/// <param name="startState">The start state (first state) of the FSM.</param>
/// <param name="otherStates">The other states of the FSM.</param>
public class FsmAsync(
    string name,
    EndStateContainer startState,
    List<EndStateContainer> otherStates
) : Fsm(name, startState, otherStates)
{
    /// <summary>
    /// The mutex to synchronize the placing of the events.
    /// It is initially locked and will be unlocked when the state machine is started (that's why semaphore).
    /// Because the trigger method is synchronized, the mutex is not necessary for synchronization - it only blocks
    /// triggering events before calling fsm.start().
    /// </summary>
    private readonly SemaphoreSlim semaphore = new(0, 1);

    /// <summary>
    /// Called when the FSM starts. Allows a derived class to execute additional startup code.
    /// </summary>
    protected override void OnStart()
    {
        this.semaphore.Release();
    }

    /// <summary>
    /// Triggers a transition.
    /// </summary>
    /// <param name="event">The event occurred.</param>
    /// <returns>Returns true.</returns>
    public override bool Trigger(IEvent @event)
    {
        _ = Task.Run(async () =>
        {
            await this.semaphore.WaitAsync();
            try
            {
                this.TriggerEvent(@event);
            }
            finally
            {
                this.semaphore.Release();
            }
        });

        return true;
    }

    /// <summary>
    /// Triggers a transition.
    /// </summary>
    /// <typeparam name="TData">The type of the data parameter.</typeparam>
    /// <param name="event">The event occurred.</param>
    /// <param name="data">The data to send with the event.</param>
    /// <returns>Returns true.</returns>
    public bool Trigger<TData>(Event @event, TData data) => this.Trigger(@event.ToDataEvent(data));

    /// <summary>
    /// Creates an asynchronous FSM from the provided data.
    /// </summary>
    /// <param name="name">The name of the FSM.</param>
    /// <param name="startState">The start state (first state) of the FSM.</param>
    /// <param name="otherStates">The other states of the FSM.</param>
    /// <returns>A new instance of FsmSync.</returns>
    public static FsmAsync Of(string name, EndStateContainer startState, params EndStateContainer[] otherStates)
        => new(name, startState, otherStates.ToList());
}