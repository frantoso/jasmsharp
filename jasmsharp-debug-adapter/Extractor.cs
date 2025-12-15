// -----------------------------------------------------------------------
// <copyright file="Extractor.cs">
//     Created by Frank Listing at 2025/12/15.
// </copyright>
// -----------------------------------------------------------------------

// ReSharper disable UnusedMember.Global

namespace jasmsharp_debug_adapter;

using jasmsharp;

/// <summary>
///     A class representing a transition in the diagram generator.
/// </summary>
public class TransitionInfo(string id, bool isHistory, bool isDeepHistory, bool isToFinal)
{
    /// <summary>
    ///     Gets the end point identifier.
    /// </summary>
    public string EndPointId { get; } = id;

    /// <summary>
    ///     Gets a value indicating whether this transition ends in a history state.
    /// </summary>
    public bool IsHistory { get; } = isHistory;

    /// <summary>
    ///     Gets a value indicating whether this transition ends in a deep history state.
    /// </summary>
    public bool IsDeepHistory { get; } = isDeepHistory;

    /// <summary>
    ///     Gets a value indicating whether this transition ends in the final state.
    /// </summary>
    public bool IsToFinal { get; } = isToFinal;
}

/// <summary>
///     A class representing a state in the diagram generator.
/// </summary>
public class StateInfo(
    string id,
    string name,
    bool isInitial,
    bool isFinal,
    IList<TransitionInfo> transitions,
    IList<FsmInfo> children,
    bool hasHistory,
    bool hasDeepHistory)
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="StateInfo" /> class.
    /// </summary>
    /// <param name="toCopy">The state to copy all information except the history stuff from.</param>
    /// <param name="hasHistory">A value indicating whether this state contains a history end point.</param>
    /// <param name="hasDeepHistory">A value indicating whether this state contains a deep history end point.</param>
    public StateInfo(StateInfo toCopy, bool hasHistory, bool hasDeepHistory) :
        this(
            toCopy.Id,
            toCopy.Name,
            toCopy.IsInitial,
            toCopy.IsFinal,
            toCopy.Transitions,
            toCopy.Children,
            hasHistory,
            hasDeepHistory)
    {
    }

    /// <summary>
    ///     Gets the name of the state.
    /// </summary>
    public string Name { get; } = name;

    /// <summary>
    ///     Gets the identifier of the state.
    /// </summary>
    public string Id { get; } = id;

    /// <summary>
    ///     Gets the transitions.
    /// </summary>
    public IList<TransitionInfo> Transitions { get; } = transitions;

    /// <summary>
    ///     Gets the children.
    /// </summary>
    public IList<FsmInfo> Children { get; } = children;

    /// <summary>
    ///     Gets a value indicating whether this instance is the initial state.
    /// </summary>
    public bool IsInitial { get; } = isInitial;

    /// <summary>
    ///     Gets a value indicating whether this instance is the final state.
    /// </summary>
    public bool IsFinal { get; } = isFinal;

    /// <summary>
    ///     Gets a value indicating whether this instance has a history end point.
    /// </summary>
    public bool HasHistory { get; } = hasHistory;

    /// <summary>
    ///     Gets a value indicating whether this instance has a deep history end point.
    /// </summary>
    public bool HasDeepHistory { get; } = hasDeepHistory;

    /// <summary>
    ///     Returns a string that represents the current object.
    /// </summary>
    /// <returns>A string that represents the current object.</returns>
    public override string ToString() => $"{this.Name}";
}

/// <summary>
///     A class representing a finite state machine in the diagram generator.
/// </summary>
public class FsmInfo(string name, IList<StateInfo> states)
{
    /// <summary>
    ///     Gets the name of the state machine.
    /// </summary>
    public string Name { get; } = name;

    /// <summary>
    ///     Gets the states contained in this machine.
    /// </summary>
    public IList<StateInfo> States { get; } = states;

    /// <summary>
    ///     Returns a string that represents the current object.
    /// </summary>
    /// <returns>A string that represents the current object.</returns>
    public override string ToString() => $"{this.Name}";
}

/// <summary>
///     Extracts all necessary information from an FSM to create a diagram.
/// </summary>
public static class Extractor
{
    public const string FinalStateId = "Final.ID";

    /// <summary>
    ///     Converts a <see cref="ITransition" /> to a <see cref="TransitionInfo" />.
    /// </summary>
    /// <returns>Returns the converted object.</returns>
    public static TransitionInfo Convert(this ITransition transition) =>
        new(
            transition.IsToFinal ? FinalStateId : transition.EndPoint.State.Id,
            transition.EndPoint.History.IsHistory,
            transition.EndPoint.History.IsDeepHistory,
            transition.IsToFinal);

    /// <summary>
    ///     Converts a <see cref="IStateContainer{StateBase}" /> to a <see cref="StateInfo" />.
    /// </summary>
    /// <returns>Returns the converted object.</returns>
    public static StateInfo Convert(this IStateContainer<StateBase> container)
    {
        var transitions = container.Transitions.Select(t => t.Convert()).ToList();
        var children = container.Children.Select(fsm => fsm.Convert()).ToList();
        return new StateInfo(
            container.State is FinalState ? FinalStateId : container.State.Id,
            container.State.Name,
            container.State is InitialState,
            container.State is FinalState,
            transitions,
            children,
            false,
            false);
    }

    /// <summary>
    ///     Updates a <see cref="StateInfo" /> with history information extracted from the specified transitions.
    /// </summary>
    /// <param name="state">The state to update.</param>
    /// <param name="transitions">The transitions to analyse.</param>
    /// <returns>Returns a new state with the updated information.</returns>
    public static StateInfo Update(this StateInfo state, IList<TransitionInfo> transitions)
    {
        var (hasHistory, hasDeepHistory) = transitions
            .Where(transition => transition.EndPointId == state.Id)
            .Aggregate(
                Tuple.Create(false, false),
                (combined, info) => Tuple.Create(
                    combined.Item1 || info.IsHistory,
                    combined.Item2 || info.IsDeepHistory));

        return new StateInfo(state, hasHistory, hasDeepHistory);
    }

    /// <summary>
    ///     Converts a <see cref="Fsm" /> to a <see cref="FsmInfo" />.
    /// </summary>
    /// <returns>Returns the converted object.</returns>
    public static FsmInfo Convert(this Fsm fsm)
    {
        var rawStates = fsm.Initial.MakeList<IStateContainer<StateBase>>().Concat(fsm.States)
            .Select(co => co.Convert()).ToList();
        var transitions = rawStates.SelectMany(it => it.Transitions).ToList();
        var states = rawStates.Select(st => st.Update(transitions)).ToList();

        return new FsmInfo(fsm.Name, states);
    }
}