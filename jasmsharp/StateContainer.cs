// -----------------------------------------------------------------------
// <copyright file="StateContainer.cs">
//     Created by Frank Listing at 2025/10/01.
// </copyright>
// -----------------------------------------------------------------------

namespace jasmsharp;

using System.Collections.Immutable;

public interface IStateContainer<out TState> where TState : StateBase
{
    ///<summary>Gets the encapsulated state.</summary>
    TState State { get; }

    /// <summary> Gets a list of all child state machines. </summary>
    IReadOnlyList<FsmSync> Children { get; }

    IAction OnDoInState { get; }

    void Start(IEvent @event, History history);

    ChangeStateData Trigger(IEvent @event);
}

public class StateContainerBase<TState>(
    TState state,
    IReadOnlyList<FsmSync> children,
    IReadOnlyList<ITransition> transitions,
    IAction onEntry,
    IAction onExit,
    IAction onDoInState
) : IStateContainer<TState> where TState : StateBase
{
    public TState State { get; } = state;
    public IReadOnlyList<FsmSync> Children { get; } = children;
    public IReadOnlyList<ITransition> Transitions { get; } = transitions;
    public IAction OnEntry { get; } = onEntry;
    public IAction OnExit { get; } = onExit;
    public IAction OnDoInState { get; } = onDoInState;

    /// <summary>
    /// Gets the name of the enclosed state.
    /// </summary>
    public string Name { get; } = state.Name;

    /// <summary>
    /// Gets a value indicating whether this state has transitions.
    /// </summary>
    public bool HasTransitions { get; } = transitions.Count > 0;

    /// <summary>
    /// Gets a value indicating whether this state has child machines.
    /// </summary>
    public bool HasChildren { get; } = children.Count > 0;

    /// <summary>
    /// The list of currently working child state machines.
    /// </summary>
    private ImmutableList<FsmSync> activeChildren = [];

    private ImmutableList<FsmSync> ActiveChildren
    {
        get
        {
            lock (this)
            {
                return this.activeChildren;
            }
        }
        set
        {
            lock (this)
            {
                this.activeChildren = value;
            }
        }
    }

    /// <summary>
    /// Gets a value indicating whether this state has active child machines.
    /// </summary>
    private bool HasActiveChildren => !this.ActiveChildren.IsEmpty;

    /// <summary>
    /// Calls the OnEntry handler of this state and starts all child FSMs if there are some.
    /// </summary>
    public void Start() => this.Start(new NoEvent(), History.None);

    /// <summary>
    /// Calls the OnEntry handler of this state and starts all child FSMs if there are some.
    /// </summary>
    /// <typeparam name="T">The data type of the data to provide to the entry function.</typeparam>
    /// <param name="data">The data to provide to the entry function.</param>
    public void Start<T>(T data) => this.Start(new DataEvent<NoEvent, T>(data), History.None);

    /// <summary>
    /// Calls the OnEntry handler of this state and starts all child FSMs if there are some.
    /// </summary>
    /// <param name="event">The event which initiated this start.</param>
    /// <param name="history">The kind of history to use.</param>
    public void Start(IEvent @event, History history)
    {
        if ((history.IsHistory && this.TryStartHistory(@event)) ||
            (history.IsDeepHistory && this.TryStartDeepHistory()))
        {
            return;
        }

        this.OnEntry.Fire(@event);
        this.StartChildren(@event);
    }

    /// <summary>
    /// Let all direct child FSMs continue working. Calls Start() if there is no active child.
    /// </summary>
    /// <param name="event">The event which initiated this start.</param>
    /// <returns>Returns a value indicating whether the start was successful.</returns>
    private bool TryStartHistory(IEvent @event)
    {
        if (!this.HasActiveChildren)
        {
            return false;
        }

        foreach (var fsm in this.Children)
        {
            fsm.CurrentStateContainer.Start(@event, History.None);
        }

        return true;
    }

    /// <summary>
    /// Let all child FSMs continue working. Calls Start() if there is no active child.
    /// </summary>
    /// <returns>Returns a value indicating whether the start was successful.</returns>
    private bool TryStartDeepHistory() => this.HasActiveChildren;

    /// <summary>
    /// Triggers a transition.
    /// </summary>
    /// <param name="event">The event occurred.</param>
    /// <returns>Returns the data needed to proceed with the new state.</returns>
    public ChangeStateData Trigger(IEvent @event)
    {
        var result = this.ProcessChildren(@event);

        return result.Item1 ? new ChangeStateData(true) : this.ProcessTransitions(result.Item2);
    }

    /// <summary>
    /// Starts all registered sub state-machines.
    /// </summary>
    /// <param name="event">The event occurred.</param>
    internal void StartChildren(IEvent @event)
    {
        this.ActiveChildren = this.ActiveChildren.Clear();
        foreach (var fsm in this.Children)
        {
            this.StartChild(fsm, @event);
        }
    }

    /// <summary>
    /// Starts the specified child machine.
    /// </summary>
    /// <param name="child">The child machine to start.</param>
    /// <param name="event">The event occurred.</param>
    private void StartChild(FsmSync child, IEvent @event)
    {
        this.ActiveChildren = this.ActiveChildren.Add(child);

        var mi = child.GetGenericStart(@event.DataType);
        if (mi != null)
        {
            mi.Invoke(child, [@event.Data]);
        }
        else
        {
            child.Start();
        }
    }

    /// <summary>
    /// Processes the transitions.
    /// </summary>
    /// <param name="event">The event occurred.</param>
    /// <returns>Returns the data needed to proceed with the new state.</returns>
    private ChangeStateData ProcessTransitions(IEvent @event)
    {
        foreach (var transition in this.Transitions.Where(t => t.IsAllowed(@event)))
        {
            return this.ChangeState(@event, transition);
        }

        return new ChangeStateData(false);
    }

    /// <summary>
    /// Processes the child machines.
    /// </summary>
    /// <param name="event">The event occurred.</param>
    /// <returns>
    /// Returns a tuple where Item1 is true if the event was handled; false otherwise. Normally the returned trigger is the original one.
    /// Exception: If the last child machine went to the final state, Item1 is false and the returned trigger is NoEvent.
    /// </returns>
    private Tuple<bool, IEvent> ProcessChildren(IEvent @event)
    {
        if (!this.HasActiveChildren)
        {
            return Tuple.Create(false, @event);
        }

        var handled = this.TriggerChildren(@event);
        return handled
            ? Tuple.Create(true, @event)
            : Tuple.Create(false, this.HasActiveChildren ? @event : @event.ToNoEvent());
    }

    /// <summary>
    /// Changes the state to the one stored in the transition object.
    /// </summary>
    /// <param name="event">The event occurred.</param>
    /// <param name="transition">The actual transition.</param>
    /// <returns>Returns the data needed to proceed with the new state.</returns>
    private ChangeStateData ChangeState(
        IEvent @event,
        ITransition transition
    )
    {
        this.OnExit.Fire(@event);
        return new ChangeStateData(!transition.IsToFinal, transition.EndPoint);
    }

    /// <summary>
    /// Triggers the child machines.
    /// </summary>
    /// <param name="event">The event occurred.</param>
    /// <returns>Returns true if the event was handled; false otherwise.</returns>
    private bool TriggerChildren(IEvent @event) =>
        // ToList is essential, otherwise the first handled==true will stop processing the other children.
        this.ActiveChildren.Select(fsm => this.TriggerChild(fsm, @event)).ToList().Any(handled => handled);

    /// <summary>
    /// Triggers the specified child machine.
    /// </summary>
    /// <param name="child">The child machine to trigger.</param>
    /// <param name="event">The event occurred.</param>
    /// <returns>Returns true if the event was handled; false otherwise.</returns>
    private bool TriggerChild(
        FsmSync child,
        IEvent @event
    )
    {
        var handled = child.Trigger(@event);
        if (child.HasFinished)
        {
            this.ActiveChildren = this.ActiveChildren.Remove(child);
        }

        return handled;
    }

    /// <summary>Returns a string that represents the current object.</summary>
    /// <returns>A string that represents the current object.</returns>
    public override string ToString() => this.Name;
}

