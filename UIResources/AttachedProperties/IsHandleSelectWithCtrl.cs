using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace UIResources.AttachedProperties
{
    public partial class AttachedProperty
    {
        public static RoutedEventHandler PreviewMouseLeftButtonDownEventHandler = (s, e) =>
        {
            var container = ItemsControl.ContainerFromElement(s as ItemsControl, (DependencyObject) e.OriginalSource);
            if (container != null)
            {
                if (container is TreeViewItem && (container as TreeViewItem).IsSelected && Keyboard.Modifiers == ModifierKeys.Control)
                    e.Handled = true;

                if (container is ListBoxItem && (container as ListBoxItem).IsSelected && Keyboard.Modifiers == ModifierKeys.Control)
                    e.Handled = true;
            }
        };

        public static readonly DependencyProperty IsHandleSelectWithCtrlProperty = DependencyProperty.RegisterAttached("IsHandleSelectWithCtrl", typeof(bool), typeof(AttachedProperty),
            new PropertyMetadata(false, IsHandleSelectWithCtrlPropertyChangedCallback));
        public static bool GetIsHandleSelectWithCtrl(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsHandleSelectWithCtrlProperty);
        }
        public static void SetIsHandleSelectWithCtrl(DependencyObject obj, bool value)
        {
            obj.SetValue(IsHandleSelectWithCtrlProperty, value);
        }
        static void IsHandleSelectWithCtrlPropertyChangedCallback(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            ItemsControl obj = target as ItemsControl;
            if (obj == null)
                return;

            var isHandle = (bool)e.NewValue;

            if (isHandle)
                obj.AddHandler(UIElement.PreviewMouseLeftButtonDownEvent, PreviewMouseLeftButtonDownEventHandler, true);
            else
                obj.RemoveHandler(UIElement.PreviewMouseLeftButtonDownEvent, PreviewMouseLeftButtonDownEventHandler);
        }

    }
}
