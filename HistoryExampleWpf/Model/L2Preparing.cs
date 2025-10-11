// -----------------------------------------------------------------------
// <copyright file="L2Preparing.cs">
//     Created by Frank Listing at 2025/10/07.
// </copyright>
// -----------------------------------------------------------------------

namespace HistoryExampleWpf.Model;

using System.Diagnostics;
using jasmsharp;

/// <summary>
/// The composite state machine of the L2-Preparing state.
/// </summary>
public class L2Preparing
{
    /// <summary> The state 'p1'. </summary>
    public readonly State StateL3P1 = new("L3-P1");

    /// <summary> The state 'p2'. </summary>
    public readonly State StateL3P2 = new("L3-P2");

    /// <summary> The state 'p3'. </summary>
    public readonly State StateL3P3 = new("L3-P3");

    /// <summary> The state 'p4'. </summary>
    public readonly State StateL3P4 = new("L3-P4");

    /// <summary> Initializes a new instance of the <see cref="L2Preparing" /> class. </summary>
    public L2Preparing()
    {
        this.Machine = this.CreateFsm();
    }

    /// <summary> Gets the embedded state machine. </summary>
    public FsmSync Machine { get; }

    /// <summary> Creates the state machine. </summary>
    private FsmSync CreateFsm() =>
        FsmSync.Of(
            "L2-Preparing",
            this.StateL3P1
                .Transition<NextEvent>(this.StateL3P2)
                .Entry(this.P1Entry),
            this.StateL3P2
                .Transition<NextEvent>(this.StateL3P3)
                .Entry(this.P2Entry),
            this.StateL3P3
                .Transition<NextEvent>(this.StateL3P4)
                .Entry(this.P3Entry),
            this.StateL3P4
                .Transition<NextEvent>(new FinalState())
                .Entry(this.P4Entry));

    /// <summary> Handles the entry action of the P1 state. </summary>
    private void P1Entry() => Debug.WriteLine(nameof(this.P1Entry));

    /// <summary> Handles the entry action of the P2 state. </summary>
    private void P2Entry() => Debug.WriteLine(nameof(this.P2Entry));

    /// <summary> Handles the entry action of the P3 state. </summary>
    private void P3Entry() => Debug.WriteLine(nameof(this.P3Entry));

    /// <summary> Handles the entry action of the P4 state. </summary>
    private void P4Entry() => Debug.WriteLine(nameof(this.P4Entry));
}