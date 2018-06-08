using System;
using System.Collections.Generic;
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

        private Thumb _leftThumb;
        private Thumb _centerThumb;
        private Thumb _rightThumb;

        static RangeSlider()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RangeSlider), new FrameworkPropertyMetadata(typeof(RangeSlider)));
        }

        public RangeSlider()
        {
            this.Loaded += RangeSlider_Loaded;
            this.Unloaded += RangeSlider_Unloaded;
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

            ctrl.CoerceValue(RightValueProperty);
            ctrl.CoerceValue(LeftValueProperty);
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
            ctrl.CoerceValue(RightValueProperty);
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
            //             RoutedPropertyChangedEventArgs<double> args = new RoutedPropertyChangedEventArgs<double>(oldValue, newValue);
            //             args.RoutedEvent = RangeBase.ValueChangedEvent;
            //             RaiseEvent(args);
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

            ctrl.CoerceValue(LeftValueProperty);
            ctrl.UpdateThumbPosition();
            ctrl.OnRightValueChanged((double)e.OldValue, (double)e.NewValue);
        }
        protected virtual void OnRightValueChanged(double oldValue, double newValue)
        {
            //             RoutedPropertyChangedEventArgs<double> args = new RoutedPropertyChangedEventArgs<double>(oldValue, newValue);
            //             args.RoutedEvent = RangeBase.ValueChangedEvent;
            //             RaiseEvent(args);
        }

        public static readonly DependencyProperty SelectionForegroundProperty =
            DependencyProperty.Register("SelectionForeground", typeof(Brush), typeof(RangeSlider), new PropertyMetadata(Brushes.Green));
        public Brush SelectionForeground
        {
            get { return (Brush)GetValue(SelectionForegroundProperty); }
            set { SetValue(SelectionForegroundProperty, value); }
        }

        public static readonly DependencyProperty SliderWidthProperty =
            DependencyProperty.Register("SliderWidth", typeof(double), typeof(RangeSlider), new PropertyMetadata(20d, OnSliderWidthChanged, CoerceSliderWidth), IsValidDoubleValue);
        public double SliderWidth
        {
            get { return (double)GetValue(SliderWidthProperty); }
            set { SetValue(SliderWidthProperty, value); }
        }
        private static object CoerceSliderWidth(DependencyObject d, object value)
        {
            return Math.Max(0, (double)value);
        }
        private static void OnSliderWidthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = (RangeSlider)d;
            ctrl.UpdateThumbPosition();
        }

        #endregion

        #region Func

        private static bool IsValidDoubleValue(object value)
        {
            double d = (double)value;

            return !(double.IsNaN(d) || double.IsInfinity(d));
        }

        private void UpdateThumbPosition()
        {
            if (!IsLoaded)
                return;

            var leftOffset = (ActualWidth - 2 * SliderWidth) * (LeftValue - Minimum) / (Maximum - Minimum);
            var rightOffset = ActualWidth - (ActualWidth - 2 * SliderWidth) * (RightValue - Minimum) / (Maximum - Minimum) - SliderWidth;
            var selectionWidth = (ActualWidth - 2 * SliderWidth) * (RightValue - LeftValue) / (Maximum - Minimum);

            Canvas.SetLeft(_leftThumb, leftOffset);
            Canvas.SetRight(_rightThumb, rightOffset);

            Canvas.SetLeft(_centerThumb, leftOffset + SliderWidth);
            _centerThumb.Width = selectionWidth;
        }

        private void UpdateValue()
        {
            LeftValue = Canvas.GetLeft(_leftThumb) / (this.ActualWidth - 2 * SliderWidth) * (Maximum - Minimum);
            RightValue = (this.ActualWidth - Canvas.GetRight(_rightThumb) - 2 * SliderWidth) / (this.ActualWidth - 2 * SliderWidth) * (Maximum - Minimum);
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

            Canvas.SetLeft(_centerThumb, Canvas.GetLeft(_leftThumb) + SliderWidth);
            _centerThumb.Width = ActualWidth - Canvas.GetRight(_rightThumb) - Canvas.GetLeft(_leftThumb) - 2 * SliderWidth;
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

            _centerThumb.Width = ActualWidth - Canvas.GetRight(_rightThumb) - Canvas.GetLeft(_leftThumb) - 2 * SliderWidth;
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
                    (ActualWidth - Canvas.GetRight(_rightThumb) - Canvas.GetLeft(_leftThumb) - 2 * SliderWidth) / 2);

                Canvas.SetLeft(_leftThumb, Canvas.GetLeft(_leftThumb) + canMoveDis);
                Canvas.SetRight(_rightThumb, Canvas.GetRight(_rightThumb) + canMoveDis);
            }

            Canvas.SetLeft(_centerThumb, Canvas.GetLeft(_leftThumb) + SliderWidth);
            _centerThumb.Width = ActualWidth - Canvas.GetRight(_rightThumb) - Canvas.GetLeft(_leftThumb) - 2 * SliderWidth;
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

            Canvas.SetLeft(_centerThumb, Canvas.GetLeft(_leftThumb) + SliderWidth);
            _centerThumb.Width = ActualWidth - Canvas.GetRight(_rightThumb) - Canvas.GetLeft(_leftThumb) - 2 * SliderWidth;
        }

        private void UnSubscribeEvents()
        {
            if (_leftThumb != null)
                _leftThumb.DragDelta -= Thumb_DragDelta;

            if (_rightThumb != null)
                _rightThumb.DragDelta -= Thumb_DragDelta;

            if (_centerThumb != null)
                _centerThumb.DragDelta -= Thumb_DragDelta;
        }

        private void SubscribeEvents()
        {
            // Always unsubscribe events to ensure we don't subscribe twice
            UnSubscribeEvents();

            if (_leftThumb != null)
                _leftThumb.DragDelta += Thumb_DragDelta;

            if (_rightThumb != null)
                _rightThumb.DragDelta += Thumb_DragDelta;

            if (_centerThumb != null)
                _centerThumb.DragDelta += Thumb_DragDelta;
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

        private void RangeSlider_Unloaded(object sender, RoutedEventArgs e)
        {
            UnSubscribeEvents();
        }

        private void RangeSlider_Loaded(object sender, RoutedEventArgs e)
        {
            SubscribeEvents();
            UpdateThumbPosition();
        }

        private void Thumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            var thumb = sender as Thumb;
            if (thumb != null)
            {
                var delta = e.HorizontalChange;
                switch (thumb.Name)
                {
                    case LeftThumbTemplateName:
                        //DragLeftThumb(delta);
                        RelativeDragThumb(-delta);
                        break;

                    case RightThumbTemplateName:
                        //DragRightThumb(delta);
                        RelativeDragThumb(delta);
                        break;

                    case CenterThumbTemplateName:
                        DragCenterThumb(delta);
                        break;
                }

                UpdateValue();
            }
        }

        #endregion
    }
}

