﻿using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using UIResources.Helps;

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

        private const double MiniScale = 0.01d;
        private const double MaxiScale = 48d;
        private const double ScaleRatio = 1.1;

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

        private DependencyPropertyDescriptor _dependencyPropertyDescriptor;

        static ZoomBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(_typeofSelf, new FrameworkPropertyMetadata(_typeofSelf));
        }

        public ZoomBox()
        {
            _dependencyPropertyDescriptor = DependencyPropertyDescriptor.FromProperty(ContentProperty, _typeofSelf);
            _dependencyPropertyDescriptor.AddValueChanged(this, OnContentChanged);
             
            WeakEventManager<FrameworkElement, RoutedEventArgs>.AddHandler(this, "Loaded", OnLoaded);
            WeakEventManager<FrameworkElement, RoutedEventArgs>.AddHandler(this, "Unloaded", OnUnloaded);
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

        private void OnContentChanged(object sender, EventArgs e)
        {
            InitContent();
        }

        public static readonly DependencyProperty ScaleProperty =
            DependencyProperty.Register("Scale", typeof(double), _typeofSelf, new PropertyMetadata(1d, OnScalePropertyChanged, CoerceScale));
        public double Scale
        {
            get { return (double)GetValue(ScaleProperty); }
            set { SetValue(ScaleProperty, value); }
        }

        private static object CoerceScale(DependencyObject d, object value)
        {
            var val = (double)value;

            if (DoubleUtil.LessThan(val, MiniScale))
                return MiniScale;

            if (DoubleUtil.GreaterThan(val, MaxiScale))
                return MaxiScale;

            return value;
        }

        private static void OnScalePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var zoomBox = sender as ZoomBox;
            zoomBox.UpdateScaleTransform();
        }

        public static readonly DependencyProperty UnitProperty =
            DependencyProperty.Register("Unit", typeof(RulerUnit), _typeofSelf, new PropertyMetadata(RulerUnit.Pixel, OnUnitPropertyChanged));
        public RulerUnit Unit
        {
            get { return (RulerUnit)GetValue(UnitProperty); }
            set { SetValue(UnitProperty, value); }
        }

        private static void OnUnitPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var zoomBox = sender as ZoomBox;
            zoomBox.UpdateRulerParams();
        }

        public static readonly DependencyProperty RibbonProperty =
            DependencyProperty.Register("Ribbon", typeof(object), _typeofSelf, new PropertyMetadata(null, OnRibbonChanged));
        public object Ribbon
        {
            get { return (object)GetValue(RibbonProperty); }
            set { SetValue(RibbonProperty, value); }
        }

        private static void OnRibbonChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var zoomBox = (ZoomBox)d;
            zoomBox.OnContentChanged(e.OldValue, e.NewValue);
        }

        public static readonly DependencyProperty IsShowRibbonProperty =
            DependencyProperty.Register("IsShowRibbon", typeof(bool), _typeofSelf, new PropertyMetadata(true));
        public bool IsShowRibbon
        {
            get { return (bool)GetValue(IsShowRibbonProperty); }
            set { SetValue(IsShowRibbonProperty, value); }
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

        protected override void OnScrollChanged(ScrollChangedEventArgs e)
        {
            base.OnScrollChanged(e);

            if (!IsLoaded)
                return;

            UpdateRulerParams();
        }

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

        private void OnLoaded(object sender, EventArgs e)
        {
            UpdateScaleTransform();
            UpdateRulerParams();
        }

        private void OnUnloaded(object sender, EventArgs e)
        {
            if (_dependencyPropertyDescriptor != null)
                _dependencyPropertyDescriptor.RemoveValueChanged(this, OnContentChanged);

            WeakEventManager<FrameworkElement, RoutedEventArgs>.RemoveHandler(this, "Loaded", OnLoaded);
            WeakEventManager<FrameworkElement, RoutedEventArgs>.RemoveHandler(this, "Unloaded", OnUnloaded);
        }

        #endregion

        #region Func

        private void InitContent()
        {
            if (_partScrollContentPresenter == null)
                return;

            if (_partScrollContentPresenter.Content != null && _partScrollContentPresenter.Content is string)
            {
                _partScrollContentPresenter.Content = new TextBlock { Text = (string)_partScrollContentPresenter.Content };
                _isStringContent = true;
            }
            else
            {
                _isStringContent = false;
            }

            var content = _partScrollContentPresenter.Content as FrameworkElement;
            if (content != null)
            {
                _partScaleTransform = new ScaleTransform(1, 1);

                _elementContent = content;
                _elementContent.RenderTransformOrigin = new Point(0.5, 0.5);
                _elementContent.LayoutTransform = _partScaleTransform;
            }
        }

        private void UpdateRulerParams()
        {
            if (_partScrollContentPresenter == null || _elementContent == null || _partHorizontalRuler == null || _partVerticalRuler == null || _partScaleTransform == null)
                return;

            using (_partHorizontalRuler.DeferRefresh())
            {
                KeepingHorizontalViewPoint((double)_partHorizontalRuler.Scale, _partScrollContentPresenter.CanHorizontallyScroll);

                _partHorizontalRuler.Scale = (decimal)Scale;
                _partHorizontalRuler.Unit = Unit;

                var offset = _elementContent.TranslatePoint(new Point(), _partScrollContentPresenter);
                SetValue(HorizontalOriginShiftPropertyKey, offset.X);
            }

            using (_partVerticalRuler.DeferRefresh())
            {
                KeepingVerticalViewPoint((double)_partVerticalRuler.Scale, _partScrollContentPresenter.CanVerticallyScroll);

                _partVerticalRuler.Scale = (decimal)Scale;
                _partVerticalRuler.Unit = Unit;

                var offset = _elementContent.TranslatePoint(new Point(), _partScrollContentPresenter);
                SetValue(VerticalOriginShiftPropertyKey, offset.Y);
            }

            _viewPoint = null;
        }

        private void UpdateScaleTransform()
        {
            if (_partScaleTransform == null)
                return;

            _partScaleTransform.ScaleX = Scale;
            _partScaleTransform.ScaleY = Scale;
        }

        private void KeepingHorizontalViewPoint(double lastScale, bool canHorizontallyScroll)
        {
            if (_viewPoint.HasValue && canHorizontallyScroll)
                ScrollToHorizontalOffset((_viewPoint.Value.PointToScrollContent.X - _elementContent.Margin.Left) * Scale / lastScale + _elementContent.Margin.Left - _viewPoint.Value.PointToViewport.X);
        }

        private void KeepingVerticalViewPoint(double lastScale, bool canVerticallyScroll)
        {
            if (_viewPoint.HasValue && canVerticallyScroll)
                ScrollToVerticalOffset((_viewPoint.Value.PointToScrollContent.Y - _elementContent.Margin.Top) * Scale / lastScale + _elementContent.Margin.Top - _viewPoint.Value.PointToViewport.Y);
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

        public void ZoomIn()
        {
            Scale = Scale * ScaleRatio;
        }

        public void ZoomOut()
        {
            Scale = Scale / ScaleRatio;
        }

        #endregion
    }
}
