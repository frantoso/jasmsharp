// -----------------------------------------------------------------------
// <copyright file="DelegateCommand.cs">
//     Created by Frank Listing at 2025/10/07.
// </copyright>
// -----------------------------------------------------------------------

namespace HistoryExampleWpf.Common.Mvvm;

    using System;
    using System.Windows.Input;

    /// <summary>
    ///     A general <see cref="ICommand" /> implementation.
    /// </summary>
    public class DelegateCommand : ICommand
    {
        /// <summary>
        ///     A method used to decide whether the command can be executed.
        /// </summary>
        private readonly Predicate<object?>? canExecute;

        /// <summary>
        ///     The action method of the command.
        /// </summary>
        private readonly Action<object?> execute;

        /// <summary>
        ///     Initializes a new instance of the <see cref="DelegateCommand" /> class.
        /// </summary>
        /// <param name="execute">The action method of the command.</param>
        /// <param name="canExecute">A method used to decide whether the command can be executed.</param>
        /// <exception cref="ArgumentNullException">execute - Execute method missing</exception>
        public DelegateCommand(Action<object?> execute, Predicate<object?>? canExecute = null)
        {
            // ReSharper disable once LocalizableElement
            this.execute = execute ?? throw new ArgumentNullException(nameof(execute), "Execute method missing");
            this.canExecute = canExecute;
        }

        /// <summary>
        ///     Occurs when changes occur that affect whether the command should execute.
        /// </summary>
        public event EventHandler? CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        /// <summary>
        ///     Defines the method that determines whether the command can execute in its current state.
        /// </summary>
        /// <param name="parameter">
        ///     Data used by the command.  If the command does not require data to be passed, this object can
        ///     be set to <see langword="null" />.
        /// </param>
        /// <returns>
        ///     <see langword="true" /> if this command can be executed; otherwise, <see langword="false" />.
        /// </returns>
        public bool CanExecute(object? parameter) => this.canExecute?.Invoke(parameter) ?? true;

        /// <summary>
        ///     Defines the method to be called when the command is invoked.
        /// </summary>
        /// <param name="parameter">
        ///     Data used by the command.  If the command does not require data to be passed, this object can
        ///     be set to <see langword="null" />.
        /// </param>
        public void Execute(object? parameter) => this.execute(parameter);
    }
