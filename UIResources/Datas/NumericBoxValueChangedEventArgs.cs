using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace UIResources.Datas
{
    public class NumericBoxValueChangedEventArgs<T> : RoutedPropertyChangedEventArgs<T>
    {
        public bool IsManual { get; private set; }
        public bool IsBusy {get; private set; }

        public NumericBoxValueChangedEventArgs(T oldValue, T newValue, bool isManual, bool isBusy)
            : base(oldValue, newValue)
        {
            IsManual = isManual;
            IsBusy = isBusy;
        }

        public NumericBoxValueChangedEventArgs(T oldValue, T newValue, bool isManual, bool isBusy, RoutedEvent routedEvent)
            : base(oldValue, newValue, routedEvent)
        {
            IsManual = isManual;
            IsBusy = isBusy;
        }
    }
}
