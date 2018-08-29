using System;
using System.Windows;
using System.Windows.Media;
using System.Threading;

namespace UIResources.Controls
{
    public class OutlinedTextBlock : FrameworkElement
    {
        private static readonly Type _typeofSelf = typeof(OutlinedTextBlock);

        private Pen _drawPen = null;
        private SolidColorBrush _fillBrush = null;

        private Geometry _textGeometry = null;
        private Geometry _textHighLightGeometry = null;

        public static readonly DependencyProperty IsHighlightProperty = DependencyProperty.Register("IsHighlight", typeof(bool), _typeofSelf,
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender));
        public bool IsHighlight
        {
            get { return (bool)GetValue(IsHighlightProperty); }
            set { SetValue(IsHighlightProperty, value); }
        }

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), _typeofSelf,
            new FrameworkPropertyMetadata("Text", FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public static readonly DependencyProperty FontFamilyProperty = DependencyProperty.Register("FontFamily", typeof(FontFamily), _typeofSelf,
            new FrameworkPropertyMetadata(new FontFamily("Microsoft YaHei"), FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));
        public FontFamily FontFamily
        {
            get { return (FontFamily)GetValue(FontFamilyProperty); }
            set { SetValue(FontFamilyProperty, value); }
        }

        public static readonly DependencyProperty FontStyleProperty = DependencyProperty.Register("FontStyle", typeof(FontStyle), _typeofSelf,
            new FrameworkPropertyMetadata(FontStyles.Normal, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));
        public FontStyle FontStyle
        {
            get { return (FontStyle)GetValue(FontStyleProperty); }
            set { SetValue(FontStyleProperty, value); }
        }

        public static readonly DependencyProperty FontWeightProperty = DependencyProperty.Register("FontWeight", typeof(FontWeight), _typeofSelf,
            new FrameworkPropertyMetadata(FontWeights.Normal, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));
        public FontWeight FontWeight
        {
            get { return (FontWeight)GetValue(FontWeightProperty); }
            set { SetValue(FontWeightProperty, value); }
        }

        public static readonly DependencyProperty FontSizeProperty = DependencyProperty.Register("FontSize", typeof(double), _typeofSelf,
            new FrameworkPropertyMetadata(12d, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));
        public double FontSize
        {
            get { return (double)GetValue(FontSizeProperty); }
            set { SetValue(FontSizeProperty, value); }
        }

        public static readonly DependencyProperty FillProperty = DependencyProperty.Register("Fill", typeof(Color), _typeofSelf,
            new PropertyMetadata(Colors.White, OnFillPropertyChanged));
        public Color Fill
        {
            get { return (Color)GetValue(FillProperty); }
            set { SetValue(FillProperty, value); }
        }
        static void OnFillPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = d as OutlinedTextBlock;
            if (ctrl._fillBrush != null)
                ctrl._fillBrush.Color = (Color)e.NewValue;
        }

        public static readonly DependencyProperty StrokeProperty = DependencyProperty.Register("Stroke", typeof(Brush), _typeofSelf,
            new PropertyMetadata(Brushes.Black, OnStrokePropertyChanged));
        public Brush Stroke
        {
            get { return (Brush)GetValue(StrokeProperty); }
            set { SetValue(StrokeProperty, value); }
        }
        static void OnStrokePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = d as OutlinedTextBlock;
            if (ctrl._drawPen != null)
                ctrl._drawPen.Brush = (Brush)e.NewValue;
        }

        public static readonly DependencyProperty StrokeThicknessProperty = DependencyProperty.Register("StrokeThickness", typeof(double), _typeofSelf,
            new PropertyMetadata(1d, OnStrokeThicknessPropertyChanged, CoerceStrokeThickness));
        public double StrokeThickness
        {
            get { return (double)GetValue(StrokeThicknessProperty); }
            set { SetValue(StrokeThicknessProperty, value); }
        }
        static void OnStrokeThicknessPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = d as OutlinedTextBlock;
            if (ctrl._drawPen != null)
                ctrl._drawPen.Thickness = (double)e.NewValue;
        }
        static object CoerceStrokeThickness(DependencyObject d, object value)
        {
            return Math.Max(0, (double)value);
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            InitTextInfo();

            return _textHighLightGeometry.Bounds.IsEmpty ? new Size() :
                new Size(
                    Math.Min(availableSize.Width, Math.Max(_textHighLightGeometry.Bounds.Width, _textGeometry.Bounds.Width)),
                    Math.Min(availableSize.Height, Math.Max(_textHighLightGeometry.Bounds.Height, _textGeometry.Bounds.Height)));
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            if (_drawPen == null)
                _drawPen = new Pen(Stroke, StrokeThickness);

            if (_fillBrush == null)
                _fillBrush = new SolidColorBrush(Fill);

            drawingContext.DrawGeometry(_fillBrush, _drawPen, _textGeometry);

            //Draw the text highlight based on the properties that are set.
            if (IsHighlight)
                drawingContext.DrawGeometry(null, _drawPen, _textHighLightGeometry);
        }

        private void InitTextInfo()
        {
            //Create the formatted text based on the properties set.
            var formattedText = new FormattedText(
                Text,
                Thread.CurrentThread.CurrentUICulture,
                FlowDirection.LeftToRight,
                new Typeface(FontFamily, FontStyle, FontWeight, FontStretches.Normal),
                FontSize,
                Brushes.Black); //This brush does not matter since we use the geometry of the text.  

            //Build the geometry object that represents the text.
            _textGeometry = formattedText.BuildGeometry(new Point(0, 0));

            //Build the geometry object that represents the text hightlight.
            _textHighLightGeometry = formattedText.BuildHighlightGeometry(new Point(0, 0));
        }
    }
}

