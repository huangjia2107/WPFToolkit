using System.Windows;

namespace UIResources.AttachedProperties
{
    public partial class AttachedProperty
    {
        //HandleMouseWheel
        public static RoutedEventHandler MouseWheelEventHandler = (s, e) =>
        {
            e.Handled = false;
        };

        public static readonly DependencyProperty IsHandleMouseWheelProperty = DependencyProperty.RegisterAttached("IsHandleMouseWheel", typeof(bool), typeof(AttachedProperty), 
            new PropertyMetadata(true, IsHandleMouseWheelPropertyChangedCallback));
        public static bool GetIsHandleMouseWheel(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsHandleMouseWheelProperty);
        }
        public static void SetIsHandleMouseWheel(DependencyObject obj, bool value)
        {
            obj.SetValue(IsHandleMouseWheelProperty, value);
        }
        static void IsHandleMouseWheelPropertyChangedCallback(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            var obj = target as FrameworkElement;
            if (obj == null)
                return;

            var isHandle = (bool)e.NewValue;

            if (isHandle)
                obj.RemoveHandler(UIElement.MouseWheelEvent, MouseWheelEventHandler);
            else
                obj.AddHandler(UIElement.MouseWheelEvent, MouseWheelEventHandler, true);
        }
    }
}
