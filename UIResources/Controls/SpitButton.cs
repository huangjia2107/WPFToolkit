using System; 
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives; 
using System.Windows.Input; 

namespace UIResources.Controls
{
    [TemplatePart(Name = DockPanelTemplateName, Type = typeof(DockPanel))]
    [TemplatePart(Name = DropDownBorderTemplateName, Type = typeof(Border))]
    [TemplatePart(Name = ContentButtonTemplateName, Type = typeof(Button))]
    [TemplatePart(Name = PopupTemplateName, Type = typeof(Popup))]
    public class SpitButton : MenuBase
    {
        private static readonly Type _typeofSelf = typeof(SpitButton);

        private const string DockPanelTemplateName = "PART_DockPanel";
        private const string DropDownBorderTemplateName = "PART_DropDownBorder";
        private const string ContentButtonTemplateName = "PART_ContentButton";
        private const string PopupTemplateName = "PART_Popup";

        private DockPanel _dockPanel = null;
        private Border _dropDownBorder = null;
        private Button _contentButton = null;
        private Popup _popup = null;

        static SpitButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SpitButton), new FrameworkPropertyMetadata(typeof(SpitButton)));

            EventManager.RegisterClassHandler(_typeofSelf, LostMouseCaptureEvent, new MouseEventHandler(OnLostMouseCaptureThunk));
            EventManager.RegisterClassHandler(_typeofSelf, MenuItem.ClickEvent, new RoutedEventHandler(OnMenuItemExClick));
            EventManager.RegisterClassHandler(_typeofSelf, Mouse.PreviewMouseDownOutsideCapturedElementEvent, new MouseButtonEventHandler(OnOutsideCapturedElementHandler));
        }

        public SpitButton()
        {
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }

        /// <summary>
        ///     DropDown Open event
        /// </summary>
        public event EventHandler DropDownOpened;

        /// <summary>
        ///     DropDown Close event
        /// </summary>
        public event EventHandler DropDownClosed;

        //RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent, this));
        public static readonly RoutedEvent ClickEvent = EventManager.RegisterRoutedEvent("Click", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(SpitButton));
        public event RoutedEventHandler Click
        {
            add { AddHandler(ClickEvent, value); }
            remove { RemoveHandler(ClickEvent, value); }
        }

        #region Properties

        public static readonly DependencyProperty ContentProperty = ContentControl.ContentProperty.AddOwner(_typeofSelf);
        public object Content
        {
            get { return (object)GetValue(ContentProperty); }
            set { SetValue(ContentProperty, value); }
        }

        public static readonly DependencyProperty IsSplitProperty = DependencyProperty.Register("IsSplit", typeof(bool), _typeofSelf, new PropertyMetadata(true));
        public bool IsSplit
        {
            get { return (bool)GetValue(IsSplitProperty); }
            set { SetValue(IsSplitProperty, value); }
        }

        public static readonly DependencyProperty IsDropDownOpenProperty = DependencyProperty.Register("IsDropDownOpen", typeof(bool), _typeofSelf, new PropertyMetadata(false, OnIsDropDownOpenChanged));
        public bool IsDropDownOpen
        {
            get { return (bool)GetValue(IsDropDownOpenProperty); }
            set { SetValue(IsDropDownOpenProperty, value); }
        }

        private static void OnIsDropDownOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var spitButton = (SpitButton)d;
            if ((bool)e.NewValue)
            {
                spitButton.CaptureAndFocus();
                spitButton.OnDropDownOpened(EventArgs.Empty);
            }
            else
            {
                // If focus is within the subtree, make sure we have the focus so that focus isn't in the disposed hwnd 
                if (spitButton.IsKeyboardFocusWithin)
                {
                    // make sure the control has focus 
                    spitButton.Focus();
                }

                spitButton.ReleaseMouseCapture();
                spitButton.OnDropDownClosed(EventArgs.Empty);
            }
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
            UnSubscribeEvents();

            base.OnApplyTemplate();

            _dockPanel = base.GetTemplateChild(DockPanelTemplateName) as DockPanel;
            _dropDownBorder = base.GetTemplateChild(DropDownBorderTemplateName) as Border;
            _contentButton = base.GetTemplateChild(ContentButtonTemplateName) as Button;
            _popup = base.GetTemplateChild(PopupTemplateName) as Popup;

            SubscribeEvents();
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Escape:
                    if (this.IsDropDownOpen)
                        this.IsDropDownOpen = false;
                    break;
            }
        }

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new MenuItem();
        }

        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is FrameworkElement;
        }

        #endregion

        #region Event

        //MouseDown on MenuEx,then MouseUp outside of MenuEx
        private static void OnLostMouseCaptureThunk(object sender, MouseEventArgs e)
        {
            var spitButton = sender as SpitButton;
            if (e.OriginalSource is SpitButton && spitButton.IsDropDownOpen && Mouse.Captured == null)
                spitButton.CaptureAndFocus();
        }

        private static void OnOutsideCapturedElementHandler(object sender, MouseButtonEventArgs e)
        {
            (sender as SpitButton).IsDropDownOpen = false;
        }

        private static void OnMenuItemExClick(object sender, RoutedEventArgs e)
        {
            e.Handled = true;

            var spitButton = sender as SpitButton;
            if (spitButton.IsDropDownOpen)
                spitButton.IsDropDownOpen = false;
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            UnSubscribeEvents();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            SubscribeEvents();
        }

        private void OnDropDownBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.Focus();
            this.IsDropDownOpen = !this.IsDropDownOpen;
        }

        private void OnContentButton_Click(object sender, RoutedEventArgs e)
        {
            if (!IsSplit)
                return;

            IsDropDownOpen = false;
            this.RaiseEvent(new RoutedEventArgs(ClickEvent));
        }

        private void DockPanel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (IsSplit)
                return;

            OnDropDownBorder_MouseLeftButtonDown(sender, e);
        }

        #endregion

        #region Method

        private void OnDropDownOpened(EventArgs e)
        {
            if (DropDownOpened != null)
                DropDownOpened(this, e);
        }

        private void OnDropDownClosed(EventArgs e)
        {
            if (DropDownClosed != null)
                DropDownClosed(this, e);
        }

        private void SubscribeEvents()
        {
            // Always unsubscribe events to ensure we don't subscribe twice
            UnSubscribeEvents();

            if (_dropDownBorder != null)
                _dropDownBorder.MouseLeftButtonDown += OnDropDownBorder_MouseLeftButtonDown;

            if (_contentButton != null)
                _contentButton.Click += OnContentButton_Click;

            if (_dockPanel != null)
                _dockPanel.MouseLeftButtonDown += DockPanel_MouseLeftButtonDown;
        }

        private void UnSubscribeEvents()
        {
            if (_dropDownBorder != null)
                _dropDownBorder.MouseLeftButtonDown -= OnDropDownBorder_MouseLeftButtonDown;

            if (_contentButton != null)
                _contentButton.Click -= OnContentButton_Click;

            if (_dockPanel != null)
                _dockPanel.MouseLeftButtonDown -= DockPanel_MouseLeftButtonDown;
        }

        private static ItemsPanelTemplate GetDefaultPanel()
        {
            ItemsPanelTemplate template = new ItemsPanelTemplate(new FrameworkElementFactory(typeof(StackPanel)));
            template.Seal();
            return template;
        }

        private void CaptureAndFocus()
        {
            Keyboard.Focus(_popup);
            Mouse.Capture(this, CaptureMode.SubTree);

            Dispatcher.BeginInvoke((Action)(() =>
            {
                var container = this.ItemContainerGenerator.ContainerFromIndex(0);

                NavigateToContainer(container);

                // Edge case: Whole dropdown content is disabled
                if (this.IsKeyboardFocusWithin == false)
                    Keyboard.Focus(this._popup);
            }));
        }

        private static void NavigateToContainer(DependencyObject container)
        {
            var element = container as FrameworkElement;

            if (element == null)
                return;

            if (element.Focusable)
                Keyboard.Focus(element);
            else
            {
                var predicted = element.PredictFocus(FocusNavigationDirection.Down);

                if (predicted is MenuBase == false)
                {
                    element.MoveFocus(new TraversalRequest(FocusNavigationDirection.Down));
                }
            }
        }

        #endregion
    }
}
