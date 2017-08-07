using System;
using System.Collections.Generic; 
using System.Linq.Expressions;
using System.Runtime.CompilerServices; 
using MvvmUtils.Interfaces;
using MvvmUtils.Messaging;
using MvvmUtils.Messaging.Messages;

namespace MvvmUtils.ViewModel
{
    public abstract class ViewModelBase : ObservableObject, ICleanup
    {
        private IMessenger _messengerInstance;
        protected IMessenger MessengerInstance
        {
            get { return _messengerInstance ?? Messenger.Default; }
            set { _messengerInstance = value; }
        }

        public ViewModelBase()
            : this(null)
        { }

        public ViewModelBase(IMessenger messenger)
        {
            _messengerInstance = messenger;
        }

        public virtual void Cleanup()
        {
            MessengerInstance.Unregister(this);
        }

        public virtual void RaisePropertyChanged<T>([CallerMemberName] string propertyName = null, T oldValue = default(T), T newValue = default(T), bool broadcast = false)
        {
            if (string.IsNullOrEmpty(propertyName))
                throw new ArgumentException("This method cannot be called with an empty string", "propertyName");

            this.RaisePropertyChanged(propertyName);
            if (broadcast)
            {
                this.Broadcast<T>(oldValue, newValue, propertyName);
            }
        }

        public virtual void RaisePropertyChanged<T>(Expression<Func<T>> propertyExpression, T oldValue, T newValue, bool broadcast)
        {
            this.RaisePropertyChanged<T>(propertyExpression);
            if (broadcast)
            {
                string propertyName = ObservableObject.GetPropertyName<T>(propertyExpression);
                this.Broadcast<T>(oldValue, newValue, propertyName);
            }
        }

        protected virtual void Broadcast<T>(T oldValue, T newValue, string propertyName)
        {
            MessengerInstance.Send(new PropertyChangedMessage<T>(this, oldValue, newValue, propertyName));
        }

        protected bool Set<T>(Expression<Func<T>> propertyExpression, ref T field, T newValue, bool broadcast)
        {
            if (EqualityComparer<T>.Default.Equals(field, newValue))
                return false;

            T oldValue = field;
            field = newValue;
            this.RaisePropertyChanged<T>(propertyExpression, oldValue, field, broadcast);
            return true;
        } 

        protected bool Set<T>(ref T field, T newValue = default(T), bool broadcast = false, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, newValue))
                return false;

            T oldValue = field;
            field = newValue;
            this.RaisePropertyChanged<T>(propertyName, oldValue, field, broadcast);
            return true;
        }
    }
}
