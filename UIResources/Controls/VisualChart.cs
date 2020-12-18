using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Globalization;
using CASApp.Theme.Datas;
using System.Windows.Media;
using CASApp.Util.Common;
using CASApp.Theme.Utils;

namespace CASApp.Theme.Controls
{
    public class VisualChart : FrameworkElement
    {
        private readonly VisualCollection _children = null;
        private readonly Dictionary<string, PointData<long, int>> _pointsDic = new Dictionary<string, PointData<long, int>>();
        private readonly Dictionary<string, Visual> _visualsDic = new Dictionary<string, Visual>();

        private const string XAxisTimeVisualKey = "XAxisTime";
        private const string YAxisBgLineVisualKey = "YAxisBgLine";
        private const int TimeWithMillisecondWidth = 80;
        private const int TimeWithSecondWidth = 55;
        private const double ZoomRatio = 2;
        private const double MaxZoomRatio = 48;

        private double _originalWidth = double.NaN;
        private long _minTimestamp = 0;
        private long _maxTimestamp = 0;

        public VisualChart()
        {
            _children = new VisualCollection(this);
        }

        protected override int VisualChildrenCount
        {
            get { return _children.Count; }
        }

        protected override Visual GetVisualChild(int index)
        {
            if (index < 0 || index >= _children.Count)
            {
                throw new ArgumentOutOfRangeException();
            }

            return _children[index];
        }

        private void AddVisual(Visual visual)
        {
            _children.Add(visual);
        }

        private void RemoveVisual(Visual visual)
        {
            _children.Remove(visual);
            visual = null;
        }

        public static readonly DependencyProperty StartTimeProperty =
            DependencyProperty.Register("StartTime", typeof(DateTime), typeof(VisualChart), new PropertyMetadata(DateTime.Now, OnStartTimePropertyChanged));
        public DateTime StartTime
        {
            get { return (DateTime)GetValue(StartTimeProperty); }
            set { SetValue(StartTimeProperty, value); }
        }

        static void OnStartTimePropertyChanged(DependencyObject d,DependencyPropertyChangedEventArgs e)
        {
            var ctrl = d as VisualChart;
            ctrl._minTimestamp = Common.DateTimeToTimestamp((DateTime)e.NewValue);
        }

        public static readonly DependencyProperty EndTimeProperty =
            DependencyProperty.Register("EndTime", typeof(DateTime), typeof(VisualChart), new PropertyMetadata(DateTime.Now, OnEndTimePropertyChanged));
        public DateTime EndTime
        {
            get { return (DateTime)GetValue(EndTimeProperty); }
            set { SetValue(EndTimeProperty, value); }
        }

        static void OnEndTimePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = d as VisualChart;
            ctrl._maxTimestamp = Common.DateTimeToTimestamp((DateTime)e.NewValue);
        }

        public static readonly DependencyProperty YMaxProperty =
            DependencyProperty.Register("YMax", typeof(int), typeof(VisualChart), new PropertyMetadata(-40));
        public int YMax
        {
            get { return (int)GetValue(YMaxProperty); }
            set { SetValue(YMaxProperty, value); }
        }

        public static readonly DependencyProperty YMinProperty =
            DependencyProperty.Register("YMin", typeof(int), typeof(VisualChart), new PropertyMetadata(-120));
        public int YMin
        {
            get { return (int)GetValue(YMinProperty); }
            set { SetValue(YMinProperty, value); }
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            var size = base.ArrangeOverride(finalSize);

            //Redraw();

            return size;
        }

        public void ZoomIn()
        {
            if (_pointsDic.Count == 0)
                return;

            if (double.IsNaN(this.Width))
                _originalWidth = this.ActualWidth;

            var newWidth = GetCurrentWidth() * ZoomRatio;
            var maxWidth = _originalWidth * MaxZoomRatio;

            if(DoubleUtil.AreClose(GetCurrentWidth(), maxWidth))
                return;

            if (DoubleUtil.LessThanOrClose(maxWidth, newWidth))
                this.Width = maxWidth;
            else
                this.Width = newWidth;

            Redraw();
        }

        public void ZoomOut()
        {
            if (_pointsDic.Count == 0)
                return;

            if (double.IsNaN(this.Width))
                _originalWidth = this.ActualWidth;

            var newWidth = GetCurrentWidth() / ZoomRatio;

            if (DoubleUtil.AreClose(GetCurrentWidth(), _originalWidth))
                return;

            if (DoubleUtil.GreaterThanOrClose(_originalWidth, newWidth))
                this.Width = _originalWidth;
            else
                this.Width = newWidth;

            Redraw();
        }

        public void AppendLine(string key, PointPair<long, int>[] points, bool isDeferDraw = false)
        {
            if(!isDeferDraw)
            {
                if (!_visualsDic.ContainsKey(XAxisTimeVisualKey))
                    DrawX();

                if (!_visualsDic.ContainsKey(YAxisBgLineVisualKey))
                    DrawBgLine();
            }

            if (_pointsDic.ContainsKey(key))
            {
                _pointsDic[key].Points = points;

                if(!isDeferDraw)
                {
                    if (_visualsDic.ContainsKey(key))
                    {
                        RemoveVisual(_visualsDic[key]);
                        _visualsDic.Remove(key);
                    }

                    _visualsDic.Add(key, DrawLine(_pointsDic[key]));
                }
            }
            else
            {
                var pen = new Pen(CommonUtil.GetBrush(), 1);
                pen.Freeze();

                var pointData = new PointData<long, int> { Pen = pen, Points = points };

                _pointsDic.Add(key, pointData);

                if (!isDeferDraw)
                    _visualsDic.Add(key, DrawLine(pointData));
            }

            if (isDeferDraw)
                Redraw();
        }

