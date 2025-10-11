// -----------------------------------------------------------------------
// <copyright file="StateContainerExtensions.cs">
//     Created by Frank Listing at 2025/10/01.
// </copyright>
// -----------------------------------------------------------------------

namespace jasmsharp;

public static class StateContainerExtensions
{
    /// <summary>
    /// Encapsulates a normal state in a container.
    /// </summary>
    public static StateContainer ToContainer(this State state) =>
        new(
            state: state,
            children: (state as CompositeState)?.SubMachines ?? [],
            transitions: [],
            onEntry: new NoAction(),
            onExit: new NoAction(),
            onDoInState: new NoAction()
        );

    /// <summary>
    /// Adds a new child machine.
    /// </summary>
    /// <param name="state">The extension object.</param>
    /// <param name="stateMachine">The child machine to add.</param>
    /// <returns>Returns a new state container.</returns>
    public static StateContainer Child(this State state, FsmSync stateMachine)
        => state.ToContainer().Child(stateMachine);

    /// <summary>
    /// Adds new child machines.
    /// </summary>
    /// <param name="state">The extension object.</param>
    /// <param name="stateMachines">The children to add.</param>
    /// <returns>Returns a new state container.</returns>
    public static StateContainer ChildList(this State state, List<FsmSync> stateMachines)
        => state.ToContainer().ChildList(stateMachines);

    /// <summary>
    /// Sets the handler method for the state entry action.
    /// </summary>
    /// <typeparam name="T">The type of the action's parameter.</typeparam>
    /// <param name="state">The extension object.</param>
    /// <param name="action">The handler method for the state entry action.</param>
    /// <returns>Returns a new state container.</returns>
    public static StateContainer Entry<T>(this State state, System.Action<T?> action)
        => state.ToContainer().Entry(action);

    /// <summary>
    /// Sets the handler method for the state entry action.
    /// </summary>
    /// <param name="state">The extension object.</param>
    /// <param name="action">The handler method for the state entry action.</param>
    /// <returns>Returns a new state container.</returns>
    public static StateContainer Entry(this State state, System.Action action) => state.ToContainer().Entry(action);

    /// <summary>
    /// Sets the handler method for the state exit action.
    /// </summary>
    /// <typeparam name="T">The type of the action's parameter.</typeparam>
    /// <param name="state">The extension object.</param>
    /// <param name="action">The handler method for the state exit action.</param>
    /// <returns>Returns a new state container.</returns>
    public static StateContainer Exit<T>(this State state, System.Action<T?> action)
        => state.ToContainer().Exit(action);

    /// <summary>
    /// Sets the handler method for the state exit action.
    /// </summary>
    /// <param name="state">The extension object.</param>
    /// <param name="action">The handler method for the state exit action.</param>
    /// <returns>Returns a new state container.</returns>
    public static StateContainer Exit(this State state, System.Action action) => state.ToContainer().Exit(action);

    /// <summary>
    /// Sets the handler method for the state's Do-In-State action.
    /// </summary>
    /// <typeparam name="T">The type of the action's parameter.</typeparam>
    /// <param name="state">The extension object.</param>
    /// <param name="action">The handler method for the Do-In-State action.</param>
    /// <returns>Returns a new state container.</returns>
    public static StateContainer DoInState<T>(this State state, System.Action<T?> action)
        => state.ToContainer().DoInState(action);

    /// <summary>
    /// Sets the handler method for the state's Do-In-State action.
    /// </summary>
    /// <param name="state">The extension object.</param>
    /// <param name="action">The handler method for the Do-In-State action.</param>
    /// <returns>Returns a new state container.</returns>
    public static StateContainer DoInState(this State state, System.Action action)
        => state.ToContainer().DoInState(action);

    /// <summary>
    /// Adds a new transition to the state.
    /// </summary>
    /// <typeparam name="TEvent">The Event type that initiates this transition.</typeparam>
    /// <param name="state">The extension object.</param>
    /// <param name="stateTo">A reference to the end point of this transition.</param>
    /// <param name="guard">Condition handler of this transition.</param>
    /// <returns>Returns a new state container.</returns>
    public static StateContainer Transition<TEvent>(this State state, IEndState stateTo, Func<bool> guard)
        where TEvent : Event => state.ToContainer().Transition<TEvent>(stateTo, guard);

    /// <summary>
    /// Adds a new transition to the state.
    /// </summary>
    /// <typeparam name="TEvent">The Event type that initiates this transition.</typeparam>
    /// <param name="state">The extension object.</param>
    /// <param name="stateTo">A reference to the end point of this transition.</param>
    /// <returns>Returns a new state container.</returns>
    public static StateContainer Transition<TEvent>(this State state, IEndState stateTo)
        where TEvent : Event => state.ToContainer().Transition<TEvent>(stateTo);

