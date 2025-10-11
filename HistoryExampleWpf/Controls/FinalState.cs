// -----------------------------------------------------------------------
// <copyright file="FinalState.cs">
//     Created by Frank Listing at 2025/10/07.
// </copyright>
// -----------------------------------------------------------------------

namespace HistoryExampleWpf.Controls;

using System.Windows;

/// <summary>
/// Draws a final state.
/// </summary>
/// <seealso cref="HistoryExampleWpf.Controls.State" />
public class FinalState : State
{
    /// <summary> Initializes static members of the <see cref="FinalState" /> class. </summary>
    static FinalState() =>
        FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(
            typeof(FinalState),
            new FrameworkPropertyMetadata(typeof(FinalState)));

    /// <summary> Initializes a new instance of the <see cref="FinalState" /> class. </summary>
    public FinalState() => this.Id = new jasmsharp.FinalState().Name;
}
