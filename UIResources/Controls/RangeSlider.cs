using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using UIResources.Helps;

namespace UIResources.Controls
{
    [TemplatePart(Name = LeftThumbTemplateName, Type = typeof(Thumb))]
    [TemplatePart(Name = CenterThumbTemplateName, Type = typeof(Thumb))]
    [TemplatePart(Name = RightThumbTemplateName, Type = typeof(Thumb))]
    public class RangeSlider : Control
    {
        private const string LeftThumbTemplateName = "PART_LeftThumb";
        private const string CenterThumbTemplateName = "PART_CenterThumb";
        private const string RightThumbTemplateName = "PART_RightThumb";

        private Thumb _leftThumb = null;
        private Thumb _centerThumb = null;
        private Thumb _rightThumb = null;

        private ToolTip _leftAutoToolTip = null;
        private ToolTip _rightAutoToolTip = null;

        private object _leftThumbOriginalToolTip = null;
        private object _rightThumbOriginalToolTip = null;

        static RangeSlider()
        {
            // Register Event Handler for the Thumb
            EventManager.RegisterClassHandler(typeof(RangeSlider), Thumb.DragStartedEvent, new DragStartedEventHandler(RangeSlider.OnThumbDragStarted));
            EventManager.RegisterClassHandler(typeof(RangeSlider), Thumb.DragDeltaEvent, new DragDeltaEventHandler(RangeSlider.OnThumbDragDelta));
            EventManager.RegisterClassHandler(typeof(RangeSlider), Thumb.DragCompletedEvent, new DragCompletedEventHandler(RangeSlider.OnThumbDragCompleted));

            EventManager.RegisterClassHandler(typeof(RangeSlider), SizeChangedEvent, new SizeChangedEventHandler(RangeSlider.OnSizeChanged));

            DefaultStyleKeyProperty.OverrideMetadata(typeof(RangeSlider), new FrameworkPropertyMetadata(typeof(RangeSlider)));
        }

        public RangeSlider()
        {
            this.Loaded += RangeSlider_Loaded;
            this.Unloaded += RangeSlider_Unloaded;
        }

        public static readonly RoutedEvent LeftValueChangedEvent = EventManager.RegisterRoutedEvent("LeftValueChanged", RoutingStrategy.Bubble, typeof(RoutedPropertyChangedEventHandler<double>), typeof(RangeSlider));
        public event RoutedPropertyChangedEventHandler<double> LeftValueChanged
        {
            add { AddHandler(LeftValueChangedEvent, value); }
            remove { RemoveHandler(LeftValueChangedEvent, value); }
        }

        public static readonly RoutedEvent RightValueChangedEvent = EventManager.RegisterRoutedEvent("RightValueChanged", RoutingStrategy.Bubble, typeof(RoutedPropertyChangedEventHandler<double>), typeof(RangeSlider));
        public event RoutedPropertyChangedEventHandler<double> RightValueChanged
        {
            add { AddHandler(RightValueChangedEvent, value); }
            remove { RemoveHandler(RightValueChangedEvent, value); }
        }

        #region DependencyProperties

        //MinValue
        public static readonly DependencyProperty MinimumProperty =
               DependencyProperty.Register("Minimum", typeof(double), typeof(RangeSlider),
                       new FrameworkPropertyMetadata(0.0d, OnMinimumChanged), IsValidDoubleValue);

        public double Minimum
        {
            get { return (double)GetValue(MinimumProperty); }
            set { SetValue(MinimumProperty, value); }
        }
        private static void OnMinimumChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = (RangeSlider)d;

            ctrl.CoerceValue(MaximumProperty, ctrl.Maximum);
            ctrl.CoerceValue(RightValueProperty, ctrl.RightValue);
            ctrl.CoerceValue(LeftValueProperty, ctrl.LeftValue);

            ctrl.UpdateThumbPosition();
        }

        //MaxValue
        public static readonly DependencyProperty MaximumProperty =
                DependencyProperty.Register("Maximum", typeof(double), typeof(RangeSlider),
                        new FrameworkPropertyMetadata(100.0d, OnMaximumChanged, CoerceMaximum), IsValidDoubleValue);
        public double Maximum
        {
            get { return (double)GetValue(MaximumProperty); }
            set { SetValue(MaximumProperty, value); }
        }

        private static object CoerceMaximum(DependencyObject d, object value)
        {
            var ctrl = (RangeSlider)d;

