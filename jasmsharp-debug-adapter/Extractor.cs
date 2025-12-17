// -----------------------------------------------------------------------
// <copyright file="Extractor.cs">
//     Created by Frank Listing at 2025/12/15.
// </copyright>
// -----------------------------------------------------------------------

// ReSharper disable UnusedMember.Global

namespace jasmsharp_debug_adapter;

using jasmsharp;
using model;

/// <summary>
///     Extracts all necessary information from an FSM to create a diagram.
/// </summary>
public static class Extractor
{
    public const string FinalStateId = "Final.ID";

    public static string NormalizedId(this IState state, Fsm owner) =>
        state is FinalState ? $"{owner.Name}-{FinalStateId}" : state.Id;

    /// <summary>
    ///     Converts a <see cref="ITransition" /> to a <see cref="TransitionInfo" />.
    /// </summary>
    /// <returns>Returns the converted object.</returns>
    public static TransitionInfo Convert(this ITransition transition, Fsm owner) =>
        new(
            transition.EndPoint.State.NormalizedId(owner),
            transition.EndPoint.History.IsHistory,
            transition.EndPoint.History.IsDeepHistory,
            transition.IsToFinal);

    /// <summary>
    ///     Converts a <see cref="IStateContainer{StateBase}" /> to a <see cref="StateInfo" />.
    /// </summary>
    /// <returns>Returns the converted object.</returns>
    public static StateInfo Convert(this IStateContainer<StateBase> container, Fsm owner)
    {
        var transitions = container.Transitions.Select(t => t.Convert(owner)).ToList();
        var children = container.Children.Select(fsm => fsm.Convert()).ToList();
        return new StateInfo(
            container.State.NormalizedId(owner),
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
            .Select(co => co.Convert(fsm)).ToList();
        var transitions = rawStates.SelectMany(it => it.Transitions).ToList();
        var states = rawStates.Select(st => st.Update(transitions)).ToList();

        return new FsmInfo(fsm.Name, states);
    }

    /// <summary>
    ///     Converts a <see cref="Fsm" /> to a <see cref="FsmInfo" />.
    /// </summary>
    /// <returns>Returns the converted object.</returns>
    public static List<Fsm> AllMachines(this Fsm fsm)
    {
        return fsm.States.Aggregate(fsm.MakeList(), (list, s) => list.Concat(s.Children.SelectMany(c => c.AllMachines())).ToList());
    }
}