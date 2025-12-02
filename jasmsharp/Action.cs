// -----------------------------------------------------------------------
// <copyright file="Action.cs">
//     Created by Frank Listing at 2025/11/06.
// </copyright>
// -----------------------------------------------------------------------

namespace jasmsharp;

/// <summary>The interface an action must provide.</summary>
public interface IAction
{
    /// <summary>When implemented fires the action.</summary>
    /// <param name="event">The event which originally started the process which results in this action.</param>
    void Fire(IEvent @event);
}

/// <summary>Fake-action doing nothing.</summary>
public class NoAction :
    IAction
{
    /// <summary>Does nothing.</summary>
    /// <param name="event">The event which originally started the process which results in this action.</param>
    public void Fire(IEvent @event)
    {
    }
}

/// <summary>Encapsulates an action.</summary>
/// <param name="action">The function to execute.</param>
public class Action(System.Action action) : IAction
{
    /// <summary>Fires the action.</summary>
    /// <param name="event">The event which originally started the process which results in this action.</param>
    /// <exception cref="FsmException">Thrown if the action throws an exception.</exception>
    public void Fire(IEvent @event)
    {
        try
        {
            action();
        }
        catch (Exception ex)
        {
            throw new FsmException("Error calling the action", "?", ex);
        }
    }
}

/// <summary>Encapsulates an action with a parameter of a state.</summary>
/// <typeparam name="TData">The data type of the action's parameter.</typeparam>
/// <param name="action">The function to execute.</param>
public class Action<TData>(System.Action<TData?> action) : IAction
{
    /// <summary>Fires the action. </summary>
    /// <param name="event">The event which originally started the process which results in this action.</param>
    public void Fire(IEvent @event) =>
        @event.IsDataEvent
            .And(() => typeof(TData).IsAssignableFrom(@event.DataType))
            .If(() => this.CallAction((TData?)@event.Data))
            .Else(() => this.CallAction(default)); // Not a matching DataEvent => pass default (null) to the action

    /// <summary>Fires the action with provided data.</summary>
    /// <param name="data">The data to provide as a parameter.</param>
    /// <exception cref="FsmException">Thrown if the action throws an exception.</exception>
    private void CallAction(TData? data)
    {
        try
        {
            action(data);
        }
        catch (Exception ex)
        {
            throw new FsmException("Error calling the action", "?", ex);
        }
    }
}