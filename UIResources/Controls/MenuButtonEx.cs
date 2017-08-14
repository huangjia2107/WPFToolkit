using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace UIResources.Controls
{
    [TemplatePart(Name = PART_ExpandButton, Type = typeof(Button))]
    [TemplatePart(Name = PART_Popup, Type = typeof(Popup))]
    public class MenuButtonEx : Menu
    {
        private static readonly Type _typeofSelf = typeof(MenuButtonEx);

        private const string PART_ExpandButton = "PART_ExpandButton";
        private const string PART_Popup = "PART_Popup";


        private Button _expandButton = null;
        private Popup _popup = null;

        static MenuButtonEx()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MenuButtonEx), new FrameworkPropertyMetadata(typeof(MenuButtonEx)));
        }

        #region Properties

        public static readonly DependencyProperty IsSplitProperty = DependencyProperty.Register("IsSplit", typeof(bool), _typeofSelf, new PropertyMetadata(true));
        public bool IsSplit
        {
            get { return (bool)GetValue(IsSplitProperty); }
            set { SetValue(IsSplitProperty, value); }
        }

        public static readonly DependencyProperty IsExpandedProperty = DependencyProperty.Register("IsExpanded", typeof(bool), _typeofSelf, new PropertyMetadata(false, IsExpandedPropertyChangedCallback));
        public bool IsExpanded
        {
            get { return (bool)GetValue(IsExpandedProperty); }
            set { SetValue(IsExpandedProperty, value); }
        }

        private static void IsExpandedPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            //((MenuButton)d).RefreshMenu();
        }

        public static readonly DependencyProperty OrientationProperty = DependencyProperty.Register("Orientation", typeof(Orientation), _typeofSelf, new PropertyMetadata(Orientation.Horizontal));
        public Orientation Orientation
        {
            get { return (Orientation)GetValue(OrientationProperty); }
            set { SetValue(OrientationProperty, value); }
        }

        #endregion

        #region Override

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _expandButton = base.GetTemplateChild(PART_ExpandButton) as Button;
            _popup = base.GetTemplateChild(PART_Popup) as Popup;
             
        }

        #endregion

    }
}