/// <summary>
/// The container for normal states.
/// </summary>
/// <param name="state">The state to encapsulate.</param>
/// <param name="children">Gets a list of all child state machines.</param>
/// <param name="transitions">Gets a list storing the transition information.</param>
/// <param name="onEntry">Gets the handler method for the state entry action.</param>
/// <param name="onExit">Gets the handler method for the state exit action.</param>
/// <param name="onDoInState">Gets the handler method for the states do in state action.</param>
public abstract class EndStateContainer(
    EndStateBase state,
    IReadOnlyList<FsmSync> children,
    IReadOnlyList<ITransition> transitions,
    IAction onEntry,
    IAction onExit,
    IAction onDoInState
) : StateContainerBase<EndStateBase>(state, children, transitions, onEntry, onExit, onDoInState);

/// <summary>
/// The container for normal states.
/// </summary>
/// <param name="state">The state to encapsulate.</param>
/// <param name="children">Gets a list of all child state machines.</param>
/// <param name="transitions">Gets a list storing the transition information.</param>
/// <param name="onEntry">Gets the handler method for the state entry action.</param>
/// <param name="onExit">Gets the handler method for the state exit action.</param>
/// <param name="onDoInState">Gets the handler method for the states do in state action.</param>
public class StateContainer(
    EndStateBase state,
    IReadOnlyList<FsmSync> children,
    IReadOnlyList<ITransition> transitions,
    IAction onEntry,
    IAction onExit,
    IAction onDoInState
) : EndStateContainer(state, children, transitions, onEntry, onExit, onDoInState)
{
    /// <summary>
    /// Adds a new child machine.
    /// </summary>
    /// <param name="stateMachine">The child-machine to add.</param>
    /// <returns>Returns a new state container.</returns>
    public StateContainer Child(FsmSync stateMachine) =>
        new(
            state: this.State,
            children: this.Children.Concat([stateMachine]).ToList(),
            transitions: this.Transitions,
            onEntry: this.OnEntry,
            onExit: this.OnExit,
            onDoInState: this.OnDoInState
        );

    /// <summary>
    /// Adds new child machines.
    /// </summary>
    /// <param name="stateMachines">The children to add.</param>
    /// <returns>Returns a new state container.</returns>
    public StateContainer ChildList(IReadOnlyList<FsmSync> stateMachines) =>
        new(
            state: this.State,
            children: this.Children.Concat(stateMachines).ToList(),
            transitions: this.Transitions,
            onEntry: this.OnEntry,
            onExit: this.OnExit,
            onDoInState: this.OnDoInState
        );

    /// <summary>
    /// Sets the handler method for the state entry action.
    /// </summary>
    /// <param name="action">The handler method for the state entry action.</param>
    /// <returns>Returns a new state container.</returns>
    public StateContainer Entry(IAction action) =>
        new(
            state: this.State,
            children: this.Children,
            transitions: this.Transitions,
            onEntry: action,
            onExit: this.OnExit,
            onDoInState: this.OnDoInState
        );

    /// <summary>
    /// Sets the handler method for the state entry action.
    /// </summary>
    /// <typeparam name="T">The type of the action's parameter.</typeparam>
    /// <param name="action">The handler method for the state entry action.</param>
    /// <returns>Returns a new state container.</returns>
    public StateContainer Entry<T>(System.Action<T?> action) => this.Entry(new Action<T>(action));

    /// <summary>
    /// Sets the handler method for the state entry action.
    /// </summary>
    /// <param name="action">The handler method for the state entry action.</param>
    /// <returns>Returns a new state container.</returns>
    public StateContainer Entry(System.Action action) => this.Entry(new Action(action));

    /// <summary>
    /// Sets the handler method for the state exit action.
    /// </summary>
    /// <param name="action">The handler method for the state exit action.</param>
    /// <returns>Returns a new state container.</returns>
    public StateContainer Exit(IAction action) =>
        new(
            state: this.State,
            children: this.Children,
            transitions: this.Transitions,
            onEntry: this.OnEntry,
            onExit: action,
            onDoInState: this.OnDoInState
        );

    /// <summary>
    /// Sets the handler method for the state exit action.
    /// </summary>
    /// <typeparam name="T">The type of the action's parameter.</typeparam>
    /// <param name="action">The handler method for the state exit action.</param>
    /// <returns>Returns a new state container.</returns>
    public StateContainer Exit<T>(System.Action<T?> action) => this.Exit(new Action<T>(action));

    /// <summary>
    /// Sets the handler method for the state exit action.
    /// </summary>
    /// <param name="action">The handler method for the state exit action.</param>
    /// <returns>Returns a new state container.</returns>
    public StateContainer Exit(System.Action action) => this.Exit(new Action(action));

    /// <summary>
    /// Sets the handler method for the state's Do-In-State action.
    /// </summary>
    /// <param name="action">The handler method for the Do-In-State action.</param>
    /// <returns>Returns a new state container.</returns>
    public StateContainer DoInState(IAction action) =>
        new(
            state: this.State,
            children: this.Children,
            transitions: this.Transitions,
            onEntry: this.OnEntry,
            onExit: this.OnExit,
            onDoInState: action
        );

    /// <summary>
    /// Sets the handler method for the state's Do-In-State action.
    /// </summary>
    /// <typeparam name="T">The type of the action's parameter.</typeparam>
    /// <param name="action">The handler method for the Do-In-State action.</param>
    /// <returns>Returns a new state container.</returns>
    public StateContainer DoInState<T>(System.Action<T?> action) => this.DoInState(new Action<T>(action));

    /// <summary>
    /// Sets the handler method for the state's Do-In-State action.
    /// </summary>
    /// <param name="action">The handler method for the Do-In-State action.</param>
    /// <returns>Returns a new state container.</returns>
    public StateContainer DoInState(System.Action action) => this.DoInState(new Action(action));

    /// <summary>
    /// Adds a new transition to the state.
    /// </summary>
    /// <typeparam name="TEvent">The event type that initiates this transition.</typeparam>
    /// <param name="stateTo">A reference to the end point of this transition.</param>
    /// <param name="guard">Condition handler of this transition.</param>
    /// <returns>Returns a new state container.</returns>
    public StateContainer Transition<TEvent>(IEndState stateTo, Func<bool> guard) where TEvent : Event =>
        new(
            state: this.State,
            children: this.Children,
            transitions: this.Transitions.Concat([new Transition<TEvent>(stateTo, guard)]).ToList(),
            onEntry: this.OnEntry,
            onExit: this.OnExit,
            onDoInState: this.OnDoInState
        );

    /// <summary>
    /// Adds a new transition without an event to a nested state. The event 'NoEvent' is automatically used.
    /// </summary>
    /// <param name="stateTo">A reference to the end point of this transition.</param>
    /// <param name="guard">Condition handler of this transition.</param>
    /// <returns>Returns a new state container.</returns>
    public StateContainer Transition(IEndState stateTo, Func<bool> guard) =>
        new(
            state: this.State,
            children: this.Children,
            transitions: this.Transitions.Concat([new Transition<NoEvent>(stateTo, guard)]).ToList(),
            onEntry: this.OnEntry,
            onExit: this.OnExit,
            onDoInState: this.OnDoInState
        );

    /// <summary>
    /// Adds a new transition without an event to a nested state. The event 'NoEvent' is automatically used.
    /// </summary>
    /// <param name="stateTo">A reference to the end point of this transition.</param>
    /// <returns>Returns a new state container.</returns>
    public StateContainer Transition(IEndState stateTo) => this.Transition(stateTo, () => true);

    /// <summary>
    /// Adds a new transition to the state.
    /// </summary>
    /// <typeparam name="TEvent">The event type that initiates this transition.</typeparam>
    /// <param name="stateTo">A reference to the end point of this transition.</param>
    /// <returns>Returns a new state container.</returns>
    public StateContainer Transition<TEvent>(IEndState stateTo) where TEvent : Event =>
        this.Transition<TEvent>(stateTo, () => true);

    /// <summary>
    /// Adds a new transition to the state.
    /// </summary>
    /// <typeparam name="TEvent">The event type that initiates this transition.</typeparam>
    /// <typeparam name="TData">The type of the action's parameter.</typeparam>
    /// <param name="stateTo">A reference to the end point of this transition.</param>
    /// <param name="guard">Condition handler of this transition.</param>
    /// <returns>Returns a new state container.</returns>
    public StateContainer Transition<TEvent, TData>(IEndState stateTo, Func<TData?, bool> guard) where TEvent : Event =>
        new(
            state: this.State,
            children: this.Children,
            transitions: this.Transitions.Concat([new Transition<TEvent, TData>(stateTo, guard)])
                .ToList(),
            onEntry: this.OnEntry,
            onExit: this.OnExit,
            onDoInState: this.OnDoInState
        );

    /// <summary>
    /// Adds a new transition without an event to a nested state. The event 'NoEvent' is automatically used.
    /// </summary>
    /// <typeparam name="TData">The type of the action's parameter.</typeparam>
    /// <param name="stateTo">A reference to the end point of this transition.</param>
    /// <param name="guard">Condition handler of this transition.</param>
    /// <returns>Returns a new state container.</returns>
    public StateContainer Transition<TData>(IEndState stateTo, Func<TData?, bool> guard) =>
        new(
            state: this.State,
            children: this.Children,
            transitions: this.Transitions.Concat([new Transition<NoEvent, TData>(stateTo, guard)])
                .ToList(),
            onEntry: this.OnEntry,
            onExit: this.OnExit,
            onDoInState: this.OnDoInState
        );

    /// <summary>
    /// Adds a new transition to the state.
    /// </summary>
    /// <typeparam name="TEvent">The event type that initiates this transition.</typeparam>
    /// <typeparam name="TData">The type of the action's parameter.</typeparam>
    /// <param name="stateTo">A reference to the end point of this transition.</param>
    /// <returns>Returns a new state container.</returns>
    public StateContainer Transition<TEvent, TData>(IEndState stateTo) where TEvent : Event =>
        this.Transition<TEvent, TData>(stateTo, _ => true);

    /// <summary>
    /// Adds a new transition to the state.
    /// </summary>
    /// <typeparam name="TEvent">The event type that initiates this transition.</typeparam>
    /// <param name="endPoint">A reference to the end point of this transition.</param>
    /// <param name="guard">Condition handler of this transition.</param>
    /// <returns>Returns a new state container.</returns>
    public StateContainer Transition<TEvent>(TransitionEndPoint endPoint, Func<bool> guard) where TEvent : IEvent =>
        new(
            state: this.State,
            children: this.Children,
            transitions: this.Transitions.Concat([new Transition<TEvent>(endPoint, guard)]).ToList(),
            onEntry: this.OnEntry,
            onExit: this.OnExit,
            onDoInState: this.OnDoInState
        );

    /// <summary>
    /// Adds a new transition to the state.
    /// </summary>
    /// <typeparam name="TEvent">The event type that initiates this transition.</typeparam>
    /// <typeparam name="TData">The type of the action's parameter.</typeparam>
    /// <param name="endPoint">A reference to the end point of this transition.</param>
    /// <param name="guard">Condition handler of this transition.</param>
    /// <returns>Returns a new state container.</returns>
    public StateContainer Transition<TEvent, TData>(TransitionEndPoint endPoint, Func<TData?, bool> guard)
        where TEvent : IEvent =>
        new(
            state: this.State,
            children: this.Children,
            transitions: this.Transitions.Concat([new Transition<TEvent, TData>(endPoint, guard)])
                .ToList(),
            onEntry: this.OnEntry,
            onExit: this.OnExit,
            onDoInState: this.OnDoInState
        );

    /// <summary>
    /// Adds a new transition to the state.
    /// </summary>
    /// <typeparam name="TEvent">The event type that initiates this transition.</typeparam>
    /// <param name="endPoint">A reference to the end point of this transition.</param>
    /// <returns>Returns a new state container.</returns>
    public StateContainer Transition<TEvent>(TransitionEndPoint endPoint) where TEvent : IEvent =>
        this.Transition<TEvent>(endPoint, () => true);

    /// <summary>
    /// Adds a new transition to the state.
    /// </summary>
    /// <typeparam name="TEvent">The event type that initiates this transition.</typeparam>
    /// <typeparam name="TData">The type of the action's parameter.</typeparam>
    /// <param name="endPoint">A reference to the end point of this transition.</param>
    /// <returns>Returns a new state container.</returns>
    public StateContainer Transition<TEvent, TData>(TransitionEndPoint endPoint) where TEvent : IEvent =>
        this.Transition<TEvent, TData>(endPoint, _ => true);

    /// <summary>
    /// Adds a new transition without an event to a nested state. The event 'NoEvent' is automatically used.
    /// </summary>
    /// <param name="endPoint">A reference to the end point of this transition.</param>
    /// <param name="guard">Condition handler of this transition.</param>
    /// <returns>Returns a new state container.</returns>
    public StateContainer Transition(TransitionEndPoint endPoint, Func<bool> guard) =>
        new(
            state: this.State,
            children: this.Children,
            transitions: this.Transitions.Concat([new Transition<NoEvent>(endPoint, guard)]).ToList(),
            onEntry: this.OnEntry,
            onExit: this.OnExit,
            onDoInState: this.OnDoInState
        );

    /// <summary>
    /// Adds a new transition without an event to a nested state. The event 'NoEvent' is automatically used.
    /// </summary>
    /// <typeparam name="TData">The type of the action's parameter.</typeparam>
    /// <param name="endPoint">A reference to the end point of this transition.</param>
    /// <param name="guard">Condition handler of this transition.</param>
    /// <returns>Returns a new state container.</returns>
    public StateContainer Transition<TData>(TransitionEndPoint endPoint, Func<TData?, bool> guard) =>
        new(
            state: this.State,
            children: this.Children,
            transitions: this.Transitions.Concat([new Transition<NoEvent, TData>(endPoint, guard)])
                .ToList(),
            onEntry: this.OnEntry,
            onExit: this.OnExit,
            onDoInState: this.OnDoInState
        );

    /// <summary>
    /// Adds a new transition without an event to a nested state. The event 'NoEvent' is automatically used.
    /// </summary>
    /// <param name="endPoint">A reference to the end point of this transition.</param>
    /// <returns>Returns a new state container.</returns>
    public StateContainer Transition(TransitionEndPoint endPoint) =>
        this.Transition(endPoint, () => true);

    /// <summary>
    /// Adds a new transition to the final state.
    /// </summary>
    /// <typeparam name="TEvent">The event type that initiates this transition.</typeparam>
    /// <param name="guard">Condition handler of this transition.</param>
    /// <returns>Returns a new state container.</returns>
    public StateContainer TransitionToFinal<TEvent>(Func<bool> guard) where TEvent : IEvent =>
        new(
            state: this.State,
            children: this.Children,
            transitions: this.Transitions
                .Concat([new Transition<TEvent>(new FinalState(), guard)]).ToList(),
            onEntry: this.OnEntry,
            onExit: this.OnExit,
            onDoInState: this.OnDoInState
        );

    /// <summary>
    /// Adds a new transition to the final state.
    /// </summary>
    /// <typeparam name="TEvent">The event type that initiates this transition.</typeparam>
    /// <returns>Returns a new state container.</returns>
    public StateContainer TransitionToFinal<TEvent>() where TEvent : IEvent =>
        this.TransitionToFinal<TEvent>(() => true);

    /// <summary>
    /// Adds a new transition to the final state.
    /// </summary>
    /// <typeparam name="TEvent">The event type that initiates this transition.</typeparam>
    /// <typeparam name="TData">The type of the action's parameter.</typeparam>
    /// <param name="guard">Condition handler of this transition.</param>
    /// <returns>Returns a new state container.</returns>
    public StateContainer TransitionToFinal<TEvent, TData>(Func<TData?, bool> guard) where TEvent : IEvent =>
        new(
            state: this.State,
            children: this.Children,
            transitions: this.Transitions
                .Concat([new Transition<TEvent, TData>(new FinalState(), guard)]).ToList(),
            onEntry: this.OnEntry,
            onExit: this.OnExit,
            onDoInState: this.OnDoInState
        );

    /// <summary>
    /// Adds a new transition to the final state.
    /// </summary>
    /// <typeparam name="TEvent">The event type that initiates this transition.</typeparam>
    /// <typeparam name="TData">The type of the action's parameter.</typeparam>
    /// <returns>Returns a new state container.</returns>
    public StateContainer TransitionToFinal<TEvent, TData>() where TEvent : IEvent =>
        this.TransitionToFinal<TEvent, TData>(_ => true);
}

/// <summary>
/// This class represents a particular state of the state machine.
/// </summary>
/// <param name="transitions">Gets a list storing the transition information.</param>
public class InitialStateContainer(
    IReadOnlyList<ITransition> transitions
) : StateContainerBase<InitialState>(
    new InitialState(),
    [],
    transitions,
    new NoAction(),
    new NoAction(),
    new NoAction())
{
    /// <summary>
    /// Adds a single transition to the initial state.
    /// </summary>
    /// <param name="stateTo">A reference to the end point of this transition.</param>
    /// <returns>Returns a new state container.</returns>
    public static InitialStateContainer Transition(IEndState stateTo) =>
        new(transitions: [new Transition<StartEvent>(stateTo, () => true)]);
}

/// <summary>
/// This class represents a particular state of the state machine.
/// </summary>
public class FinalStateContainer() :
    EndStateContainer(new FinalState(), [], [], new NoAction(), new NoAction(), new NoAction());