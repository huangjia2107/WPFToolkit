using System; 
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;
using UIResources.Datas; 

namespace UIResources.AttachedProperties
{
    public partial class AttachedProperty
    {
        public static readonly DependencyProperty RelativeVisibilityProperty = DependencyProperty.RegisterAttached("RelativeVisibility", typeof(Visibility), typeof(AttachedProperty),
            new PropertyMetadata(Visibility.Visible, RelativeVisibilityPropertyChangedCallback));
        public static Visibility GetRelativeVisibility(DependencyObject obj)
        {
            return (Visibility)obj.GetValue(RelativeVisibilityProperty);
        }
        public static void SetRelativeVisibility(DependencyObject obj, Visibility value)
        {
            obj.SetValue(RelativeVisibilityProperty, value);
        }
        static void RelativeVisibilityPropertyChangedCallback(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            var obj = target as UIElement;

            obj.Visibility = (Visibility)e.NewValue;
            var binding = (IRelativeBinding)BindingOperations.GetBindingBase(obj, RelativeVisibilityProperty);

            if (binding != null)
                UpdateSpecifiedAncestorTypeVisibility(obj, binding.AncestorType, binding.AncestorLevel);
        }

        private static void UpdateSpecifiedAncestorTypeVisibility(DependencyObject sourceObject, Type ancestorType, uint ancestorLevel)
        {
            if (sourceObject == null || ancestorType == null || ancestorLevel == 0)
                return;

            uint curAncestorLevel = 0;
            var parentNode = LogicalTreeHelper.GetParent(sourceObject);

            while (parentNode != null)
            {
                if (parentNode is ContentControl || parentNode is Panel || parentNode is Decorator)
                    GetAndSetParentVisibility((IAddChild)parentNode);
                else
                {
                    parentNode = LogicalTreeHelper.GetParent(parentNode);
                    continue;
                }

                if (ancestorType.IsAssignableFrom(parentNode.GetType()))
                {
                    curAncestorLevel++;
                    if (curAncestorLevel == ancestorLevel)
                        break;
                }

                parentNode = LogicalTreeHelper.GetParent(parentNode);
            }
        }

        private static Visibility GetAndSetParentVisibility<T>(T obj) where T : IAddChild
        {
            var result = Visibility.Collapsed;
            if (obj is Panel)
            {
                var panel = obj as Panel;
                foreach (var child in panel.Children)
                {
                    result = (child as UIElement).Visibility;
                    if (result == Visibility.Visible)
                        break;
                }
                panel.Visibility = result;
            }
            else if (obj is Decorator)
            {
                var decorator = obj as Decorator;
                result = decorator.Visibility = decorator.Child.Visibility;
            }
            else if (obj is ContentControl)
            {
                var contentControl = obj as ContentControl;
                result = contentControl.Visibility = contentControl.HasContent ? (contentControl.Content as UIElement).Visibility : Visibility.Visible;
            }

            return result;
        }
    }
}
