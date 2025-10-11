// -----------------------------------------------------------------------
// <copyright file="State.cs">
//     Created by Frank Listing at 2025/10/07.
// </copyright>
// -----------------------------------------------------------------------

namespace HistoryExampleWpf.Controls;

using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;

/// <summary>
///     Draws a state.
/// </summary>
/// <seealso cref="System.Windows.Controls.Control" />
public class State : Control
{
    /// <summary>
    ///     A dependency property to get or set the selected border brush.
    /// </summary>
    public static readonly DependencyProperty SelectedBorderBrushProperty = DependencyProperty.Register(
        nameof(State.SelectedBorderBrush),
        typeof(Brush),
        typeof(State),
        new PropertyMetadata(default(Brush)));

    /// <summary>
    ///     A dependency property to get or set the display name.
    /// </summary>
    public static readonly DependencyProperty DisplayNameProperty = DependencyProperty.Register(
        nameof(State.DisplayName),
        typeof(string),
        typeof(State),
        new PropertyMetadata(default(string)));

    /// <summary>
    ///     A dependency property to get or set the identifier.
    /// </summary>
    public static readonly DependencyProperty IdProperty = DependencyProperty.Register(
        nameof(State.Id),
        typeof(string),
        typeof(State),
        new PropertyMetadata(default(string)));

    /// <summary>
    ///     A dependency property to get or set the current state.
    /// </summary>
    public static readonly DependencyProperty CurrentStateProperty = DependencyProperty.Register(
        nameof(State.CurrentState),
        typeof(string),
        typeof(State),
        new PropertyMetadata(null, State.OnStateChanged));

    /// <summary>
    ///     A dependency property to get or set a value indicating whether this instance is selected.
    /// </summary>
    public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.Register(
        nameof(State.IsSelected),
        typeof(bool),
        typeof(State),
        new PropertyMetadata(false));

    /// <summary>
    ///     A dependency property to get or set the selected border thickness.
    /// </summary>
    public static readonly DependencyProperty SelectedBorderThicknessProperty = DependencyProperty.Register(
        nameof(State.SelectedBorderThickness),
        typeof(Thickness),
        typeof(State),
        new PropertyMetadata(default(Thickness)));

    /// <summary>
    ///     A dependency property to get or set a value indicating whether this is a composite state.
    /// </summary>
    public static readonly DependencyProperty IsCompositeProperty = DependencyProperty.Register(
        nameof(State.IsComposite),
        typeof(bool),
        typeof(State),
        new PropertyMetadata(false));

    /// <summary>
    ///     A dependency property to get or set the selected background.
    /// </summary>
    public static readonly DependencyProperty SelectedBackgroundProperty = DependencyProperty.Register(
        nameof(State.SelectedBackground),
        typeof(Brush),
        typeof(State),
        new PropertyMetadata(default(Brush)));

    /// <summary>
    ///     A dependency property to get or set the corner radius.
    /// </summary>
    public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.Register(
        nameof(State.CornerRadius),
        typeof(CornerRadius),
        typeof(State),
        new PropertyMetadata(default(CornerRadius)));

    /// <summary>
    ///     Initializes static members of the <see cref="State" /> class.
    /// </summary>
    static State() =>
        FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(
            typeof(State),
            new FrameworkPropertyMetadata(typeof(State)));

    /// <summary>
    ///     Gets or sets a value indicating whether this instance is selected.
    /// </summary>
    public bool IsSelected
    {
        get => (bool)this.GetValue(State.IsSelectedProperty);
        set => this.SetValue(State.IsSelectedProperty, value);
    }

    /// <summary>
    ///     Gets or sets the selected border brush.
    /// </summary>
    public Brush SelectedBorderBrush
    {
        get => (Brush)this.GetValue(State.SelectedBorderBrushProperty);
        set => this.SetValue(State.SelectedBorderBrushProperty, value);
    }

    /// <summary>
    ///     Gets or sets the selected border thickness.
    /// </summary>
    public Thickness SelectedBorderThickness
    {
        get => (Thickness)this.GetValue(State.SelectedBorderThicknessProperty);
        set => this.SetValue(State.SelectedBorderThicknessProperty, value);
    }

    /// <summary>
    ///     Gets or sets the selected background.
    /// </summary>
    public Brush SelectedBackground
    {
        get => (Brush)this.GetValue(State.SelectedBackgroundProperty);
        set => this.SetValue(State.SelectedBackgroundProperty, value);
    }

    /// <summary>
    ///     Gets or sets the display name.
    /// </summary>
    public string DisplayName
    {
        get => (string)this.GetValue(State.DisplayNameProperty);
        set => this.SetValue(State.DisplayNameProperty, value);
    }

    /// <summary>
    ///     Gets or sets the identifier.
    /// </summary>
    public string Id
    {
        get => (string)this.GetValue(State.IdProperty);
        set => this.SetValue(State.IdProperty, value);
    }

    /// <summary>
    ///     Gets or sets the current state.
    /// </summary>
    public string CurrentState
    {
        get => (string)this.GetValue(State.CurrentStateProperty);
        set => this.SetValue(State.CurrentStateProperty, value);
    }

    /// <summary>
    ///     Gets or sets a value indicating whether this is a composite state.
    /// </summary>
    public bool IsComposite
    {
        get => (bool)this.GetValue(State.IsCompositeProperty);
        set => this.SetValue(State.IsCompositeProperty, value);
    }

    /// <summary>
    ///     Gets or sets the corner radius.
    /// </summary>
    public CornerRadius CornerRadius
    {
        get => (CornerRadius)this.GetValue(State.CornerRadiusProperty);
        set => this.SetValue(State.CornerRadiusProperty, value);
    }

    /// <summary>
    ///     Called when the state has changed.
    /// </summary>
    /// <param name="d">The d.</param>
    /// <param name="e">The <see cref="DependencyPropertyChangedEventArgs" /> instance containing the event data.</param>
    private static void OnStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) =>
        (d as State)?.OnStateChanged($"{e.NewValue}");

    /// <summary>
    ///     Called when the state has changed.
    /// </summary>
    /// <param name="currentState">The name of the current state.</param>
    private void OnStateChanged(string currentState) => this.IsSelected = currentState == this.Id;
}