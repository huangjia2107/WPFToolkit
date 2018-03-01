using System.Windows;

namespace UIResources.AttachedProperties
{
    public partial class AttachedProperty
    {
        public static readonly DependencyProperty IsHandleMouseWheelProperty = DependencyProperty.RegisterAttached("IsHandleMouseWheel", typeof(bool?), typeof(AttachedProperty), 
            new UIPropertyMetadata(null, IsHandleMouseWheelPropertyChangedCallback));
        public static bool? GetIsHandleMouseWheel(DependencyObject obj)
        {
            return (bool?)obj.GetValue(IsHandleMouseWheelProperty);
        }
        public static void SetIsHandleMouseWheel(DependencyObject obj, bool? value)
        {
            obj.SetValue(IsHandleMouseWheelProperty, value);
        }
        
        static void IsHandleMouseWheelPropertyChangedCallback(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            var obj = target as FrameworkElement;
            if (obj == null)
                return;

            var isHandled = (bool?)e.NewValue;
            if (isHandled.HasValue)
                obj.AddHandler(UIElement.MouseWheelEvent, new RoutedEventHandler((s, args) => args.Handled = isHandled.Value), true);
        } 
    }
}
