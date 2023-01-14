using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace VBase
{
#if WINDOWS
    public class RelayCommand : ICommand
    {
        #region Fields 
        readonly Action<object> _execute;
        #endregion // Fields 

        #region Constructors 
        public RelayCommand(Action<object> execute)
        {
            if (execute == null)
                throw new ArgumentNullException("execute");
            _execute = execute;
        }
        #endregion // Constructors 

        #region ICommand Members 
        public bool CanExecute(object parameter)
        {
            return true;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }      
        public void Execute(object parameter)
        {
            Task.Run(() => _execute(parameter));
        }
        #endregion // ICommand Members 
    }
#endif
}
