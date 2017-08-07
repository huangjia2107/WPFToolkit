using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using MvvmUtils.Models;

namespace MvvmUtils.Command
{
    public class RelayCommand : ICommand
    {
        private readonly WeakAction _execute;
        private readonly WeakFunc<bool> _canExecute;

        private EventHandler _requerySuggestedLocal;

        public RelayCommand(Action execute)
            : this(execute, null)
        {

        }

        public RelayCommand(Action execute, Func<bool> canExecute)
        {
            if (execute == null)
                throw new ArgumentNullException("execute");

            _execute = new WeakAction(execute);
            if (canExecute != null)
                _canExecute = new WeakFunc<bool>(canExecute);
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute == null || ((_canExecute.IsStatic || _canExecute.IsAlive) && _canExecute.Execute());
        }

        public virtual void Execute(object parameter)
        {
            if (CanExecute(parameter) && _execute != null && (_execute.IsStatic || _execute.IsAlive))
                _execute.Execute();
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