    /// <summary>
    /// Adds a new transition to the state.
    /// </summary>
    /// <typeparam name="TEvent">The event type that initiates this transition.</typeparam>
    /// <typeparam name="TData">The type of the action's parameter.</typeparam>
    /// <param name="state">The extension object.</param>
    /// <param name="stateTo">A reference to the end point of this transition.</param>
    /// <param name="guard">Condition handler of this transition.</param>
    /// <returns>Returns a new state container.</returns>
    public static StateContainer Transition<TEvent, TData>(
        this State state,
        IEndState stateTo,
        Func<TData?, bool> guard
    ) where TEvent : Event => state.ToContainer().Transition<TEvent, TData>(stateTo, guard);

    /// <summary>
    /// Adds a new transition to the state.
    /// </summary>
    /// <typeparam name="TEvent">The event type that initiates this transition.</typeparam>
    /// <typeparam name="TData">The type of the action's parameter.</typeparam>
    /// <param name="state">The extension object.</param>
    /// <param name="stateTo">A reference to the end point of this transition.</param>
    /// <returns>Returns a new state container.</returns>
    public static StateContainer Transition<TEvent, TData>(this State state, IEndState stateTo) where TEvent : Event
        => state.ToContainer().Transition<TEvent, TData>(stateTo);

    /// <summary>
    /// Adds a new transition without an event to a nested state. The event 'NoEvent' is automatically used.
    /// </summary>
    /// <param name="state">The extension object.</param>
    /// <param name="stateTo">A reference to the end point of this transition.</param>
    /// <param name="guard">Condition handler of this transition.</param>
    /// <returns>Returns a new state container.</returns>
    public static StateContainer Transition(this State state, IEndState stateTo, Func<bool> guard)
        => state.ToContainer().Transition(stateTo, guard);

    /// <summary>
    /// Adds a new transition without an event to a nested state. The event 'NoEvent' is automatically used.
    /// </summary>
    /// <param name="state">The extension object.</param>
    /// <param name="stateTo">A reference to the end point of this transition.</param>
    /// <returns>Returns a new state container.</returns>
    public static StateContainer Transition(this State state, IEndState stateTo)
        => state.ToContainer().Transition(stateTo);

    /// <summary>
    /// Adds a new transition without an event to a nested state. The event 'NoEvent' is automatically used.
    /// </summary>
    /// <typeparam name="TData">The type of the action's parameter.</typeparam>
    /// <param name="state">The extension object.</param>
    /// <param name="stateTo">A reference to the end point of this transition.</param>
    /// <param name="guard">Condition handler of this transition.</param>
    /// <returns>Returns a new state container.</returns>
    public static StateContainer Transition<TData>(this State state, IEndState stateTo, Func<TData?, bool> guard)
        => state.ToContainer().Transition(stateTo, guard);

    /// <summary>
    /// Adds a new transition to the state.
    /// </summary>
    /// <typeparam name="TEvent">The event type that initiates this transition.</typeparam>
    /// <param name="state">The extension object.</param>
    /// <param name="endPoint">A reference to the end point of this transition.</param>
    /// <param name="guard">Condition handler of this transition.</param>
    /// <returns>Returns a new state container.</returns>
    public static StateContainer Transition<TEvent>(this State state, TransitionEndPoint endPoint, Func<bool> guard)
        where TEvent : Event => state.ToContainer().Transition<TEvent>(endPoint, guard);

    /// <summary>
    /// Adds a new transition to the state.
    /// </summary>
    /// <typeparam name="TEvent">The event type that initiates this transition.</typeparam>
    /// <param name="state">The extension object.</param>
    /// <param name="endPoint">A reference to the end point of this transition.</param>
    /// <returns>Returns a new state container.</returns>
    public static StateContainer Transition<TEvent>(this State state, TransitionEndPoint endPoint)
        where TEvent : Event => state.ToContainer().Transition<TEvent>(endPoint);

    /// <summary>
    /// Adds a new transition to the state.
    /// </summary>
    /// <typeparam name="TEvent">The event type that initiates this transition.</typeparam>
    /// <typeparam name="TData">The type of the action's parameter.</typeparam>
    /// <param name="state">The extension object.</param>
    /// <param name="endPoint">A reference to the end point of this transition.</param>
    /// <param name="guard">Condition handler of this transition.</param>
    /// <returns>Returns a new state container.</returns>
    public static StateContainer Transition<TEvent, TData>(
        this State state,
        TransitionEndPoint endPoint,
        Func<TData?, bool> guard) where TEvent : Event =>
        state.ToContainer().Transition<TEvent, TData>(endPoint, guard);

