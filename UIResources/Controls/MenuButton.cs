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
using System.ComponentModel;

namespace UIResources.Controls
{
    [TemplatePart(Name = ExpandButtonTemplateName, Type = typeof(Button))]
    public class MenuButton : ButtonBase
    {
        private static readonly Type _typeofSelf = typeof(MenuButton);

        private const string ExpandButtonTemplateName = "PART_ExpandButton";
        private Button _expandButton = null;

        static MenuButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MenuButton), new FrameworkPropertyMetadata(typeof(MenuButton)));
            VisibilityProperty.OverrideMetadata(typeof(MenuButton), new UIPropertyMetadata(Visibility.Visible, OnVisibilityPropertyChanged));
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
            ((MenuButton)d).RefreshMenu();
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

            if (_expandButton != null)
                _expandButton.Click -= ExpandButton_Click;

            _expandButton = base.GetTemplateChild(ExpandButtonTemplateName) as Button;
            if (_expandButton != null)
                _expandButton.Click += ExpandButton_Click;
        }

        protected override void OnClick()
        {
            base.OnClick();

            if (!IsSplit)
            {
                IsExpanded = !IsExpanded;
                RefreshMenu();
            }
        }

        protected override void OnPreviewMouseRightButtonDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseRightButtonDown(e);

            if (this.ContextMenu != null)
            {
                IsExpanded = false;
                this.ContextMenu.Visibility = Visibility.Collapsed;
            }
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);

            if (this.ContextMenu != null && e.Key == Key.Apps)
            {
                IsExpanded = false;
                this.ContextMenu.Visibility = Visibility.Collapsed;
            }
        }

        #endregion

        #region Event

        private void ExpandButton_Click(object sender, RoutedEventArgs e)
        {
            IsExpanded = !IsExpanded;

            e.Handled = true;
        }

        private void ContextMenu_Closed(object sender, RoutedEventArgs e)
        {
            IsExpanded = false;
        } 

        private static void OnVisibilityPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MenuButton obj = d as MenuButton;
            if (obj.ContextMenu != null && obj.Visibility != Visibility.Visible)
                obj.IsExpanded = false;
        }

        #endregion

        #region Method

        private void RefreshMenu()
        {
            if (this.ContextMenu == null || this.Visibility != Visibility.Visible)
            {
                IsExpanded = false;
                return;
            }

            if (IsExpanded)
            {
                this.ContextMenu.Closed += ContextMenu_Closed;

                this.ContextMenu.Visibility = Visibility.Visible;
                this.ContextMenu.PlacementTarget = this;
                this.ContextMenu.Placement = PlacementMode.Bottom;
            }
            else
                this.ContextMenu.Closed -= ContextMenu_Closed;

            this.ContextMenu.IsOpen = IsExpanded;
        }

        #endregion
    }
}
