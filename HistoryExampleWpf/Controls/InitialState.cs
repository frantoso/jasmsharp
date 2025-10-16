// -----------------------------------------------------------------------
// <copyright file="InitialState.cs">
//     Created by Frank Listing at 2025/10/07.
// </copyright>
// -----------------------------------------------------------------------

namespace HistoryExampleWpf.Controls;

using System.Windows;

/// <summary>
/// Draws an initial state.
/// </summary>
/// <seealso cref="HistoryExampleWpf.Controls.State" />
public class InitialState : State
{
    /// <summary> Initializes static members of the <see cref="InitialState" /> class. </summary>
    static InitialState() =>
        FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(
            typeof(InitialState),
            new FrameworkPropertyMetadata(typeof(InitialState)));

    /// <summary> Initializes a new instance of the <see cref="InitialState" /> class. </summary>
    public InitialState() => this.Id = new jasmsharp.InitialState().Name;
}