    /// <summary>
    /// Adds a new transition to the state.
    /// </summary>
    /// <typeparam name="TEvent">The event type that initiates this transition.</typeparam>
    /// <typeparam name="TData">The type of the action's parameter.</typeparam>
    /// <param name="state">The extension object.</param>
    /// <param name="endPoint">A reference to the end point of this transition.</param>
    /// <returns>Returns a new state container.</returns>
    public static StateContainer Transition<TEvent, TData>(this State state, TransitionEndPoint endPoint)
        where TEvent : Event => state.ToContainer().Transition<TEvent, TData>(endPoint);

    /// <summary>
    /// Adds a new transition without an event to a nested state. The event 'NoEvent' is automatically used.
    /// </summary>
    /// <param name="state">The extension object.</param>
    /// <param name="endPoint">A reference to the end point of this transition.</param>
    /// <param name="guard">Condition handler of this transition.</param>
    /// <returns>Returns a new state container.</returns>
    public static StateContainer Transition(this State state, TransitionEndPoint endPoint, Func<bool> guard)
        => state.ToContainer().Transition(endPoint, guard);

    /// <summary>
    /// Adds a new transition without an event to a nested state. The event 'NoEvent' is automatically used.
    /// </summary>
    /// <param name="state">The extension object.</param>
    /// <param name="endPoint">A reference to the end point of this transition.</param>
    /// <returns>Returns a new state container.</returns>
    public static StateContainer Transition(this State state, TransitionEndPoint endPoint)
        => state.ToContainer().Transition(endPoint);

    /// <summary>
    /// Adds a new transition without an event to a nested state. The event 'NoEvent' is automatically used.
    /// </summary>
    /// <typeparam name="TData">The type of the action's parameter.</typeparam>
    /// <param name="state">The extension object.</param>
    /// <param name="endPoint">A reference to the end point of this transition.</param>
    /// <param name="guard">Condition handler of this transition.</param>
    /// <returns>Returns a new state container.</returns>
    public static StateContainer Transition<TData>(
        this State state,
        TransitionEndPoint endPoint,
        Func<TData?, bool> guard
    ) => state.ToContainer().Transition(endPoint, guard);

    /// <summary>
    /// Adds a new transition to the final state.
    /// </summary>
    /// <typeparam name="TEvent">The event type that initiates this transition.</typeparam>
    /// <typeparam name="TData">The type of the action's parameter.</typeparam>
    /// <param name="state">The extension object.</param>
    /// <param name="guard">Condition handler of this transition.</param>
    /// <returns>Returns a new state container.</returns>
    public static StateContainer TransitionToFinal<TEvent, TData>(this State state, Func<TData?, bool> guard)
        where TEvent : Event => state.ToContainer().TransitionToFinal<TEvent, TData>(guard);

    /// <summary>
    /// Adds a new transition to the final state.
    /// </summary>
    /// <typeparam name="TEvent">The event type that initiates this transition.</typeparam>
    /// <typeparam name="TData">The type of the action's parameter.</typeparam>
    /// <param name="state">The extension object.</param>
    /// <returns>Returns a new state container.</returns>
    public static StateContainer TransitionToFinal<TEvent, TData>(this State state)
        where TEvent : Event => state.ToContainer().TransitionToFinal<TEvent, TData>();

    /// <summary>
    /// Adds a new transition to the final state.
    /// </summary>
    /// <typeparam name="TEvent">The event type that initiates this transition.</typeparam>
    /// <param name="state">The extension object.</param>
    /// <param name="guard">Condition handler of this transition.</param>
    /// <returns>Returns a new state container.</returns>
    public static StateContainer TransitionToFinal<TEvent>(this State state, Func<bool> guard) where TEvent : Event
        => state.ToContainer().TransitionToFinal<TEvent>(guard);

    /// <summary>
    /// Adds a new transition to the final state.
    /// </summary>
    /// <typeparam name="TEvent">The event type that initiates this transition.</typeparam>
    /// <param name="state">The extension object.</param>
    /// <returns>Returns a new state container.</returns>
    public static StateContainer TransitionToFinal<TEvent>(this State state) where TEvent : Event
        => state.ToContainer().TransitionToFinal<TEvent>();

    /// <summary>
    /// Checks the trigger for data and converts it to a NoEvent instance.
    /// </summary>
    /// <param name="event">The event occurred.</param>
    internal static IEvent ToNoEvent(this IEvent @event) =>
        @event.IsDataEvent
            .LetIf(() => @event.GetType().GetMethod(nameof(DataEvent<NoEvent, int>.FromData))
                ?.Let(method =>
                    method.MakeGenericMethod(typeof(NoEvent))
                        .Let(genericMethod => genericMethod.Invoke(@event, []) as IEvent)
                )
            ) ?? new NoEvent();
}