// -----------------------------------------------------------------------
// <copyright file="FsmViewModel.cs">
//     Created by Frank Listing at 2025/10/07.
// </copyright>
// -----------------------------------------------------------------------

namespace HistoryExampleWpf.ViewModel;

using Common.Mvvm;
using jasmsharp;

/// <summary>
///     The view model for the state machines.
/// </summary>
/// <seealso cref="Common.Mvvm.ModelBase" />
public class FsmViewModel : ModelBase
{
    /// <summary>
    ///     The associated state machine.
    /// </summary>
    private readonly FsmSync fsm;

    /// <summary>
    ///     The current state.
    /// </summary>
    private string currentState = string.Empty;

    /// <summary>
    ///     A value indicating whether the state machine is inactive.
    /// </summary>
    private bool isInactive = true;

    /// <summary>
    ///     Initializes a new instance of the <see cref="FsmViewModel" /> class.
    /// </summary>
    /// <param name="fsm">The state machine to associate.</param>
    public FsmViewModel(FsmSync fsm)
    {
        this.fsm = fsm;
        this.fsm.StateChanged += this.OnStateChanged;
        this.CurrentState = this.fsm.CurrentState.Name;
    }

    /// <summary>
    ///     Gets or sets the current state.
    /// </summary>
    public string CurrentState
    {
        get => this.currentState;
        set
        {
            this.SetField(ref this.currentState, value);
            this.IsInactive = this.fsm.CurrentState.Equals(new InitialState()) ||
                              this.fsm.HasFinished;
        }
    }

    /// <summary>
    ///     Gets or sets a value indicating whether the state machine is inactive.
    /// </summary>
    public bool IsInactive
    {
        get => this.isInactive;
        set => this.SetField(ref this.isInactive, value);
    }

    /// <summary>
    ///     Called when the state machine changes it's state.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="StateChangedEventArgs" /> instance containing the event data.</param>
    private void OnStateChanged(object? sender, StateChangedEventArgs e)
    {
        this.CurrentState = e.NewState.Name;
    }
}