        public void RemoveLine(string key)
        {
            if (_visualsDic.ContainsKey(key))
            {
                RemoveVisual(_visualsDic[key]);
                _visualsDic.Remove(key);
            }

            if (_pointsDic.ContainsKey(key))
                _pointsDic.Remove(key);
        }

        public void ClearLines()
        {
            _visualsDic.Clear();
            _pointsDic.Clear();
            _children.Clear();
        }

        private double GetCurrentWidth()
        {
            return DoubleUtil.IsNaN(this.Width) ? this.ActualWidth : this.Width;
        }

        private DrawingVisual DrawLine(PointData<long,int> pointData)
        {
            var disPerX = GetCurrentWidth() / (_maxTimestamp - _minTimestamp);
            var disPerY = (this.ActualHeight - 30) / (YMax - YMin);

            PointPair<long, int>? prePoint = null;
            var preX = 0d;
            var preY = 0d;

            var dv = new DrawingVisual();
            using (var dc = dv.RenderOpen())
            {
                for (int i = 0; i < pointData.Points.Length; i++)
                {
                    var point = pointData.Points[i];
                    if (point.Y == -1)
                    {
                        prePoint = null;
                        continue;
                    }

                    if (prePoint == null)
                    {
                        prePoint = point;
                        preX = (point.X - _minTimestamp) * disPerX;
                        preY = (YMax - point.Y) * disPerY;

                        continue;
                    }

                    var x = (point.X - _minTimestamp) * disPerX;
                    var y = (YMax - point.Y) * disPerY;

                    dc.DrawLine(pointData.Pen, new Point(preX, preY), new Point(x, y));

                    prePoint = point;
                    preX = x;
                    preY = y;
                }
            }

            AddVisual(dv);

            return dv;
        }

        private void Redraw()
        {
            _children.Clear();
            _visualsDic.Clear();

            if (_pointsDic.Count == 0)
                return;

            DrawBgLine();
            DrawX();

            foreach (var kvp in _pointsDic)
            {
                _visualsDic.Add(kvp.Key, DrawLine(kvp.Value));
            }
        }

        private void DrawBgLine()
        {
            var disPerY = (this.ActualHeight - 30) / (YMax - YMin);

            var pen = new Pen(new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFE5E5E5")), 1);
            pen.Freeze();

            var dv = new DrawingVisual();
            using (var dc = dv.RenderOpen())
            {
                for (int i = YMin; i <= YMax; i += 10)
                {
                    var y = (YMax - i) * disPerY;

                    dc.DrawLine(pen, new Point(0, y), new Point(GetCurrentWidth(), y));
                }
            }

            AddVisual(dv);
            _visualsDic.Add(YAxisBgLineVisualKey, dv);
        }

        private void DrawX()
        {
            var disPerX = GetCurrentWidth() / (_maxTimestamp - _minTimestamp);
            var formatAndInterval = GetXFormatAndInterval(StartTime, EndTime);

            var pen = new Pen(new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFE5E5E5")), 1);
            pen.Freeze();

            var dv = new DrawingVisual();
            using (var dc = dv.RenderOpen())
            {
                for (var i = StartTime+ TimeSpan.FromMilliseconds(formatAndInterval.interval); i < EndTime; i += TimeSpan.FromMilliseconds(formatAndInterval.interval))
                {
                    var timestamp = Common.DateTimeToTimestamp(i);

                    var x = (timestamp - _minTimestamp) * disPerX;
                    var y = this.ActualHeight - 25;

                    var ft = GetFormattedText(i.ToString(formatAndInterval.format));

                    dc.DrawLine(pen, new Point(x, 0), new Point(x, y - 5));
                    dc.DrawText(ft, new Point(x - ft.Width / 2, y));
                }
            }

            AddVisual(dv);
            _visualsDic.Add(XAxisTimeVisualKey, dv);
        }

        private (string format,int interval) GetXFormatAndInterval(DateTime startTime,DateTime endTime)
        {
            var milliseconds = (endTime - startTime).TotalMilliseconds;

            var list = new int[] { 10, 20, 30, 40, 50, 100, 200, 300, 400, 500, 1000, 2000, 3000, 5000, 10000, 15000, 20000, 30000, 60000, 120000, 180000, 300000, 600000, 1800000, 3600000 };

            foreach (var i in list)
            {
                if(i<1000)
                {
                    if (TimeWithMillisecondWidth * (milliseconds / i) < GetCurrentWidth())
                        return ("HH:mm:ss.fff", i);
                }
                else
                {
                    if (TimeWithSecondWidth * (milliseconds / i) < GetCurrentWidth())
                        return ("HH:mm:ss", i);
                }
            }

            return ("HH:mm:ss", 5000);
        }

        private FormattedText GetFormattedText(string textToFormat)
        {
            var ft = new FormattedText(
                       textToFormat,
                       CultureInfo.CurrentCulture,
                       FlowDirection.LeftToRight,
                       new Typeface("Arial"),
                       13,
                       Brushes.Black,
                       null,
                       TextFormattingMode.Display,
                       1d);

            ft.SetFontWeight(FontWeights.Regular);
            ft.TextAlignment = TextAlignment.Left;

            return ft;
        }

        public void Dispose()
        {
            _children.Clear();
            _pointsDic.Clear();
            _visualsDic.Clear();
        }
    }
}
