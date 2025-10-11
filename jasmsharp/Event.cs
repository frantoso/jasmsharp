namespace jasmsharp;

/// <summary>
/// An interface all events must provide.
/// </summary>
public interface IEvent
{
    /// <summary>Gets the type of the event.</summary>
    Type Type { get; }

    /// <summary> Gets a value indicating whether this event is a data event. </summary>
    bool IsDataEvent { get; }

    /// <summary>Gets the data stored in this instance.</summary>
    public object? Data { get; }

    /// <summary>Gets the type of the data stored in this instance.</summary>
    public Type? DataType { get; }
}

/// <summary>
/// A class representing an event.
/// </summary>
public abstract class Event : IEvent
{
    /// <summary>Gets the type of the event.</summary>
    public Type Type => this.GetType();

    /// <summary> Gets a value indicating whether this event is a data event. </summary>
    public bool IsDataEvent => false;

    /// <summary>Returns null.</summary>
    public object? Data => null;

    /// <summary>Gets the type of the data stored in this instance.</summary>
    public Type? DataType => null;

    /// <summary>Returns a string representation of the event - it's name.</summary>
    /// <returns>The name of the event.</returns>
    public override string ToString() => this.GetType().Name;

    /// <summary>Determines whether the specified object is equal to the current object.</summary>
    /// <param name="other">The object to compare with the current object.</param>
    /// <returns>
    /// <see langword="true" /> if the specified object  is equal to the current object; otherwise, <see langword="false" />.</returns>
    public override bool Equals(object? other) => other != null && this.Equals((Event)other);

    /// <summary>Determines whether the specified object is equal to the current object.</summary>
    /// <param name="other">The object to compare with the current object.</param>
    /// <returns>
    /// <see langword="true" /> if the specified object  is equal to the current object; otherwise, <see langword="false" />.</returns>
    private bool Equals(Event other) => this.GetType() == other.GetType();

    /// <summary>Serves as the default hash function.</summary>
    /// <returns>A hash code for the current object.</returns>
    public override int GetHashCode() => this.GetType().GetHashCode();

    /// <summary>
    /// Converts the current object to a data event with the specified data.
    /// </summary>
    /// <typeparam name="TData">The type of the data to be included in the event.</typeparam>
    /// <param name="data">The data to be included in the event.</param>
    /// <returns>An IEvent instance containing the provided data.</returns>
    /// <exception cref="FsmException">Thrown when the event creation fails.</exception>
    public IEvent ToDataEvent<TData>(TData data) =>
        typeof(Event).GetMethod(nameof(Event.CreateDataEvent)) // Get the CreateDataEvent method from Event class
            .Let(methodInfo => (methodInfo?.MakeGenericMethod(this.GetType(), typeof(TData)))
                .Let(generic =>
                    generic?.Invoke(this, [data]) as IEvent ?? throw new FsmException("Could not create DataEvent.")
                ));

    /// <summary>
    /// Creates a new DataEvent instance with the specified data.
    /// </summary>
    /// <typeparam name="TData">The type of the data contained in the event.</typeparam>
    /// <typeparam name="TEvent">The type of the event, which must implement the IEvent interface.</typeparam>
    /// <param name="data">The data to be included in the event.</param>
    /// <returns>A new DataEvent instance containing the provided data.</returns>
    public static DataEvent<TEvent, TData> CreateDataEvent<TEvent, TData>(TData data) where TEvent : IEvent =>
        new(data);
}

public class NoEvent : Event;

internal class StartEvent : Event;

/// <summary>
/// A container to bundle an event with data.
/// It is open to support DataEvent objects with fixed data.
/// </summary>
public class DataEvent<TEvent, TData>(TData data) : IEvent where TEvent : IEvent
{
    /// <summary>Gets the type of the event.</summary>
    public Type Type { get; } = typeof(TEvent);

    /// <summary>Gets the data stored in this instance.</summary>
    public TData Data { get; } = data;

    /// <summary>Gets the data stored in this instance.</summary>
    object? IEvent.Data => this.Data;

    /// <summary>Gets the type of the data stored in this instance.</summary>
    public Type DataType { get; } = typeof(TData);

    /// <summary> Gets a value indicating whether this event is a data event. </summary>
    public bool IsDataEvent => true;

    /// <summary>Returns a string representation of the event - it's name.</summary>
    /// <returns>The name of the event.</returns>
    public override string ToString() => this.Type.Name;

    /// <summary>
    /// Assigns the enclosed data to a new event type.
    /// </summary>
    /// <typeparam name="TNewEvent">The new event type.</typeparam>
    /// <returns>A DataEvent with the same data but changed an event type.</returns>
    public DataEvent<TNewEvent, TData> FromData<TNewEvent>() where TNewEvent : IEvent => new(this.Data);
}