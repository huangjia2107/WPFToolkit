using System;
using System.Windows;
using System.Windows.Controls;

namespace UIResources.Controls
{
    public class BusyIndicator : Control
    {
        private static readonly Type _typeofSelf = typeof(BusyIndicator);

        static BusyIndicator()
        {
            DefaultStyleKeyProperty.OverrideMetadata(_typeofSelf, new FrameworkPropertyMetadata(_typeofSelf));
        }

        public static readonly DependencyProperty IsActiveProperty =
            DependencyProperty.Register("IsActive", typeof(bool), _typeofSelf, new PropertyMetadata(false));
        public bool IsActive
        {
            get { return (bool)GetValue(IsActiveProperty); }
            set { SetValue(IsActiveProperty, value); }
        }
    }
}
