using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using UIResources.Datas;
using Utils.Common;

namespace CASApp.Theme.Controls
{
    [TemplatePart(Name = GridTemplateName, Type = typeof(Grid))]
    [TemplatePart(Name = LiveChartGraphTemplateName, Type = typeof(LiveChartGraph))]
    public class LiveChart : Control
    {
        private static readonly Type _typeofSelf = typeof(LiveChart);

        private const string GridTemplateName = "PART_Grid";
        private const string LiveChartGraphTemplateName = "PART_LiveChartGraph";

        private Grid _grid = null;
        private LiveChartGraph _liveChartGraph = null;

        private readonly DrawingGroup _drawingGroup = null;

        static LiveChart()
        {
            DefaultStyleKeyProperty.OverrideMetadata(_typeofSelf, new FrameworkPropertyMetadata(_typeofSelf));
        }

        public LiveChart()
        {
            _drawingGroup = new DrawingGroup();
        }

        public static readonly DependencyProperty YMaxProperty =
            DependencyProperty.Register("YMax", typeof(int), _typeofSelf, new FrameworkPropertyMetadata(-40, FrameworkPropertyMetadataOptions.AffectsArrange, OnYMaxPropertyChanged));
        public int YMax
        {
            get { return (int)GetValue(YMaxProperty); }
            set { SetValue(YMaxProperty, value); }
        }

        static void OnYMaxPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = d as LiveChart;
            ctrl.DrawYAxis();
        }

        public static readonly DependencyProperty YMinProperty =
            DependencyProperty.Register("YMin", typeof(int), _typeofSelf, new FrameworkPropertyMetadata(-120, FrameworkPropertyMetadataOptions.AffectsArrange, OnYMinPropertyChanged));
        public int YMin
        {
            get { return (int)GetValue(YMinProperty); }
            set { SetValue(YMinProperty, value); }
        }

        static void OnYMinPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = d as LiveChart;
            ctrl.DrawYAxis();
        }


        #region Override

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _grid = GetTemplateChild(GridTemplateName) as Grid;
            _liveChartGraph = GetTemplateChild(LiveChartGraphTemplateName) as LiveChartGraph;
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            DrawYAxis();
            drawingContext.DrawDrawing(_drawingGroup);
        }

        #endregion

        #region Public

        public void AppendPoints(IDictionary<string, List<PointPair<long, int>>> dic)
        {
            _liveChartGraph.AppendPoints(dic, YMin, YMax);
        }

        #endregion

        #region Private

        private void DrawYAxis()
        {
            if (_grid == null)
                return;

            var maxWidth = 0d;
            var disPerY = (_grid.ActualHeight - 30) / (YMax - YMin);

            var texts = new List<(FormattedText, double)>();

            for (int i = YMin; i <= YMax; i += 10)
            {
                var y = (YMax - i) * disPerY;

                if (DoubleUtil.GreaterThan(y, _grid.ActualHeight))
                    break;

                var ft = GetFormattedText(i.ToString());
                maxWidth = Math.Max(maxWidth, ft.Width);

                texts.Add((ft, y - ft.Height / 2));
            }

            _grid.ColumnDefinitions[0].Width = new GridLength(maxWidth);

            using (var dc = _drawingGroup.Open())
            {
                texts.ForEach(t => dc.DrawText(t.Item1, new System.Windows.Point(maxWidth - t.Item1.Width, t.Item2)));
            }
        }

        private FormattedText GetFormattedText(string textToFormat)
        {
            var ft = new FormattedText(
                       textToFormat,
                       CultureInfo.CurrentCulture,
                       FlowDirection.LeftToRight,
                       new Typeface("Arial"),
                       13,
                       Brushes.White);

            ft.SetFontWeight(FontWeights.Regular);
            ft.TextAlignment = TextAlignment.Left;

            return ft;
        }

        #endregion


    }
}
