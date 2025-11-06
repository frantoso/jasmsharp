// -----------------------------------------------------------------------
// <copyright file="Transition.cs">
//     Created by Frank Listing at 2025/11/06.
// </copyright>
// -----------------------------------------------------------------------

namespace jasmsharp;

/// <summary>
///     An interface defining all information needed to process a transition.
/// </summary>
public interface ITransition
{
    /// <summary>Gets the type of the event that initiates this transition.</summary>
    Type EventType { get; }

    /// <summary>Gets the end point of this transition.</summary>
    TransitionEndPoint EndPoint { get; }

    /// <summary>Gets a value indicating whether the end point is the final state.</summary>
    bool IsToFinal { get; }

    /// <summary>Returns a value indicating whether the transition is allowed for the given event.</summary>
    bool IsAllowed(IEvent @event);
}

/// <summary>
///     A base class for all transitions.
/// </summary>
/// <typeparam name="TEvent">The event type that initiates this transition.</typeparam>
/// <param name="endPoint">A reference to the end point of this transition.</param>
public abstract class TransitionBase<TEvent>(TransitionEndPoint endPoint) where TEvent : IEvent
{
    /// <summary>Gets the type of the event that initiates this transition.</summary>
    public Type EventType { get; } = typeof(TEvent);

    /// <summary>Gets the end point of this transition.</summary>
    public TransitionEndPoint EndPoint { get; } = endPoint;

    /// <summary>Gets a value indicating whether the end point is the final state.</summary>
    public bool IsToFinal { get; } = endPoint.State is FinalState;
}

/// <summary>
///     A class holding all information about a transition.
/// </summary>
/// <typeparam name="TEvent">The event type that initiates this transition.</typeparam>
/// <param name="endPoint">A reference to the end point of this transition.</param>
/// <param name="guard">Condition handler of this transition.</param>
public sealed class Transition<TEvent>(TransitionEndPoint endPoint, Func<bool> guard)
    : TransitionBase<TEvent>(endPoint), ITransition where TEvent : IEvent
{
    /// <summary>Alternative initialization with state as an endpoint.</summary>
    /// <param name="state">The destination state of this transition.</param>
    /// <param name="guard">Condition handler of this transition.</param>
    public Transition(IEndState state, Func<bool> guard)
        : this(new TransitionEndPoint(state), guard)
    {
    }

    /// <summary>Gets the guard delegate.</summary>
    internal Func<bool> Guard { get; } = guard;

    /// <inheritdoc />
    public bool IsAllowed(IEvent @event) =>
        // Accept if the runtime event type is the same or a subclass of EventType
        typeof(TEvent).IsAssignableFrom(@event.Type) && this.Guard();
}

/// <summary>
///     A class holding all information about a data transition.
/// </summary>
/// <typeparam name="TEvent">The event type that initiates this transition.</typeparam>
/// <typeparam name="TData">The data type that must be carried by the event.</typeparam>
/// <param name="endPoint">A reference to the end point of this transition.</param>
/// <param name="guard">Condition handler of this transition.</param>
public sealed class Transition<TEvent, TData>(TransitionEndPoint endPoint, Func<TData?, bool> guard)
    : TransitionBase<TEvent>(endPoint), ITransition where TEvent : IEvent
{
    /// <summary>Initializes a new instance of the <see cref="Transition{TData}" /> class.</summary>
    /// <summary>
    ///     Alternative initialization with state as an end point.
    /// </summary>
    /// <param name="state">A reference to the destination state of this transition.</param>
    /// <param name="guard">Condition handler of this transition.</param>
    public Transition(IEndState state, Func<TData?, bool> guard)
        : this(new TransitionEndPoint(state), guard)
    {
    }

    /// <summary>Gets the guard delegate.</summary>
    internal Func<TData?, bool> Guard { get; } = guard;

    /// <inheritdoc />
    public bool IsAllowed(IEvent @event)
    {
        // Accept only DataEvent<T, TEvent> where T matches the generic parameter
        if (!typeof(TEvent).IsAssignableFrom(@event.Type) || !@event.IsDataEvent ||
            !typeof(TData).IsAssignableFrom(@event.DataType))
        {
            return false;
        }

        return this.Guard((TData?)@event.Data);
    }
}