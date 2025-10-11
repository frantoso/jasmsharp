// -----------------------------------------------------------------------
// <copyright file="ViewModel.cs">
//     Created by Frank Listing at 2025/10/07.
// </copyright>
// -----------------------------------------------------------------------

namespace HistoryExampleWpf.ViewModel;

using System.Collections.Generic;
using System.Windows.Input;
using Common.Mvvm;
using jasmsharp;
using Model;

/// <summary>
/// The view model of the application.
/// </summary>
/// <seealso cref="Common.Mvvm.ModelBase" />
public class ViewModel : ModelBase
{
    /// <summary> A table which associates a keyboard key with an event. </summary>
    private static readonly Dictionary<string, IEvent> EventTable = new()
    {
        {
            "B", Events.Break
        },
        {
            "C", Events.Continue
        },
        {
            "D", Events.ContinueDeep
        },
        {
            "N", Events.Next
        },
        {
            "R", Events.Restart
        }
    };

    /// <summary> The main state machine. </summary>
    private readonly Main mainFsm = new ();

    /// <summary> Initializes a new instance of the <see cref="ViewModel" /> class. </summary>
    public ViewModel()
    {
        this.TriggerCommand = new DelegateCommand(this.TriggerExecuted);
        this.RestartCommand = new DelegateCommand(this.RestartExecuted);

        this.Main = new FsmViewModel(this.mainFsm.Machine);
        this.Working = new FsmViewModel(this.mainFsm.Working.Machine);
        this.L2Preparing = new FsmViewModel(this.mainFsm.Working.L2Preparing.Machine);
        this.L2Working = new FsmViewModel(this.mainFsm.Working.L2Working.Machine);

        this.mainFsm.Start();
    }

    /// <summary> Gets the trigger command. </summary>
    public ICommand TriggerCommand { get; }

    /// <summary> Gets the restart command. </summary>
    public ICommand RestartCommand { get; }

    /// <summary> Gets the view model for the main state machine. </summary>
    public FsmViewModel Main { get; }

    /// <summary> Gets the view model for the state machine of the Working state. </summary>
    public FsmViewModel Working { get; }

    /// <summary> Gets the view model for the state machine of the L2Preparing state. </summary>
    public FsmViewModel L2Preparing { get; }

    /// <summary> Gets the view model for the state machine of the L2Working state. </summary>
    public FsmViewModel L2Working { get; }

    /// <summary> Executes the restart command. </summary>
    /// <param name="parameter">The parameter of the command.</param>
    private void RestartExecuted(object? parameter) => this.mainFsm.Start();

    /// <summary> Executes the trigger command. </summary>
    /// <param name="parameter">The parameter of the command.</param>
    private void TriggerExecuted(object? parameter)
    {
        var key = $"{parameter}".ToUpper();
        if (ViewModel.EventTable.TryGetValue(key, out var @event))
        {
            this.mainFsm.Trigger(@event);
        }
    }
}