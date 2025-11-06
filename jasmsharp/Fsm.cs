// -----------------------------------------------------------------------
// <copyright file="Fsm.cs">
//     Created by Frank Listing at 2025/11/06.
// </copyright>
// -----------------------------------------------------------------------

namespace jasmsharp;

/// <summary>
///     A class managing the states of an FSM (finite state machine).
/// </summary>
public abstract class Fsm
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="Fsm" /> class.
    ///     A class managing the states of an FSM (finite state machine).
    /// </summary>
    /// <param name="name">The name of the FSM.</param>
    /// <param name="startState">The start state (first state) of the FSM.</param>
    /// <param name="otherStates">The other states of the FSM.</param>
    protected Fsm(
        string name,
        EndStateContainer startState,
        List<EndStateContainer> otherStates)
    {
        this.Name = name;
        this.Initial = InitialStateContainer.Transition(startState.State);
        this.CurrentStateContainer = this.Initial;
        this.States = new List<EndStateContainer>([startState]).Concat(otherStates)
            .Concat(Fsm.DestinationOnlyStates([startState], otherStates))
            .Concat(Fsm.FinalStateOrNot([startState], otherStates))
            .ToList();

        Fsm.CheckMisuseOfNoEvent(this.States);
    }

    /// <summary>Informs about a state change.</summary>
    /// <remarks>
    ///     This event is fired before the OnEntry handler of the state is called.
    ///     It should be used mainly for informational purposes.
    /// </remarks>
    public event EventHandler<StateChangedEventArgs>? StateChanged;

    /// <summary>Informs about a state change. </summary>
    /// <remarks>
    ///     This event is fired before a state is changed.
    ///     It should be used mainly for informational purposes.
    /// </remarks>
    public event EventHandler<TriggerEventArgs>? Triggered;

    /// <summary>
    ///     Gets the name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    ///     A list of all states excluding the initial state.
    /// </summary>
    public IReadOnlyList<EndStateContainer> States { get; }

    /// <summary>
    ///     Gets the container of the currently active state.
    /// </summary>
    public IStateContainer<StateBase> CurrentStateContainer { get; private set; }

    /// <summary>
    ///     Gets the currently active state.
    /// </summary>
    public IState CurrentState => this.CurrentStateContainer.State;

    /// <summary>
    ///     Gets the currently active state and, if available, all active child states.
    /// </summary>
    public StateTreeNode CurrentStateTree =>
        new(
            this.CurrentState,
            [.. this.CurrentStateContainer.Children.Select(fsm => fsm.CurrentStateTree)]);

    /// <summary>
    ///     Gets the currently active state container and, if available, all active child containers.
    /// </summary>
    public StateContainerTreeNode CurrentStateContainerTree =>
        new(
            this.CurrentStateContainer,
            [.. this.CurrentStateContainer.Children.Select(fsm => fsm.CurrentStateContainerTree)]);

    /// <summary>
    ///     Gets a value indicating whether the automaton is started and has not reached the final state.
    /// </summary>
    public bool IsRunning => this.CurrentState is not InitialState && !this.HasFinished;

    /// <summary>
    ///     Gets a value indicating whether the automaton has reached the final state.
    /// </summary>
    public bool HasFinished => this.CurrentState is FinalState;

    /// <summary>
    ///     Gets an object implementing the debug interface. This allows access to special functions which are mainly provided
    ///     for tests.
    /// </summary>
    public IDebug DebugInterface => new DebugImpl(this);

    /// <summary>
    ///     Gets the initial state.
    /// </summary>
    private InitialStateContainer Initial { get; }

    /// <summary>
    ///     Starts the behavior of the Fsm class. Executes the transition from the start state to the first user-defined state.
    ///     This method calls the initial state’s OnEntry method.
    /// </summary>
    public void Start()
    {
        this.CurrentStateContainer = this.Initial;
        this.TriggerEvent(new StartEvent());
        this.OnStart();
    }

    /// <summary>
    ///     Starts the behavior of the Fsm class. Executes the transition from the start state to the first user-defined state.
    ///     This method calls the initial state’s OnEntry method.
    /// </summary>
    /// <typeparam name="T">The type of the data parameter.</typeparam>
    /// <param name="data">The data to provide to the function.</param>
    public void Start<T>(T data)
    {
        this.CurrentStateContainer = this.Initial;
        this.TriggerEvent(new DataEvent<StartEvent, T>(data));
        this.OnStart();
    }

    /// <summary>
    ///     Fires the parametrized Do event of the current state.
    /// </summary>
    /// <typeparam name="T">The type of the data parameter.</typeparam>
    /// <param name="data">The data to provide to the function.</param>
    public void DoAction<T>(T data) => this.CurrentStateContainer.OnDoInState.Fire(new DataEvent<NoEvent, T>(data));

    /// <summary>
    ///     Fires the Do event of the current state.
    /// </summary>
    public void DoAction() => this.CurrentStateContainer.OnDoInState.Fire(new NoEvent());

    /// <summary>
    ///     Returns a string representation of the FSM - its name.
    /// </summary>
    public override string ToString() => this.Name;

    /// <summary>
    ///     Triggers a transition.
    /// </summary>
    /// <param name="event">The event occurred.</param>
    /// <returns>
    ///     Returns true if the event was handled; false otherwise. In the case of asynchronous processing, it returns
    ///     true.
    /// </returns>
    public abstract bool Trigger(IEvent @event);

    /// <summary>
    ///     Called when the FSM starts. Allows a derived class to execute additional startup code.
    /// </summary>
    protected virtual void OnStart()
    {
    }

    /// <summary>
    ///     Triggers a transition.
    /// </summary>
    /// <param name="event">The event occurred.</param>
    /// <returns>
    ///     Returns true if the event was handled; false otherwise. In the case of asynchronous processing, it returns
    ///     true.
    /// </returns>
    protected bool TriggerEvent(IEvent @event)
    {
        lock (this)
        {
            Fsm.CheckParameter(@event);

            var changeStateData = this.CurrentStateContainer.Trigger(@event);
            this.RaiseTriggered(@event, changeStateData.Handled);
            this.ActivateState(@event, changeStateData);

            return changeStateData.Handled;
        }
    }

    /// <summary>
    ///     Gets the final state (as a list) or an empty list if there is no final state used in the transitions.
    /// </summary>
    private static List<EndStateContainer> FinalStateOrNot(
        List<EndStateContainer> startState,
        List<EndStateContainer> otherStates)
    {
        var allContainers = otherStates
            .Concat(startState)
            .SelectMany(state => state.Transitions);

        return allContainers.Any(t => t.IsToFinal) ? [new FinalStateContainer()] : [];
    }

    /// <summary>
    ///     Gets a list with states used only as destination in a transition.
    ///     It's not a normal use case, but it may happen.
    /// </summary>
    private static List<StateContainer> DestinationOnlyStates(
        List<EndStateContainer> startState,
        List<EndStateContainer> otherStates
    )
    {
        return startState
            .Concat(otherStates).ToArray()
            .Let(knownStates => knownStates.SelectMany(state =>
                    state.Transitions.Select(transition => transition.EndPoint.State))
                .Distinct().OfType<State>().Where(state => !knownStates.Select(x => x.State).Contains(state))
                .Select(state => state.ToContainer()).ToList());
    }

    /// <summary>
    ///     Checks whether NoEvent is misused in the provided states.
    ///     NoEvent can only be used for transitions in nested states.
    /// </summary>
    /// <param name="states">The list of states to check.</param>
    /// <exception cref="FsmException">Thrown when the event <see>NoEvent</see> is used at a wrong transition.</exception>
    private static void CheckMisuseOfNoEvent(IReadOnlyList<EndStateContainer> states)
    {
        var isNoEventMisused = states.SelectMany(state =>
                state.Transitions.Select(transition => Tuple.Create(state, transition)
                ))
            .Any(data =>
                !data.Item1.HasChildren && data.Item2.EventType == typeof(NoEvent));

        if (isNoEventMisused)
        {
            throw new FsmException("A transition without event can only be used for nested states!");
        }
    }

    /// <summary>
    ///     Checks whether the provided parameter is valid.
    /// </summary>
    /// <param name="event">The event parameter to check.</param>
    private static void CheckParameter(IEvent @event)
    {
        if (@event.Type == typeof(NoEvent))
        {
            throw new FsmException("Fsm.trigger: A trigger event cannot be NoEvent!");
        }
    }

    /// <summary>
    ///     Activates the new state.
    /// </summary>
    /// <param name="event">The event occurred.</param>
    /// <param name="changeStateData">The data needed to activate the next state.</param>
    private void ActivateState(IEvent @event, ChangeStateData changeStateData)
    {
        if (changeStateData.EndPoint == null)
        {
            return;
        }

        var oldState = this.CurrentStateContainer;
        this.CurrentStateContainer = this.FindContainer(changeStateData.EndPoint.State);
        this.RaiseStateChanged(oldState, this.CurrentStateContainer);
        this.CurrentStateContainer.Start(@event, changeStateData.EndPoint.History);
    }

    /// <summary>
    ///     Gets the state container of the provided state. Will crash if the state is not in the list of states.
    /// </summary>
    private EndStateContainer FindContainer(IEndState endState) => this.States.First(it => it.State.Equals(endState));

    /// <summary>
    ///     Raises the state-changed event.
    /// </summary>
    /// <param name="oldState">The old state.</param>
    /// <param name="newState">The new state.</param>
    private void RaiseStateChanged(
        IStateContainer<StateBase> oldState,
        IStateContainer<StateBase> newState
    )
    {
        try
        {
            this.StateChanged?.Invoke(this, new StateChangedEventArgs(oldState.State, newState.State));
        }
        catch (Exception ex)
        {
            throw new FsmException("Error calling onStateChanged on machine $name.", "", ex);
        }
    }

    /// <summary>
    ///     Raises the triggered event.
    /// </summary>
    /// <param name="event">The event which was processed.</param>
    /// <param name="handled">A value indicating whether the event was handled (true) or not (false).</param>
    private void RaiseTriggered(IEvent @event, bool handled)
    {
        try
        {
            this.Triggered?.Invoke(this, new TriggerEventArgs(this.CurrentState, @event, handled));
        }
        catch (Exception ex)
        {
            throw new FsmException("Error calling onTriggered on machine $name.", "", ex);
        }
    }

    /// <summary>
    ///     Sets the provided state as an active state.
    /// </summary>
    /// <param name="state">The state to set as the current state.</param>
    private void SetState(EndStateContainer state)
    {
        var stateBefore = this.CurrentStateContainer;
        this.CurrentStateContainer = state;
        this.RaiseStateChanged(stateBefore, this.CurrentStateContainer);
    }

    /// <summary>
    ///     Sets the provided state as an active state and starts child machines if present, e.g., to resume after a reboot.
    ///     IMPORTANT: This method does not call the entry function of the state.
    /// </summary>
    /// <param name="state">The state to set as the current state.</param>
    private void Resume(State state)
    {
        var container = this.FindContainer(state);
        this.SetState(container);
        container.StartChildren(new NoEvent());
    }

    /// <summary>
    ///     This interface provides methods which are not intended to use for normal operation.
    ///     Use the methods for testing purposes or to recover from a reboot.
    /// </summary>
    public interface IDebug
    {
        /// <summary>
        ///     Should set the provided state as an active state.
        /// </summary>
        /// <param name="state">The state to set as the current state.</param>
        void SetState(State state);

        /// <summary>
        ///     Sets the provided state as an active state and starts child machines if present, e.g., to resume after a reboot.
        ///     IMPORTANT: This method does not call the entry function of the state.
        /// </summary>
        /// <param name="state">The state to set as the current state.</param>
        void Resume(State state);

        /// <summary>
        ///     Triggers a transition synchronously. Independent of the concrete implementation, it calls the synchronous trigger
        ///     function from the base class.
        /// </summary>
        /// <param name="event">The event occurred.</param>
        /// <returns>Returns true if the event was handled; false otherwise.</returns>
        bool TriggerSync(IEvent @event);

        /// <summary>
        ///     Triggers a transition synchronously. Independent of the concrete implementation, it calls the synchronous trigger
        ///     function from the base class.
        /// </summary>
        /// <typeparam name="T">The type of the data parameter.</typeparam>
        /// <param name="event">The event occurred.</param>
        /// <param name="data">The data to send with the event.</param>
        /// <returns>Returns true if the event was handled; false otherwise.</returns>
        bool TriggerSync<T>(Event @event, T data) => this.TriggerSync(@event.ToDataEvent(data));
    }

    private class DebugImpl(Fsm fsm) : IDebug
    {
        private Fsm Fsm { get; } = fsm;

        public void SetState(State state) => this.Fsm.SetState(this.Fsm.FindContainer(state));

        public void Resume(State state) => this.Fsm.Resume(state);

        public bool TriggerSync(IEvent @event) => this.Fsm.TriggerEvent(@event);
    }
}