// -----------------------------------------------------------------------
// <copyright file="State.cs">
//     Created by Frank Listing at 2025/11/06.
// </copyright>
// -----------------------------------------------------------------------

namespace jasmsharp;

/// <summary>
///     An interface to be implemented by all states.
/// </summary>
public interface IState
{
    /// <summary>Gets the name of the state.</summary>
    string Name { get; }

    /// <summary>Gets a unique identifier of this object.</summary>
    string Id { get; }
}

/// <summary>
///     An interface to mark a start state (stateFrom in a transition)
/// </summary>
public interface IStartState : IState;

/// <summary>
///     An interface to mark an end state (stateTo in a transition)
/// </summary>
public interface IEndState : IState
{
    /// <summary>Gets the deep history transition end point for this state.</summary>
    TransitionEndPoint DeepHistory { get; }

    /// <summary>Gets the history transition end point for this state.</summary>
    TransitionEndPoint History { get; }
}

/// <summary>
///     Base class for all states of the state machine.
/// </summary>
public abstract class StateBase : IState
{
    private static int instanceCounter;

    /// <summary>Initializes a new instance of the <see cref="StateBase" /> class.</summary>
    /// <param name="name">The name of the state. If null or whitespace, the runtime type name is used.</param>
    protected StateBase(string? name)
    {
        this.Name = string.IsNullOrWhiteSpace(name) ? this.GetType().Name : name;
        var current = Interlocked.Increment(ref StateBase.instanceCounter) - 1;
        this.Id = $"State_{current:0000}";
    }

    /// <summary>Gets the name of this state.</summary>
    public string Name { get; }

    /// <summary>Gets a unique identifier of this object.</summary>
    public string Id { get; }

    /// <summary>Returns a string that represents the current object.</summary>
    /// <returns>The state name.</returns>
    public override string ToString() => this.Name;
}

/// <summary>
///     A base class for all end-state implementations.
/// </summary>
public abstract class EndStateBase(string? name) : StateBase(name), IEndState
{
    private TransitionEndPoint? deepHistory;
    private TransitionEndPoint? history;

    /// <summary>Gets the deep history transition end point for this state.</summary>
    public TransitionEndPoint DeepHistory => this.deepHistory ??= new TransitionEndPoint(this, jasmsharp.History.Hd);

    /// <summary>Gets the history transition end point for this state.</summary>
    public TransitionEndPoint History => this.history ??= new TransitionEndPoint(this, jasmsharp.History.H);
}

/// <summary>
///     A class to model a normal state.
/// </summary>
public class State(string name = "") : EndStateBase(name), IStartState;

/// <summary>
///     A class to model a composite state.
/// </summary>
public abstract class CompositeState(string name = "") : State(name)
{
    /// <summary>Gets the sub state-machines of this composite state.</summary>
    public abstract IReadOnlyList<FsmSync> SubMachines { get; }
}

/// <summary>
///     A class to model the special state 'initial'.
/// </summary>
public sealed class InitialState() : StateBase("Initial"), IStartState;

/// <summary>
///     A class to model the special state 'final'.
/// </summary>
public sealed class FinalState() : EndStateBase("Final")
{
    /// <summary>
    ///     Returns a value indicating whether this instance is equal to the other.
    ///     Special handling for the final state class: The object itself is not relevant, only the type.
    /// </summary>
    public override bool Equals(object? other) => other != null && other.GetType() == this.GetType();

    /// <summary>
    ///     Returns a hash code value for the object.
    /// </summary>
    public override int GetHashCode() => this.GetType().GetHashCode();
}