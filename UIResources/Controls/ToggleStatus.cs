using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Controls.Primitives;
using UIResources.Helps;
using System.ComponentModel;

namespace UIResources.Controls
{
    public class ToggleStatus : ToggleButton
    {
        private static readonly Type _typeofSelf = typeof(ToggleStatus);

        static ToggleStatus()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ToggleStatus), new FrameworkPropertyMetadata(typeof(ToggleStatus)));
        }

        #region Properties

        [Browsable(false)]
        public new object Content
        {
            get { return base.Content; }
            set { base.Content = value; }
        }

        [Browsable(false)]
        public new object ToolTip
        {
            get { return base.ToolTip; }
            set { base.ToolTip = value; }
        }

        public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.Register("CornerRadius", typeof(CornerRadius), _typeofSelf,
            new PropertyMetadata(new CornerRadius()), IsCornerRadiusValid);
        public CornerRadius CornerRadius
        {
            get { return (CornerRadius)GetValue(CornerRadiusProperty); }
            set { SetValue(CornerRadiusProperty, value); }
        }
        static bool IsCornerRadiusValid(object value)
        {
            var t = (CornerRadius)value;
            return t.IsValid(false, false, false, false);
        }

        private static void OnContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var toggleStatus = (ToggleStatus)d;
            toggleStatus.OnContentChanged(e.OldValue, e.NewValue);
        }

        public static readonly DependencyProperty CheckedContentProperty = DependencyProperty.Register("CheckedContent", typeof(object), _typeofSelf,
            new FrameworkPropertyMetadata((object)null, OnContentChanged));
        public object CheckedContent
        {
            get { return (object)GetValue(CheckedContentProperty); }
            set { SetValue(CheckedContentProperty, value); }
        }

        public static readonly DependencyProperty UnCheckedContentProperty = DependencyProperty.Register("UnCheckedContent", typeof(object), _typeofSelf,
            new FrameworkPropertyMetadata((object)null, OnContentChanged));
        public object UnCheckedContent
        {
            get { return (object)GetValue(UnCheckedContentProperty); }
            set { SetValue(UnCheckedContentProperty, value); }
        }

        public static readonly DependencyProperty CheckedToolTipProperty = DependencyProperty.Register("CheckedToolTip", typeof(object), _typeofSelf,
            new FrameworkPropertyMetadata((object)null));
        public object CheckedToolTip
        {
            get { return (object)GetValue(CheckedToolTipProperty); }
            set { SetValue(CheckedToolTipProperty, value); }
        }

        public static readonly DependencyProperty UnCheckedToolTipProperty = DependencyProperty.Register("UnCheckedToolTip", typeof(object), _typeofSelf,
            new FrameworkPropertyMetadata((object)null));
        public object UnCheckedToolTip
        {
            get { return (object)GetValue(UnCheckedToolTipProperty); }
            set { SetValue(UnCheckedToolTipProperty, value); }
        }

        #endregion
    }
}
