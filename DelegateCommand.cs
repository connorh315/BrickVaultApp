using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BrickVaultApp
{
    public class DelegateCommand(Action<object> execute, Func<object, bool> canExecute = null) : ICommand
    {
        private readonly Action<object> _execute = execute;
        private readonly Func<object, bool> _canExecute = canExecute;

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter) => _canExecute?.Invoke(parameter) ?? true;

        public void Execute(object parameter) => _execute(parameter);

        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}
