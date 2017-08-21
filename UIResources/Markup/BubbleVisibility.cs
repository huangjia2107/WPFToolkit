using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;

namespace UIResources.Markup
{
    [MarkupExtensionReturnType(typeof(Visibility))]
    [ContentProperty("Binding")]
    public class BubbleVisibilityExtension : MarkupExtension
    {
        public BubbleVisibilityExtension() { }

        public BubbleVisibilityExtension(BindingBase binding, Type ancestorType)
        {
            _binding = binding;
            _ancestorType = ancestorType;
        }

        private BindingBase _binding;
        [ConstructorArgument("binding")]
        public BindingBase Binding
        {
            get { return _binding; }
            set { _binding = value; }
        }

        private Type _ancestorType = null;
        [ConstructorArgument("ancestorType")]
        public Type AncestorType
        {
            get { return _ancestorType; }
            set
            {
                _ancestorType = value;

                if (value != null && _ancestorLevel == 0)
                    _ancestorLevel = 1;
            }
        }

        private uint _ancestorLevel = 0;
        public uint AncestorLevel
        {
            get { return _ancestorLevel; }
            set { _ancestorLevel = value; }
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            IProvideValueTarget service = serviceProvider.GetService(typeof(IProvideValueTarget)) as IProvideValueTarget;
            if (service == null)
                return null;

            UIElement mTarget = service.TargetObject as FrameworkElement;
            DependencyProperty mProperty = service.TargetProperty as DependencyProperty;
            if (mTarget != null && mProperty != null)
            {
                var v = DependencyPropertyDescriptor.FromProperty(mProperty, typeof(UIElement));
                v.AddValueChanged(mTarget, VisibilityChanged);

                if (_binding != null)
                    return _binding.ProvideValue(serviceProvider);// BindingOperations.SetBinding(mTarget, mProperty, _Binding);
            }

            return Visibility.Visible;
        }

        private void VisibilityChanged(object sender, EventArgs e)
        {
            if (_ancestorType == null || _binding == null || _ancestorLevel == 0)
                return;

            var element = sender as UIElement;
            UpdateVisibility(element);
        }

        private void UpdateVisibility(UIElement element)
        {
            if (element == null)
                throw new ArgumentNullException("element");

            var parentNode = LogicalTreeHelper.GetParent(element);
            uint foundAncestorLevel = 0;

            while (parentNode != null)
            {
                if (parentNode is ContentControl)
                    UpdateParentVisibility(parentNode as ContentControl);
                else if (parentNode is Panel)
                    UpdateParentVisibility(parentNode as Panel);
                else if (parentNode is Decorator)
                    UpdateParentVisibility(parentNode as Decorator);
                else
                {
                    parentNode = LogicalTreeHelper.GetParent(parentNode);
                    continue;
                }

                if (_ancestorType.IsInstanceOfType(parentNode))
                {
                    foundAncestorLevel++;
                    if (foundAncestorLevel == _ancestorLevel)
                        break;
                }

                parentNode = LogicalTreeHelper.GetParent(parentNode);
            }
        }

        private void UpdateParentVisibility<T>(T obj) where T : FrameworkElement, IAddChild
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
                decorator.Visibility = decorator.Child.Visibility;
            }
            else if (obj is ContentControl)
            {
                var contentControl = obj as ContentControl;
                contentControl.Visibility = (contentControl.HasContent && contentControl.Content != null) ? (contentControl.Content as UIElement).Visibility : Visibility.Visible;
            }
        }
    }
}
