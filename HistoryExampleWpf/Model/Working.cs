// -----------------------------------------------------------------------
// <copyright file="Working.cs">
//     Created by Frank Listing at 2025/10/07.
// </copyright>
// -----------------------------------------------------------------------

namespace HistoryExampleWpf.Model;

using System.Diagnostics;
using jasmsharp;

/// <summary>
/// The composite state machine of the working state.
/// </summary>
public class Working
{
    /// <summary> The state 'initializing'. </summary>
    public readonly State StateL2Initializing = new("L2-Initializing");

    /// <summary> The state 'preparing'. </summary>
    public readonly State StateL2Preparing = new("L2-Preparing");

    /// <summary> The state 'working'. </summary>
    public readonly State StateL2Working = new("L2-Working");

    /// <summary> The child state machine of the preparing state. </summary>
    public readonly L2Preparing L2Preparing = new();

    /// <summary> The child state machine of the working state. </summary>
    public readonly L2Working L2Working = new();

    /// <summary> Initializes a new instance of the <see cref="Working" /> class. </summary>
    public Working()
    {
        this.Machine = this.CreateFsm();
    }

    /// <summary> Gets the embedded state machine. </summary>
    public FsmSync Machine { get; }

    /// <summary> Creates the state machine. </summary>
    private FsmSync CreateFsm()
    {
        return FsmSync.Of(
            "Working",
            this.StateL2Initializing
                .Transition<NextEvent>(this.StateL2Preparing)
                .Entry(this.L2InitializingEntry),
            this.StateL2Preparing
                .Transition<NoEvent>(this.StateL2Working)
                .Entry(this.L2PreparingEntry)
                .Child(this.L2Preparing.Machine),
            this.StateL2Working
                .Transition<NoEvent>(new FinalState())
                .Entry(this.L2WorkingEntry)
                .Child(this.L2Working.Machine));
    }

    /// <summary> Handles the entry action of the Initializing state. </summary>
    private void L2InitializingEntry() => Debug.WriteLine(nameof(this.L2InitializingEntry));

    /// <summary> Handles the entry action of the Preparing state. </summary>
    private void L2PreparingEntry() => Debug.WriteLine(nameof(this.L2PreparingEntry));

    /// <summary> Handles the entry action of the Working state. </summary>
    private void L2WorkingEntry() => Debug.WriteLine(nameof(this.L2WorkingEntry));
}