            var min = ctrl.Minimum;
            if ((double)value < min)
                return min;

            return value;
        }

        private static void OnMaximumChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = (RangeSlider)d;

            ctrl.CoerceValue(RightValueProperty, ctrl.RightValue);
            ctrl.UpdateThumbPosition();
        }

        //Left Value
        public static readonly DependencyProperty LeftValueProperty =
            DependencyProperty.Register("LeftValue", typeof(double), typeof(RangeSlider),
               new FrameworkPropertyMetadata(40d, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault | FrameworkPropertyMetadataOptions.Journal,
                   OnLeftValueChanged, ConstrainLeftValueToRange), IsValidDoubleValue);
        public double LeftValue
        {
            get { return (double)GetValue(LeftValueProperty); }
            set { SetValue(LeftValueProperty, value); }
        }

        private static object ConstrainLeftValueToRange(DependencyObject d, object value)
        {
            var ctrl = (RangeSlider)d;
            var v = (double)value;

            var min = ctrl.Minimum;
            if (v < min)
                return min;

            var rightValue = ctrl.RightValue;
            if (v > rightValue)
                return rightValue;

            return value;
        }

        private static void OnLeftValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = (RangeSlider)d;

            ctrl.UpdateThumbPosition();
            ctrl.OnLeftValueChanged((double)e.OldValue, (double)e.NewValue);
        }

        protected virtual void OnLeftValueChanged(double oldValue, double newValue)
        {
            var args = new RoutedPropertyChangedEventArgs<double>(oldValue, newValue);
            args.RoutedEvent = RangeSlider.LeftValueChangedEvent;
            RaiseEvent(args);
        }

        //Right Value
        public static readonly DependencyProperty RightValueProperty =
            DependencyProperty.Register("RightValue", typeof(double), typeof(RangeSlider),
               new FrameworkPropertyMetadata(60d, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault | FrameworkPropertyMetadataOptions.Journal,
                   OnRightValueChanged, ConstrainRightValueToRange), IsValidDoubleValue);

        private static object ConstrainRightValueToRange(DependencyObject d, object value)
        {
            var ctrl = (RangeSlider)d;
            var v = (double)value;

            var leftValue = ctrl.LeftValue;
            if (v < leftValue)
                return leftValue;

            var max = ctrl.Maximum;
            if (v > max)
                return max;

            return value;
        }

        public double RightValue
        {
            get { return (double)GetValue(RightValueProperty); }
            set { SetValue(RightValueProperty, value); }
        }

        private static void OnRightValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = (RangeSlider)d;

            ctrl.CoerceValue(LeftValueProperty, ctrl.LeftValue);
            ctrl.UpdateThumbPosition();
            ctrl.OnRightValueChanged((double)e.OldValue, (double)e.NewValue);
        }
        protected virtual void OnRightValueChanged(double oldValue, double newValue)
        {
            var args = new RoutedPropertyChangedEventArgs<double>(oldValue, newValue);
            args.RoutedEvent = RangeSlider.RightValueChangedEvent;
            RaiseEvent(args);
        }

        public static readonly DependencyProperty SelectionForegroundProperty =
            DependencyProperty.Register("SelectionForeground", typeof(Brush), typeof(RangeSlider), new PropertyMetadata(Brushes.Green));
        public Brush SelectionForeground
        {
            get { return (Brush)GetValue(SelectionForegroundProperty); }
            set { SetValue(SelectionForegroundProperty, value); }
        }

        public static readonly DependencyProperty ThumbWidthProperty =
            DependencyProperty.Register("ThumbWidth", typeof(double), typeof(RangeSlider), new PropertyMetadata(20d, OnThumbWidthChanged, CoerceThumbWidth), IsValidDoubleValue);
        public double ThumbWidth
        {
            get { return (double)GetValue(ThumbWidthProperty); }
            set { SetValue(ThumbWidthProperty, value); }
        }
        private static object CoerceThumbWidth(DependencyObject d, object value)
        {
            return Math.Max(0, (double)value);
        }
        private static void OnThumbWidthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = (RangeSlider)d;
            ctrl.UpdateThumbPosition();
        }

        public static readonly DependencyProperty AutoToolTipPlacementProperty =
            DependencyProperty.Register("AutoToolTipPlacement", typeof(AutoToolTipPlacement), typeof(RangeSlider),
                                          new FrameworkPropertyMetadata(AutoToolTipPlacement.TopLeft),
                                          new ValidateValueCallback(IsValidAutoToolTipPlacement));

        public AutoToolTipPlacement AutoToolTipPlacement
        {
            get { return (AutoToolTipPlacement)GetValue(AutoToolTipPlacementProperty); }
            set { SetValue(AutoToolTipPlacementProperty, value); }
        }

        private static bool IsValidAutoToolTipPlacement(object o)
        {
            var placement = (AutoToolTipPlacement)o;
            return placement == AutoToolTipPlacement.None ||
                   placement == AutoToolTipPlacement.TopLeft ||
                   placement == AutoToolTipPlacement.BottomRight;
        }

        public static readonly DependencyProperty AutoToolTipPrecisionProperty =
            DependencyProperty.Register("AutoToolTipPrecision", typeof(int), typeof(RangeSlider),
                new FrameworkPropertyMetadata(2), IsValidAutoToolTipPrecision);

        public int AutoToolTipPrecision
        {
            get { return (int)GetValue(AutoToolTipPrecisionProperty); }
            set { SetValue(AutoToolTipPrecisionProperty, value); }
        }

        private static bool IsValidAutoToolTipPrecision(object o)
        {
            return (((int)o) >= 0);
        }

        public static readonly DependencyProperty IsLinkedProperty = DependencyProperty.Register("IsLinked", typeof(bool), typeof(RangeSlider), new PropertyMetadata(true));
        public bool IsLinked
        {
            get { return (bool)GetValue(IsLinkedProperty); }
            set { SetValue(IsLinkedProperty, value); }
        }

        #endregion

        #region Func

        private void CoerceValue(DependencyProperty dp, object localValue)
        {
            //Update local value firstly
            SetCurrentValue(dp, localValue);
            CoerceValue(dp);
        }

        private static bool IsValidDoubleValue(object value)
        {
            double d = (double)value;

            return !(double.IsNaN(d) || double.IsInfinity(d));
        }

        private void UpdateThumbPosition()
        {
            if (!IsLoaded || _leftThumb == null || _centerThumb == null || _rightThumb == null)
                return;

            if (_leftThumb.IsDragging || _centerThumb.IsDragging || _rightThumb.IsDragging)
                return;

            var leftOffset = Math.Max(0, (ActualWidth - 2 * ThumbWidth) * (LeftValue - Minimum) / (Maximum - Minimum));
            var rightOffset = Math.Max(0, ActualWidth - (ActualWidth - 2 * ThumbWidth) * (RightValue - Minimum) / (Maximum - Minimum) - ThumbWidth);
            var selectionWidth = Math.Max(0, (ActualWidth - 2 * ThumbWidth) * (RightValue - LeftValue) / (Maximum - Minimum));

            Canvas.SetLeft(_leftThumb, leftOffset);
            Canvas.SetRight(_rightThumb, rightOffset);

            Canvas.SetLeft(_centerThumb, leftOffset + ThumbWidth);
            _centerThumb.Width = selectionWidth;
        }

        private void UpdateValue()
        {
            if (_leftThumb == null || _rightThumb == null)
                return;

            LeftValue = Canvas.GetLeft(_leftThumb) / (this.ActualWidth - 2 * ThumbWidth) * (Maximum - Minimum);
            RightValue = (this.ActualWidth - Canvas.GetRight(_rightThumb) - 2 * ThumbWidth) / (this.ActualWidth - 2 * ThumbWidth) * (Maximum - Minimum);
        }

        private void DragLeftThumb(double delta)
        {
            if (DoubleUtil.GreaterThanOrClose(delta, 0))
            {
                var canMoveDis = Math.Min(delta, _centerThumb.ActualWidth);
                Canvas.SetLeft(_leftThumb, Canvas.GetLeft(_leftThumb) + canMoveDis);
            }
            else
            {
                var canMoveDis = Math.Min(-delta, Canvas.GetLeft(_leftThumb));
                Canvas.SetLeft(_leftThumb, Canvas.GetLeft(_leftThumb) - canMoveDis);
            }

            Canvas.SetLeft(_centerThumb, Canvas.GetLeft(_leftThumb) + ThumbWidth);
            _centerThumb.Width = ActualWidth - Canvas.GetRight(_rightThumb) - Canvas.GetLeft(_leftThumb) - 2 * ThumbWidth;
        }

        private void DragRightThumb(double delta)
        {
            if (DoubleUtil.GreaterThanOrClose(delta, 0))
            {
                var canMoveDis = Math.Min(delta, Canvas.GetRight(_rightThumb));
                Canvas.SetRight(_rightThumb, Canvas.GetRight(_rightThumb) - canMoveDis);
            }
            else
            {
                var canMoveDis = Math.Min(-delta, _centerThumb.ActualWidth);
                Canvas.SetRight(_rightThumb, Canvas.GetRight(_rightThumb) + canMoveDis);
            }

            _centerThumb.Width = ActualWidth - Canvas.GetRight(_rightThumb) - Canvas.GetLeft(_leftThumb) - 2 * ThumbWidth;
        }

        private void RelativeDragThumb(double moveDis)
        {
            if (DoubleUtil.GreaterThanOrClose(moveDis, 0))
            {
                var canMoveDis = Math.Min(moveDis,
                    Math.Min(Canvas.GetLeft(_leftThumb), Canvas.GetRight(_rightThumb)));

                Canvas.SetLeft(_leftThumb, Canvas.GetLeft(_leftThumb) - canMoveDis);
                Canvas.SetRight(_rightThumb, Canvas.GetRight(_rightThumb) - canMoveDis);
            }
            else
            {
                var canMoveDis = Math.Min(-moveDis,
                    (ActualWidth - Canvas.GetRight(_rightThumb) - Canvas.GetLeft(_leftThumb) - 2 * ThumbWidth) / 2);

                Canvas.SetLeft(_leftThumb, Canvas.GetLeft(_leftThumb) + canMoveDis);
                Canvas.SetRight(_rightThumb, Canvas.GetRight(_rightThumb) + canMoveDis);
            }

            Canvas.SetLeft(_centerThumb, Canvas.GetLeft(_leftThumb) + ThumbWidth);
            _centerThumb.Width = ActualWidth - Canvas.GetRight(_rightThumb) - Canvas.GetLeft(_leftThumb) - 2 * ThumbWidth;
        }

        private void DragCenterThumb(double moveDis)
        {
            if (DoubleUtil.GreaterThanOrClose(moveDis, 0))
            {
                var canMoveDis = Math.Min(moveDis, Canvas.GetRight(_rightThumb));

                Canvas.SetLeft(_leftThumb, Canvas.GetLeft(_leftThumb) + canMoveDis);
                Canvas.SetRight(_rightThumb, Canvas.GetRight(_rightThumb) - canMoveDis);
            }
            else
            {
                var canMoveDis = Math.Min(-moveDis, Canvas.GetLeft(_leftThumb));

                Canvas.SetLeft(_leftThumb, Canvas.GetLeft(_leftThumb) - canMoveDis);
                Canvas.SetRight(_rightThumb, Canvas.GetRight(_rightThumb) + canMoveDis);
            }

            Canvas.SetLeft(_centerThumb, Canvas.GetLeft(_leftThumb) + ThumbWidth);
            _centerThumb.Width = ActualWidth - Canvas.GetRight(_rightThumb) - Canvas.GetLeft(_leftThumb) - 2 * ThumbWidth;
        }

        private void UnSubscribeEvents()
        {
            /*
            if (_leftThumb != null)
                _leftThumb.DragDelta -= Thumb_DragDelta;

            if (_rightThumb != null)
                _rightThumb.DragDelta -= Thumb_DragDelta;

            if (_centerThumb != null)
                _centerThumb.DragDelta -= Thumb_DragDelta;
                */
        }

        private void SubscribeEvents()
        {
            // Always unsubscribe events to ensure we don't subscribe twice
            UnSubscribeEvents();

            /*
            if (_leftThumb != null)
                _leftThumb.DragDelta += Thumb_DragDelta;

            if (_rightThumb != null)
                _rightThumb.DragDelta += Thumb_DragDelta;

            if (_centerThumb != null)
                _centerThumb.DragDelta += Thumb_DragDelta;
                */
        }

        private void ShowThumbTooltipDuringDargging(Thumb thumb, ref ToolTip autoToolTip)
        {
            if (AutoToolTipPlacement != AutoToolTipPlacement.None)
            {
                if (autoToolTip == null)
                {
                    autoToolTip = new ToolTip();
                }

                autoToolTip.Content = GetAutoToolTipNumber(thumb);

                if (thumb.ToolTip != autoToolTip)
                {
                    thumb.ToolTip = autoToolTip;
                }

                if (!autoToolTip.IsOpen)
                {
                    autoToolTip.IsOpen = true;
                }

                //for Reposition.
                autoToolTip.HorizontalOffset += 1;
                autoToolTip.HorizontalOffset -= 1;
                // ((Popup)_autoToolTip.Parent).Reposition();
            }
        }

        private void ShowThumbToolTip(Thumb thumb, ref ToolTip autoToolTip, ref object originalToolTip)
        {
            // Save original tooltip
            originalToolTip = thumb.ToolTip;

            if (autoToolTip == null)
            {
                autoToolTip = new ToolTip();
                autoToolTip.Placement = PlacementMode.Custom;
                autoToolTip.PlacementTarget = thumb;
                autoToolTip.CustomPopupPlacementCallback = this.AutoToolTipCustomPlacementCallback;
            }

            thumb.ToolTip = autoToolTip;
            autoToolTip.Content = GetAutoToolTipNumber(thumb);
            autoToolTip.IsOpen = true;

            //for Reposition.
            autoToolTip.HorizontalOffset += 1;
            autoToolTip.HorizontalOffset -= 1;
            // ((Popup)_autoToolTip.Parent).Reposition();
        }

        private void HideThumbToolTip(Thumb thumb, ref ToolTip autoToolTip, ref object originalToolTip)
        {
            if (autoToolTip != null)
                autoToolTip.IsOpen = false;

            thumb.ToolTip = originalToolTip;
        }

        private string GetAutoToolTipNumber(Thumb thumb)
        {
            var format = (NumberFormatInfo)(NumberFormatInfo.CurrentInfo.Clone());
            format.NumberDecimalDigits = this.AutoToolTipPrecision;

            switch (thumb.Name)
            {
                case LeftThumbTemplateName:
                    return this.LeftValue.ToString("N", format);
                    break;

                case RightThumbTemplateName:
                    return this.RightValue.ToString("N", format);
                    break;
            }

            return "0";
        }

        private CustomPopupPlacement[] AutoToolTipCustomPlacementCallback(Size popupSize, Size targetSize, Point offset)
        {
            switch (this.AutoToolTipPlacement)
            {
                case AutoToolTipPlacement.TopLeft:

                    // Place popup at top of thumb
                    return new CustomPopupPlacement[]{new CustomPopupPlacement(
                            new Point((targetSize.Width - popupSize.Width) * 0.5, -popupSize.Height),
                            PopupPrimaryAxis.Horizontal)
                        };

                case AutoToolTipPlacement.BottomRight:

                    // Place popup at bottom of thumb
                    return new CustomPopupPlacement[] {
                            new CustomPopupPlacement(
                            new Point((targetSize.Width - popupSize.Width) * 0.5, targetSize.Height) ,
                            PopupPrimaryAxis.Horizontal)
                        };

                default:
                    return new CustomPopupPlacement[] { };
            }
        }

        #endregion

        #region Override

        public override void OnApplyTemplate()
        {
            UnSubscribeEvents();

            base.OnApplyTemplate();

            _leftThumb = GetTemplateChild(LeftThumbTemplateName) as Thumb;
            _centerThumb = GetTemplateChild(CenterThumbTemplateName) as Thumb;
            _rightThumb = GetTemplateChild(RightThumbTemplateName) as Thumb;

            SubscribeEvents();
        }

        #endregion

        #region Event

        public static void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            var slider = sender as RangeSlider;
            slider.UpdateThumbPosition();
        }

        private void RangeSlider_Unloaded(object sender, RoutedEventArgs e)
        {
            UnSubscribeEvents();
        }

        private void RangeSlider_Loaded(object sender, RoutedEventArgs e)
        {
            SubscribeEvents();
            UpdateThumbPosition();
        }

        private static void OnThumbDragStarted(object sender, DragStartedEventArgs e)
        {
            var slider = sender as RangeSlider;
            slider.OnThumbDragStarted(e);
        }

        private static void OnThumbDragDelta(object sender, DragDeltaEventArgs e)
        {
            var slider = sender as RangeSlider;
            slider.OnThumbDragDelta(e);
        }

        private static void OnThumbDragCompleted(object sender, DragCompletedEventArgs e)
        {
            var slider = sender as RangeSlider;
            slider.OnThumbDragCompleted(e);
        }

        private void OnThumbDragStarted(DragStartedEventArgs e)
        {
            var thumb = e.OriginalSource as Thumb;
            if (thumb != null && AutoToolTipPlacement != AutoToolTipPlacement.None)
            {
                if (IsLinked)
                {
                    ShowThumbToolTip(_leftThumb, ref _leftAutoToolTip, ref _leftThumbOriginalToolTip);
                    ShowThumbToolTip(_rightThumb, ref _rightAutoToolTip, ref _rightThumbOriginalToolTip);
                }
                else
                {
                    switch (thumb.Name)
                    {
                        case LeftThumbTemplateName:
                            ShowThumbToolTip(_leftThumb, ref _leftAutoToolTip, ref _leftThumbOriginalToolTip);
                            break;

                        case RightThumbTemplateName:
                            ShowThumbToolTip(_rightThumb, ref _rightAutoToolTip, ref _rightThumbOriginalToolTip);
                            break;

                        case CenterThumbTemplateName:
                            ShowThumbToolTip(_leftThumb, ref _leftAutoToolTip, ref _leftThumbOriginalToolTip);
                            ShowThumbToolTip(_rightThumb, ref _rightAutoToolTip, ref _rightThumbOriginalToolTip);
                            break;
                    }
                }
            }
        }

        private void OnThumbDragCompleted(DragCompletedEventArgs e)
        {
            var thumb = e.OriginalSource as Thumb;
            if (thumb != null && AutoToolTipPlacement != AutoToolTipPlacement.None)
            {
                if (IsLinked)
                {
                    HideThumbToolTip(_leftThumb, ref _leftAutoToolTip, ref _leftThumbOriginalToolTip);
                    HideThumbToolTip(_rightThumb, ref _rightAutoToolTip, ref _rightThumbOriginalToolTip);
                }
                else
                {
                    switch (thumb.Name)
                    {
                        case LeftThumbTemplateName:
                            HideThumbToolTip(_leftThumb, ref _leftAutoToolTip, ref _leftThumbOriginalToolTip);
                            break;

                        case RightThumbTemplateName:
                            HideThumbToolTip(_rightThumb, ref _rightAutoToolTip, ref _rightThumbOriginalToolTip);
                            break;

                        case CenterThumbTemplateName:
                            HideThumbToolTip(_leftThumb, ref _leftAutoToolTip, ref _leftThumbOriginalToolTip);
                            HideThumbToolTip(_rightThumb, ref _rightAutoToolTip, ref _rightThumbOriginalToolTip);
                            break;
                    }
                }

            }
        }

        private void OnThumbDragDelta(DragDeltaEventArgs e)
        {
            var thumb = e.OriginalSource as Thumb;
            if (thumb != null)
            {
                var delta = e.HorizontalChange;
                switch (thumb.Name)
                {
                    case LeftThumbTemplateName:
                        if (IsLinked)
                        {
                            RelativeDragThumb(-delta);

                            UpdateValue();
                            ShowThumbTooltipDuringDargging(_leftThumb, ref _leftAutoToolTip);
                            ShowThumbTooltipDuringDargging(_rightThumb, ref _rightAutoToolTip);
                        }
                        else
                        {
                            DragLeftThumb(delta);
                            UpdateValue();
                            ShowThumbTooltipDuringDargging(_leftThumb, ref _leftAutoToolTip);
                        }

                        break;

                    case RightThumbTemplateName:

                        if (IsLinked)
                        {
                            RelativeDragThumb(delta);

                            UpdateValue();
                            ShowThumbTooltipDuringDargging(_leftThumb, ref _leftAutoToolTip);
                            ShowThumbTooltipDuringDargging(_rightThumb, ref _rightAutoToolTip);
                        }
                        else
                        {
                            DragRightThumb(delta);

                            UpdateValue();
                            ShowThumbTooltipDuringDargging(_rightThumb, ref _rightAutoToolTip);
                        }
                        break;

                    case CenterThumbTemplateName:
                        DragCenterThumb(delta);

                        UpdateValue();
                        ShowThumbTooltipDuringDargging(_leftThumb, ref _leftAutoToolTip);
                        ShowThumbTooltipDuringDargging(_rightThumb, ref _rightAutoToolTip);
                        break;
                }
            }
        }

        #endregion
    }
}

