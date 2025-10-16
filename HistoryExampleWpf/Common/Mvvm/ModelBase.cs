// -----------------------------------------------------------------------
// <copyright file="ModelBase.cs">
//     Created by Frank Listing at 2025/10/07.
// </copyright>
// -----------------------------------------------------------------------

namespace HistoryExampleWpf.Common.Mvvm;

using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

/// <summary>
///     A class providing the base implementation for the <see cref="INotifyPropertyChanged" /> interface.
/// </summary>
public abstract class ModelBase : INotifyPropertyChanged
{
    /// <summary>
    ///     Occurs when a property value changes.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    ///     Checks if a property already matches a desired value. Sets the property and
    ///     notifies listeners only when necessary.
    /// </summary>
    /// <typeparam name="T">Type of the property.</typeparam>
    /// <param name="field">Reference to a field that stores the property value.</param>
    /// <param name="value">Desired value for the property.</param>
    /// <param name="propertyName">
    ///     Name of the property used to notify listeners. This
    ///     value is optional and is provided automatically.
    /// </param>
    /// <remarks>
    ///     This method is called by the Set accessor of each property.
    ///     The <see cref="CallerMemberNameAttribute" /> attribute that is applied to the optional propertyName
    ///     parameter causes the property name of the caller to be substituted as an argument.
    /// </remarks>
    /// <returns>True if the value was changed, false if the existing value matches the desired value.</returns>
    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null) =>
        this.SetFieldIntern(ref field, value, propertyName);

    /// <summary>
    ///     Notifies about a property change.
    /// </summary>
    /// <param name="propertyName">Name of the property.</param>
    // ReSharper disable once MemberCanBePrivate.Global
    protected void NotifyPropertyChanged(string? propertyName) =>
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    /// <summary>
    ///     Checks if a property already matches a desired value. Sets the property and
    ///     notifies listeners only when necessary.
    /// </summary>
    /// <typeparam name="T">Type of the property.</typeparam>
    /// <param name="field">Reference to a field that stores the property value.</param>
    /// <param name="value">Desired value for the property.</param>
    /// <param name="propertyName">Name of the property used to notify listeners.</param>
    /// <returns>True if the value was changed, false if the existing value matches the desired value.</returns>
    private bool SetFieldIntern<T>(ref T field, T value, string? propertyName)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return false;
        }

        field = value;
        this.NotifyPropertyChanged(propertyName);

        return true;
    }
}