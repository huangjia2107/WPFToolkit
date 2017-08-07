using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using MvvmUtils.Models;

namespace MvvmUtils.Command
{
    public class RelayCommand<T> : ICommand
    {
        private readonly WeakAction<T> _execute;
        private readonly WeakFuncT<T, bool> _canExecute;

        private EventHandler _requerySuggestedLocal;

        public RelayCommand(Action<T> execute)
            : this(execute, null)
        {

        }

        public RelayCommand(Action<T> execute, Func<T, bool> canExecute)
        {
            if (execute == null)
                throw new ArgumentNullException("execute");

            _execute = new WeakAction<T>(execute);
            if (canExecute != null)
                _canExecute = new WeakFuncT<T, bool>(canExecute);
        }

        public bool CanExecute(object parameter)
        {
            if (_canExecute == null)
                return true;

            if (this._canExecute.IsStatic || this._canExecute.IsAlive)
            {
                if (parameter == null && typeof(T).IsValueType)
                    return this._canExecute.Execute(default(T));

                if (parameter == null || parameter is T)
                    return this._canExecute.Execute((T)((object)parameter));
            }
            return false;
        } 

        public void Execute(object parameter)
        {
            object obj = parameter;
            if (parameter != null && parameter.GetType() != typeof(T) && parameter is IConvertible)
                obj = Convert.ChangeType(parameter, typeof(T), null);

            if (CanExecute(obj) && _execute != null && (_execute.IsStatic || _execute.IsAlive))
            {
                if (obj == null && typeof(T).IsValueType)
                {
                    _execute.Execute(default(T));
                    return;
                }
                _execute.Execute((T)(obj));
            }
        }

        public void RaiseCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }

        /// <summary>
        /// Since this event is static, it will only hold onto the handler as a weak reference. 
        /// Objects that listen for this event should keep a strong reference to their event handler to avoid it being garbage collected. 
        /// This can be accomplished by having a private field and assigning the handler as the value before or after attaching to this event.
        /// https://msdn.microsoft.com/en-us/library/system.windows.input.commandmanager.requerysuggested.aspx
        /// </summary>
        public event EventHandler CanExecuteChanged
        {
            add
            {
                if (_canExecute != null)
                {
                    /* add event handler to local handler backing field in a thread safe manner
                     * 
                    EventHandler eventHandler = this._requerySuggestedLocal;
                    EventHandler eventHandler2;
                    do
                    {
                        eventHandler2 = eventHandler;
                        EventHandler value2 = (EventHandler)Delegate.Combine(eventHandler2, value);
                        eventHandler = Interlocked.CompareExchange<EventHandler>(ref this._requerySuggestedLocal, value2, eventHandler2);
                    }
                    while (eventHandler != eventHandler2);
                     * */

                    _requerySuggestedLocal = (EventHandler)Delegate.Combine(_requerySuggestedLocal, value);
                    CommandManager.RequerySuggested += value;
                }
            }
            remove
            {
                if (_canExecute != null)
                {
                    /* removes an event handler from local backing field in a thread safe manner
                     * 
                    EventHandler eventHandler = this._requerySuggestedLocal;
                    EventHandler eventHandler2;
                    do
                    {
                        eventHandler2 = eventHandler;
                        EventHandler value2 = (EventHandler)Delegate.Remove(eventHandler2, value);
                        eventHandler = Interlocked.CompareExchange<EventHandler>(ref this._requerySuggestedLocal, value2, eventHandler2);
                    }
                    while (eventHandler != eventHandler2);
                    * */

                    _requerySuggestedLocal = (EventHandler)Delegate.Remove(_requerySuggestedLocal, value);
                    CommandManager.RequerySuggested -= value;
                }
            }
        }

    }
}
