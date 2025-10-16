// -----------------------------------------------------------------------
// <copyright file="L2Working.cs">
//     Created by Frank Listing at 2025/10/07.
// </copyright>
// -----------------------------------------------------------------------

namespace HistoryExampleWpf.Model;

using System.Diagnostics;
using jasmsharp;

/// <summary>
/// The composite state machine of the L2-Working state.
/// </summary>
public class L2Working
{
    /// <summary> The state 'w1'. </summary>
    public readonly State StateL3W1 = new("L3-W1");

    /// <summary> The state 'w2'. </summary>
    public readonly State StateL3W2 = new("L3-W2");

    /// <summary> The state 'w3'. </summary>
    public readonly State StateL3W3 = new("L3-W3");

    /// <summary> The state 'w4'. </summary>
    public readonly State StateL3W4 = new("L3-W4");

    /// <summary> The state 'w5'. </summary>
    public readonly State StateL3W5 = new("L3-W5");

    /// <summary> Initializes a new instance of the <see cref="L2Working" /> class. </summary>
    public L2Working()
    {
        this.Machine = this.CreateFsm();
    }

    /// <summary> Gets the embedded state machine. </summary>
    public FsmSync Machine { get; }

    /// <summary> Creates the state machine. </summary>
    private FsmSync CreateFsm() =>
        FsmSync.Of(
            "L2-Working",
            this.StateL3W1
                .Transition<NextEvent>(this.StateL3W2)
                .Entry(this.W1Entry),
            this.StateL3W2
                .Transition<NextEvent>(this.StateL3W3)
                .Entry(this.W2Entry),
            this.StateL3W3
                .Transition<NextEvent>(this.StateL3W4)
                .Entry(this.W3Entry),
            this.StateL3W4
                .Transition<NextEvent>(this.StateL3W5)
                .Entry(this.W4Entry),
            this.StateL3W5
                .Transition<NextEvent>(new FinalState())
                .Entry(this.W5Entry));

    /// <summary> Handles the entry action of the W5 state. </summary>
    private void W5Entry() => Debug.WriteLine(nameof(this.W5Entry));

    /// <summary> Handles the entry action of the W4 state. </summary>
    private void W4Entry() => Debug.WriteLine(nameof(this.W4Entry));

    /// <summary> Handles the entry action of the W3 state. </summary>
    private void W3Entry() => Debug.WriteLine(nameof(this.W3Entry));

    /// <summary> Handles the entry action of the W2 state. </summary>
    private void W2Entry() => Debug.WriteLine(nameof(this.W2Entry));

    /// <summary> Handles the entry action of the W1 state. </summary>
    private void W1Entry() => Debug.WriteLine(nameof(this.W1Entry));
}