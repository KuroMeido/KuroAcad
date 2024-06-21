using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace KuroAcad.UI
{
    class RelayCommand :ICommand
    {
        readonly Action<object> execute;
        readonly Predicate<object> canExecute;

        /// <summary>
        /// Creates a new instance of RelayCommand.
        /// </summary>
        /// <param name="execute">Action to execute.</param>
        /// <param name="canExecute">Predicate indicating whether the action can be executed.</param>
        public RelayCommand(Action<object> execute, Predicate<object> canExecute)
        {
            this.execute = execute;
            this.canExecute = canExecute;
        }

        /// <summary>
        /// Executes the action passed as a parameter to the constructor.
        /// </summary>
        /// <param name="parameter">Parameter of the action (can be null).</param>
        public void Execute(object parameter) => execute(parameter);

        /// <summary>
        /// Executes the predicate passed as a parameter to the constructor.
        /// </summary>
        /// <param name="parameter">Parameter of the predicate (can be null).</param>
        /// <returns>Result of the predicate execution.</returns>
        public bool CanExecute(object parameter) => canExecute(parameter);

        /// <summary>
        /// Event indicating that the return value of the predicate has changed.
        /// </summary>
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

    }
}
