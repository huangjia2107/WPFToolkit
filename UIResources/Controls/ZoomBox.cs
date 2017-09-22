using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace UIResources.Controls
{
    [TemplatePart(Name = HorizontalRulerTemplateName, Type = typeof(Ruler))]
    [TemplatePart(Name = VerticalRulerTemplateName, Type = typeof(Ruler))]
    [TemplatePart(Name = ScrollContentPresenterTemplateName, Type = typeof(ScrollContentPresenter))]
    public class ZoomBox : ScrollViewer
    {
        private static readonly Type _typeofSelf = typeof(ZoomBox);

        private const string HorizontalRulerTemplateName = "PART_HorizontalRuler";
        private const string VerticalRulerTemplateName = "PART_VerticalRuler";
        private const string ScrollContentPresenterTemplateName = "PART_ScrollContentPresenter";

        private Ruler _partHorizontalRuler;
        private Ruler _partVerticalRuler;
        private ScrollContentPresenter _partScrollContentPresenter;

        private ScaleTransform _partScaleTransform;
        private FrameworkElement _elementContent;
        private bool _isStringContent = false;

        private ViewPoint? _viewPoint = null;
        private struct ViewPoint
        {
            public Point PointToScrollContent { get; set; }
            public Point PointToViewport { get; set; }
        }

        static ZoomBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(_typeofSelf, new FrameworkPropertyMetadata(_typeofSelf));
        }

        public ZoomBox()
        {
            WeakEventManager<FrameworkElement, RoutedEventArgs>.AddHandler(this, "Loaded", OnLoaded);
            WeakEventManager<FrameworkElement, SizeChangedEventArgs>.AddHandler(this, "SizeChanged", OnSizeChanged);
        }

        #region readonly Properties

        private static readonly DependencyPropertyKey HorizontalOriginShiftPropertyKey =
           DependencyProperty.RegisterReadOnly("HorizontalOriginShift", typeof(double), _typeofSelf, new PropertyMetadata(0d));
        public static readonly DependencyProperty HorizontalOriginShiftProperty = HorizontalOriginShiftPropertyKey.DependencyProperty;
        public double HorizontalOriginShift
        {
            get { return (double)GetValue(HorizontalOriginShiftProperty); }
        }

        private static readonly DependencyPropertyKey VerticalOriginShiftPropertyKey =
           DependencyProperty.RegisterReadOnly("VerticalOriginShift", typeof(double), _typeofSelf, new PropertyMetadata(0d));
        public static readonly DependencyProperty VerticalOriginShiftProperty = VerticalOriginShiftPropertyKey.DependencyProperty;
        public double VerticalOriginShift
        {
            get { return (double)GetValue(VerticalOriginShiftProperty); }
        }

        #endregion

        #region Properties

        [Bindable(true)]
        public new object Content
        {
            get
            {
                var textBlock = _elementContent as TextBlock;
                if (textBlock != null && _isStringContent)
                    return textBlock.Text;

                return GetValue(ContentProperty);
            }
            set { SetValue(ContentProperty, value); }
        }

        public static readonly DependencyProperty ScaleProperty =
            DependencyProperty.Register("Scale", typeof(double), _typeofSelf, new PropertyMetadata(1d));
        public double Scale
        {
            get { return (double)GetValue(ScaleProperty); }
            set { SetValue(ScaleProperty, value); }
        }

        public static readonly DependencyProperty UnitProperty =
            DependencyProperty.Register("Unit", typeof(RulerUnit), _typeofSelf, new PropertyMetadata(RulerUnit.Pixel));
        public RulerUnit Unit
        {
            get { return (RulerUnit)GetValue(UnitProperty); }
            set { SetValue(UnitProperty, value); }
        }

        #endregion

        #region Override

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _partHorizontalRuler = GetTemplateChild(HorizontalRulerTemplateName) as Ruler;
            _partVerticalRuler = GetTemplateChild(VerticalRulerTemplateName) as Ruler;

            _partScrollContentPresenter = GetTemplateChild(ScrollContentPresenterTemplateName) as ScrollContentPresenter;

            if (_partHorizontalRuler == null || _partVerticalRuler == null || _partScrollContentPresenter == null)
            {
                throw new NullReferenceException(string.Format("You have missed to specify {0}, {1} or {2} in your template",
                    HorizontalRulerTemplateName, VerticalRulerTemplateName, ScrollContentPresenterTemplateName));
            }

            InitContent();
        }

        #endregion

        #region Event

        protected override void OnPreviewMouseWheel(MouseWheelEventArgs e)
        {
            base.OnPreviewMouseWheel(e);

            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                if (_partScrollContentPresenter.CanHorizontallyScroll || _partScrollContentPresenter.CanVerticallyScroll)
                    _viewPoint = ResetViewPoint();

                if (e.Delta > 0)
                {
                    ZoomIn();
                }
                if (e.Delta < 0)
                {
                    ZoomOut();
                }

                e.Handled = true;
            }
        }

        private void _elementContent_LayoutUpdated(object sender, EventArgs e)
        {
            UpdateRulerParams();
        }

        private void OnSizeChanged(object sender, EventArgs e)
        {
            UpdateScaleTransform(false);
        }

        private void OnLoaded(object sender, EventArgs e)
        {
            UpdateScaleTransform(false);
        }

        #endregion

        #region Func

        private void InitContent()
        {
            if (_partScrollContentPresenter.Content != null && _partScrollContentPresenter.Content is string)
            {
                _partScrollContentPresenter.Content = new TextBlock { Text = (string)_partScrollContentPresenter.Content };
                _isStringContent = true;
            }

            var content = _partScrollContentPresenter.Content as FrameworkElement;
            if (content != null)
            {
                _partScaleTransform = new ScaleTransform(1, 1);

                _elementContent = content;
                _elementContent.RenderTransformOrigin = new Point(0.5, 0.5);
                _elementContent.LayoutTransform = _partScaleTransform;

                _elementContent.LayoutUpdated -= _elementContent_LayoutUpdated;
                _elementContent.LayoutUpdated += _elementContent_LayoutUpdated;
            }
        }

        private void UpdateRulerParams(bool isForce = false)
        {
            if (_partScrollContentPresenter == null || _elementContent == null || _partHorizontalRuler == null || _partVerticalRuler == null || _partScaleTransform == null)
                return;

            if (isForce || _partHorizontalRuler.Scale != (decimal)Scale)
            {
                using (_partHorizontalRuler.DeferRefresh())
                {
                    KeepingHorizontalViewPoint((double)_partHorizontalRuler.Scale);

                    var offset = _elementContent.TranslatePoint(new Point(), _partScrollContentPresenter);

                    _partHorizontalRuler.Scale = (decimal)Scale;
                    SetValue(HorizontalOriginShiftPropertyKey, offset.X + HorizontalOffset);
                }
            }

            if (isForce || _partVerticalRuler.Scale != (decimal)Scale)
            {
                using (_partVerticalRuler.DeferRefresh())
                {
                    KeepingVerticalViewPoint((double)_partVerticalRuler.Scale);

                    var offset = _elementContent.TranslatePoint(new Point(), _partScrollContentPresenter);

                    _partVerticalRuler.Scale = (decimal)Scale;
                    SetValue(VerticalOriginShiftPropertyKey, offset.Y + VerticalOffset);
                }
            }

            if (_partHorizontalRuler.Scale == (decimal)Scale && _partVerticalRuler.Scale == (decimal)Scale)
                _viewPoint = null;
        }

        private void UpdateScaleTransform(bool isManual = true)
        {
            if (_partScaleTransform == null)
                return;

            _partScaleTransform.ScaleX = Scale;
            _partScaleTransform.ScaleY = Scale;

            if (!isManual)
                UpdateRulerParams(true);
        }

        private void KeepingHorizontalViewPoint(double lastScale)
        {
            if (_viewPoint.HasValue)
                ScrollToHorizontalOffset(_viewPoint.Value.PointToScrollContent.X * Scale / lastScale - _viewPoint.Value.PointToViewport.X);
        }

        private void KeepingVerticalViewPoint(double lastScale)
        {
            if (_viewPoint.HasValue)
                ScrollToVerticalOffset(_viewPoint.Value.PointToScrollContent.Y * Scale / lastScale - _viewPoint.Value.PointToViewport.Y);
        }

        private ViewPoint ResetViewPoint()
        {
            var viewPoint = new ViewPoint
            {
                PointToViewport = Mouse.GetPosition(_partScrollContentPresenter)
            };
            viewPoint.PointToScrollContent = new Point(viewPoint.PointToViewport.X + HorizontalOffset, viewPoint.PointToViewport.Y + VerticalOffset);

            return viewPoint;
        }

        private void ZoomIn()
        {
            Scale = Math.Max(1, Scale + 0.5);
            UpdateScaleTransform();
        }

        private void ZoomOut()
        {
            Scale = Math.Max(1, Scale - 0.5);
            UpdateScaleTransform();
        }

        #endregion
    }
}
