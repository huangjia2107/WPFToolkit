using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace UIResources.Controls
{
    [TemplatePart(Name = PART_DraggingThumb, Type = typeof(Thumb))]
    [TemplatePart(Name = PART_SwitchTrack, Type = typeof(Grid))]
    [TemplatePart(Name = PART_ThumbIndicator, Type = typeof(Ellipse))]
    [TemplatePart(Name = PART_ThumbTranslate, Type = typeof(TranslateTransform))]
    public class ToggleSwitch : ToggleButton
    {
        private static readonly Type _typeofSelf = typeof(ToggleSwitch);

        private const string PART_DraggingThumb = "PART_DraggingThumb";
        private const string PART_SwitchTrack = "PART_SwitchTrack";
        private const string PART_ThumbIndicator = "PART_ThumbIndicator";
        private const string PART_ThumbTranslate = "PART_ThumbTranslate";

        private Thumb _draggingThumb = null;
        private Grid _switchTrack = null;
        private Ellipse _thumbIndicator = null;
        private TranslateTransform _thumbTranslate = null;

        private DoubleAnimation _thumbAnimation = null;

        private double? _lastDragPosition = null;
        private bool _isDragging;

        static ToggleSwitch()
        {
            DefaultStyleKeyProperty.OverrideMetadata(_typeofSelf, new FrameworkPropertyMetadata(_typeofSelf));
        }

        #region Override

        protected override void OnChecked(RoutedEventArgs e)
        {
            base.OnChecked(e);

            UpdateThumb();
        }

        protected override void OnUnchecked(RoutedEventArgs e)
        {
            base.OnUnchecked(e);

            UpdateThumb();
        }

        protected override void OnToggle()
        {
            IsChecked = IsChecked != true;
            UpdateThumb();
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            UnsubscribeEvents();

            _draggingThumb = GetTemplateChild(PART_DraggingThumb) as Thumb;
            _switchTrack = GetTemplateChild(PART_SwitchTrack) as Grid;
            _thumbIndicator = GetTemplateChild(PART_ThumbIndicator) as Ellipse;
            _thumbTranslate = GetTemplateChild(PART_ThumbTranslate) as TranslateTransform;

            SubscribeEvents();
        }

        #endregion

        #region Private

        private void UnsubscribeEvents()
        {
            if (_draggingThumb != null)
            {
                _draggingThumb.DragStarted -= OnDragStarted;
                _draggingThumb.DragDelta -= OnDragDelta;
                _draggingThumb.DragCompleted -= OnDragCompleted;
            }

            if (_switchTrack != null)
                _switchTrack.SizeChanged -= OnSizeChanged;
        }

        private void SubscribeEvents()
        {
            if (_draggingThumb != null)
            {
                _draggingThumb.DragStarted += OnDragStarted;
                _draggingThumb.DragDelta += OnDragDelta;
                _draggingThumb.DragCompleted += OnDragCompleted;
            }

            if (_switchTrack != null)
                _switchTrack.SizeChanged += OnSizeChanged;
        }

        private double GetThumbIndicatorDestination()
        {
            return IsChecked.GetValueOrDefault() ? ActualWidth - (_switchTrack.Margin.Left + _switchTrack.Margin.Right + _thumbIndicator.Margin.Left + _thumbIndicator.Margin.Right + _thumbIndicator.ActualWidth) : 0;
        }

        private void UpdateThumb()
        {
            if (_thumbTranslate != null && _switchTrack != null && _thumbIndicator != null)
            {
                var destination = GetThumbIndicatorDestination();

                _thumbAnimation = new DoubleAnimation();
                _thumbAnimation.To = destination;
                _thumbAnimation.Duration = TimeSpan.FromMilliseconds(300);
                //_thumbAnimation.EasingFunction = new ExponentialEase() { Exponent = 9 };
                _thumbAnimation.FillBehavior = FillBehavior.Stop;

                AnimationTimeline currentAnimation = _thumbAnimation;
                _thumbAnimation.Completed += (sender, e) =>
                {
                    if (_thumbAnimation != null && currentAnimation == _thumbAnimation)
                    {
                        _thumbTranslate.X = destination;
                        _thumbAnimation = null;
                    }
                };
                _thumbTranslate.BeginAnimation(TranslateTransform.XProperty, _thumbAnimation);
            }
        }

        #endregion

        #region Events

        private void OnDragStarted(object sender, DragStartedEventArgs e)
        {
            if (_thumbTranslate != null)
            {
                _thumbTranslate.BeginAnimation(TranslateTransform.XProperty, null);
                _thumbTranslate.X = GetThumbIndicatorDestination();
                _thumbAnimation = null;
            }
            _lastDragPosition = _thumbTranslate.X;
            _isDragging = false;
        }

        private void OnDragDelta(object sender, DragDeltaEventArgs e)
        {
            if (_lastDragPosition.HasValue)
            {
                if (Math.Abs(e.HorizontalChange) > 3)
                    _isDragging = true;
                if (_switchTrack != null && _thumbIndicator != null)
                {
                    double lastDragPosition = _lastDragPosition.Value;
                    _thumbTranslate.X = Math.Min(ActualWidth - (_switchTrack.Margin.Left + _switchTrack.Margin.Right + _thumbIndicator.ActualWidth / 2 * 3), Math.Max(0, lastDragPosition + e.HorizontalChange));
                }
            }
        }

        private void OnDragCompleted(object sender, DragCompletedEventArgs e)
        {
            _lastDragPosition = null;
            if (!_isDragging)
            {
                OnToggle();
            }
            else if (_thumbTranslate != null && _switchTrack != null)
            {
                if (!IsChecked.GetValueOrDefault() && _thumbTranslate.X + 6.5 >= _switchTrack.ActualWidth / 2)
                {
                    OnToggle();
                }
                else if (IsChecked.GetValueOrDefault() && _thumbTranslate.X + 6.5 <= _switchTrack.ActualWidth / 2)
                {
                    OnToggle();
                }
                else UpdateThumb();
            }
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (_thumbTranslate != null && _switchTrack != null && _thumbIndicator != null)
            {
                _thumbTranslate.X = GetThumbIndicatorDestination();
            }
        }

        #endregion
    }
}