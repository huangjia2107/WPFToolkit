using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using UIResources.Helps;

namespace UIResources.Controls
{
    [TemplatePart(Name = ScaleTransformTemplateName, Type = typeof(ScaleTransform))]
    [TemplatePart(Name = HorizontalRulerTemplateName, Type = typeof(Ruler))]
    [TemplatePart(Name = VerticalRulerTemplateName, Type = typeof(Ruler))]
    [TemplatePart(Name = ScrollContentPresenterTemplateName, Type = typeof(ScrollContentPresenter))]
    public class ZoomBox : ScrollViewer
    {
        private static readonly Type _typeofSelf = typeof(ZoomBox);

        private const string HorizontalRulerTemplateName = "PART_HorizontalRuler";
        private const string VerticalRulerTemplateName = "PART_VerticalRuler";

        private const string ScrollContentPresenterTemplateName = "PART_ScrollContentPresenter";
        private const string ScaleTransformTemplateName = "PART_ScaleTransform";

        private ScaleTransform _partScaleTransform;
        private Ruler _partHorizontalRuler;
        private Ruler _partVerticalRuler;
        private ScrollContentPresenter _partScrollContentPresenter;
        private FrameworkElement _elementContent;

        static ZoomBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(_typeofSelf, new FrameworkPropertyMetadata(_typeofSelf));
        }

        public ZoomBox()
        {
            this.Loaded += OnLoaded;
            this.SizeChanged += OnSizeChanged;
        } 

        [Bindable(true)]
        public new object Content
        {
            get
            {
                var textBlock = _elementContent as TextBlock;
                if (textBlock != null)
                    return textBlock.Text;

                return GetValue(ContentProperty);
            }
            set { SetValue(ContentProperty, value); }
        }

        #region readonly Properties

        private static readonly DependencyPropertyKey ScalePropertyKey =
           DependencyProperty.RegisterReadOnly("Scale", typeof(double), _typeofSelf, new PropertyMetadata(1d, OnScalePropertyChanged));
        public static readonly DependencyProperty ScaleProperty = ScalePropertyKey.DependencyProperty;
        public double Scale
        {
            get { return (double)GetValue(ScaleProperty); }
        }

        static void OnScalePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            /*
            var zoomBox = sender as ZoomBox;

            if (zoomBox._partHorizontalRuler != null)
                zoomBox._partHorizontalRuler.Scale = (decimal)zoomBox.Scale;

            if (zoomBox._partVerticalRuler != null)
                zoomBox._partVerticalRuler.Scale = (decimal)zoomBox.Scale;
             */
        }

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

        private void InitContent()
        {
            if (_partScrollContentPresenter.Content != null && _partScrollContentPresenter.Content is string)
                _partScrollContentPresenter.Content = new TextBlock { Text = (string)_partScrollContentPresenter.Content };

            var content = _partScrollContentPresenter.Content as FrameworkElement;
            if (content != null)
            {
                _partScaleTransform = new ScaleTransform(1, 1);

                _elementContent = content;
                _elementContent.RenderTransformOrigin = new Point(0.5, 0.5);
                _elementContent.LayoutTransform = _partScaleTransform;

                _elementContent.LayoutUpdated += _elementContent_LayoutUpdated;

            }
        }

        private void _elementContent_LayoutUpdated(object sender, EventArgs e)
        {
            UpdateRulerParams();
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateScaleTransform(false);
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            UpdateScaleTransform(false);
        }

        private void UpdateRulerParams(bool isForce = false)
        {
            if (_partScrollContentPresenter == null || _elementContent == null || _partHorizontalRuler == null || _partVerticalRuler == null || _partScaleTransform == null)
                return;

            if (isForce || _partHorizontalRuler.Scale != (decimal)Scale)
            {
                var offset = _elementContent.TranslatePoint(new Point(), _partScrollContentPresenter);

                using (_partHorizontalRuler.DeferRefresh())
                {
                    _partHorizontalRuler.Scale = (decimal)Scale;
                    SetValue(HorizontalOriginShiftPropertyKey, offset.X);
                }
            }

            if (isForce || _partVerticalRuler.Scale != (decimal)Scale)
            {
                var offset = _elementContent.TranslatePoint(new Point(), _partScrollContentPresenter);

                using (_partVerticalRuler.DeferRefresh())
                {
                    _partVerticalRuler.Scale = (decimal)Scale;
                    SetValue(VerticalOriginShiftPropertyKey, offset.Y);
                }
            }
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

        protected override void OnPreviewMouseWheel(MouseWheelEventArgs e)
        {
            base.OnPreviewMouseWheel(e);

            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
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

        private void ZoomIn()
        {
            SetValue(ScalePropertyKey, Math.Max(1, Scale + 0.5));

            UpdateScaleTransform();
        }

        private void ZoomOut()
        {
            SetValue(ScalePropertyKey, Math.Max(1, Scale - 0.5));

            UpdateScaleTransform();
        }


    }
}
