using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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
        private (DateTime StartTime, DateTime EndTime)? _xRange = null;

        private readonly Pen _pen = null;

        private readonly DrawingGroup _drawingGroup = null;

        public struct MarkRecord
        {
            public string Key { get; set; }
            public string Color { get; set; }
        }

        static LiveChart()
        {
            DefaultStyleKeyProperty.OverrideMetadata(_typeofSelf, new FrameworkPropertyMetadata(_typeofSelf));
        }

        public LiveChart()
        {
            _drawingGroup = new DrawingGroup();

            _pen = new Pen(new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3D4655")), 1);
            _pen.Freeze();
        }

        private static readonly DependencyPropertyKey RecordsPropertyKey =
          DependencyProperty.RegisterReadOnly("Records", typeof(ObservableCollection<MarkRecord>), _typeofSelf, new PropertyMetadata(new ObservableCollection<MarkRecord>()));
        public static readonly DependencyProperty RecordsProperty = RecordsPropertyKey.DependencyProperty;
        public ObservableCollection<MarkRecord> Records
        {
            get { return (ObservableCollection<MarkRecord>)GetValue(RecordsProperty); }
        }

        public static readonly DependencyProperty SelectedKeyProperty = DependencyProperty.Register("SelectedKey", typeof(string), _typeofSelf);
        public string SelectedKey
        {
            get { return (string)GetValue(SelectedKeyProperty); }
            set { SetValue(SelectedKeyProperty, value); }
        }

        public static readonly DependencyProperty MarkToolTipProperty = DependencyProperty.Register("MarkToolTip", typeof(string), _typeofSelf);
        public string MarkToolTip
        {
            get { return (string)GetValue(MarkToolTipProperty); }
            set { SetValue(MarkToolTipProperty, value); }
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
            ctrl.Redraw();
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
            ctrl.Redraw();
        }

        public static readonly DependencyProperty DurationProperty = DependencyProperty.Register("Duration", typeof(int), _typeofSelf, new FrameworkPropertyMetadata(8));
        public int Duration
        {
            get { return (int)GetValue(DurationProperty); }
            set { SetValue(DurationProperty, value); }
        }

        #region Override

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (_liveChartGraph != null)
                _liveChartGraph.LineSelected -= OnLineSelected;

            _grid = GetTemplateChild(GridTemplateName) as Grid;
            _liveChartGraph = GetTemplateChild(LiveChartGraphTemplateName) as LiveChartGraph;

            if (_liveChartGraph != null)
                _liveChartGraph.LineSelected += OnLineSelected;
        }

     
        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseLeftButtonDown(e);

            if (_liveChartGraph == null)
                return;

            _liveChartGraph.FindKeyByPoint(e);
        }
         
        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            Redraw();
            drawingContext.DrawDrawing(_drawingGroup);
        }

        #endregion

        #region Event

        private void OnLineSelected(object sender, RoutedPropertyChangedEventArgs<string> e)
        {
            SelectedKey = e.NewValue;
        }

        #endregion

        #region Public

        public void Reset()
        {
            _liveChartGraph.Reset();
            Records.Clear();
            _xRange = null;
        }

        public void AppendPoints(IDictionary<string, PointData<string, long, int>> dic, string[] visibleKeys)
        {
            Records.Clear();

            var maxDataTimestamp = dic.Max(d => d.Value.Points.Last().X);
            var minDataTimestamp = dic.Min(d => d.Value.Points[0].X);

            var xAxisChanged = CheckXAxis(maxDataTimestamp);
            if (xAxisChanged)
                Redraw();

            _liveChartGraph.AppendPoints(dic, YMin, YMax, minDataTimestamp, maxDataTimestamp, _xRange.Value, xAxisChanged, visibleKeys);

            Records.AddRange(dic.Where(kvp => !string.IsNullOrEmpty(kvp.Value.Value) && visibleKeys.Contains(kvp.Value.Value)).Select(kvp => new MarkRecord { Key = kvp.Value.Value, Color = _liveChartGraph.GetPenByKey(kvp.Key).Brush.ToString() }));
        }

        #endregion

        #region Private

        private void Redraw()
        {
            using (var dc = _drawingGroup.Open())
            {
                var yMarkWidth = DrawYAxis(dc);

                DrawXAxis(dc, yMarkWidth + 10);
            }
        }

        private bool CheckXAxis(long maxTimestamp)
        {
            var maxTime = Common.TimestampToDateTime(maxTimestamp);

            if (!_xRange.HasValue || maxTime > _xRange.Value.EndTime)
            {
                var endTime = new DateTime(maxTime.Year, maxTime.Month, maxTime.Day, maxTime.Hour, maxTime.Minute, maxTime.Second).AddSeconds(1);
                var startTime = endTime.AddSeconds(-Duration);

                _xRange = (startTime, endTime);

                return true;
            }

            return false;
        }

        private void DrawXAxis(DrawingContext dc, double leftMargin = 0)
        {
            if (!_xRange.HasValue)
                return;

            var timeSpan = (int)(_xRange.Value.EndTime - _xRange.Value.StartTime).TotalSeconds;
            dc.DrawLine(_pen, new Point(leftMargin, ActualHeight - 30), new Point(ActualWidth, ActualHeight - 30));

            for (int i = 1; i < timeSpan; i++)
            {
                var timeText = GetFormattedText((_xRange.Value.StartTime + TimeSpan.FromSeconds(i)).ToString("HH:mm:ss"));

                var x = (ActualWidth - leftMargin) / timeSpan * i;

                dc.DrawLine(_pen, new Point(x + leftMargin, 0), new Point(x + leftMargin, ActualHeight - 30));
                dc.DrawText(timeText, new Point(x + leftMargin - timeText.Width / 2, this.ActualHeight - 17));
            }
        }

        private double DrawYAxis(DrawingContext dc)
        {
            if (_grid == null)
                return 0;

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
            texts.ForEach(t => dc.DrawText(t.Item1, new Point(maxWidth - t.Item1.Width, t.Item2)));

            return maxWidth;
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
