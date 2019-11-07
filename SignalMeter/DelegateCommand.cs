using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace SignalMeter
{
    public class DelegateCommand : ICommand
    {
        Action<object> execute;
        Func<object, bool> canExecute;
        public event EventHandler CanExecuteChanged 
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameter)
        {
            if (canExecute != null) return canExecute(parameter);
            return true;
        }

        public void Execute(object parameter)
        {
            if(execute != null)
            execute(parameter);
        }

        public DelegateCommand(Action<object> executeAction) : this(executeAction, null) { }
        public DelegateCommand(Action<object> executeAction, Func<object, bool> canExecuteFunc)
        {
            canExecute = canExecuteFunc;
            execute = executeAction;
        }

    }
}
