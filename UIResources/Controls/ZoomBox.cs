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
        private FrameworkElement _content;

        static ZoomBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(_typeofSelf, new FrameworkPropertyMetadata(_typeofSelf));
        }

        public ZoomBox()
        {
            this.Loaded += OnLoaded;
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
            _partScaleTransform = GetTemplateChild(ScaleTransformTemplateName) as ScaleTransform;


            if (_partScaleTransform == null || _partHorizontalRuler == null
            || _partVerticalRuler == null || _partScrollContentPresenter == null)
            {
                throw new NullReferenceException(string.Format("You have missed to specify {0}, {1}, {2} or {3} in your template",
                    HorizontalRulerTemplateName, VerticalRulerTemplateName, ScrollContentPresenterTemplateName, ScaleTransformTemplateName));
            }

            _content = _partScrollContentPresenter.Content as FrameworkElement;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            UpdateRulerOriginShift();
        }

        private void UpdateRulerOriginShift()
        {
            if (_partScrollContentPresenter == null || _content == null || _partHorizontalRuler == null || _partVerticalRuler == null || _partScaleTransform == null)
                return;

            _partScaleTransform.ScaleX = _partScaleTransform.ScaleY = Scale;

            var offset = _content.TranslatePoint(new Point(), _partScrollContentPresenter);

            using (_partHorizontalRuler.DeferRefresh())
            {
                _partHorizontalRuler.Scale = (decimal)Scale;
                SetValue(HorizontalOriginShiftPropertyKey, offset.X);
            }

            using (_partVerticalRuler.DeferRefresh())
            {
                _partVerticalRuler.Scale = (decimal)Scale;
                SetValue(VerticalOriginShiftPropertyKey, offset.Y);
            }
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
            if (_partScaleTransform == null)
                return;

            SetValue(ScalePropertyKey, Math.Max(0, _partScaleTransform.ScaleX + 0.5));

            UpdateRulerOriginShift();
        }

        private void ZoomOut()
        {
            if (_partScaleTransform == null)
                return;

            SetValue(ScalePropertyKey, Scale * 0.5);

            UpdateRulerOriginShift();
        }


    }
}
