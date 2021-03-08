using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Collections.Generic;
using System.Globalization;
using UIResources.Datas;
using Utils.Common;

namespace CASApp.Theme.Controls
{
    public class LiveChartGraph : FrameworkElement
    {
        private VisualCollection _children;
        private DrawingVisual _xAxisVisual = null;
        private (DateTime StartTime, DateTime EndTime)? _xRange = null;

        private readonly Dictionary<string, Pen> _pointsDic = new Dictionary<string, Pen>();
        private readonly Queue<VisualRecord> _visualQueue = new Queue<VisualRecord>();

        struct VisualRecord
        {
            public long MaxAxisTimestamp { get; set; }
            public long MinAxisTimestamp { get; set; }

            public long MinDataTimestamp { get; set; }
            public long MaxDataTimestamp { get; set; }

            public DrawingVisual Visual { get; set; }
        }

        public LiveChartGraph()
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
            if (visual == null)
                return;

            _children.Remove(visual);
            visual = null;
        }

        public void AppendPoints(IDictionary<string, List<PointPair<long, int>>> dic, int yMin, int yMax)
        {
            var maxDataTimestamp = dic.Max(d => d.Value.Last().X);
            var minDataTimestamp = dic.Min(d => d.Value[0].X);

            var xAxisChanged = CheckXAxis(maxDataTimestamp);

            var minAxisTimestamp = CommonUtil.DateTimeToTimestamp(_xRange.Value.StartTime);
            var maxAxisTimestamp = CommonUtil.DateTimeToTimestamp(_xRange.Value.EndTime);

            var disPerX = this.ActualWidth / (maxAxisTimestamp - minAxisTimestamp);
            var disPerY = (this.ActualHeight - 30) / (yMax - yMin);

            if (xAxisChanged && _visualQueue.Count > 0)
            {
                //移除不在当前时间范围内的对象
                while (true)
                {
                    var vr = _visualQueue.Peek();
                    if (vr.MaxDataTimestamp <= minAxisTimestamp)
                    {
                        _visualQueue.Dequeue();
                        RemoveVisual(vr.Visual);

                        continue;
                    }

                    break;
                }

                //平移到当前时间范围的相对位置上
                foreach (var vr in _visualQueue)
                {
                    vr.Visual.Transform = new TranslateTransform(0, 0) { X = -disPerX * (minAxisTimestamp - vr.MinAxisTimestamp) };
                }
            }

            //画当前数据
            var dv = new DrawingVisual();
            using (var dc = dv.RenderOpen())
            {
                foreach (var kvp in dic)
                {
                    if (!_pointsDic.ContainsKey(kvp.Key))
                    {
                        var pen = new Pen(UIResources.Helps.Utils.GetBrush(), 1);
                        pen.Freeze();

                        _pointsDic.Add(kvp.Key, pen);
                    }

                    var usedPen = _pointsDic[kvp.Key];

                    PointPair<long, int>? prePoint = null;
                    var preX = 0d;
                    var preY = 0d;

                    foreach (var point in kvp.Value)
                    {
                        if (point.Y == -1)
                        {
                            prePoint = null;
                            continue;
                        }

                        if (prePoint == null)
                        {
                            prePoint = point;
                            preX = (point.X - minAxisTimestamp) * disPerX;
                            preY = (yMax - point.Y) * disPerY;

                            continue;
                        }

                        var x = (point.X - minAxisTimestamp) * disPerX;
                        var y = (yMax - point.Y) * disPerY;

                        dc.DrawLine(usedPen, new Point(preX, preY), new Point(x, y));

                        prePoint = point;
                        preX = x;
                        preY = y;
                    }
                }
            }

            dv.CacheMode = new BitmapCache();
            AddVisual(dv);

            //记录
            _visualQueue.Enqueue(new VisualRecord { Visual = dv, MinDataTimestamp = minDataTimestamp, MaxDataTimestamp = maxDataTimestamp, MinAxisTimestamp = minAxisTimestamp, MaxAxisTimestamp = maxAxisTimestamp });
        }

        private bool CheckXAxis(long maxTimestamp)
        {
            var maxTime = CommonUtil.TimestampToDateTime(maxTimestamp);

            if (!_xRange.HasValue || maxTime > _xRange.Value.EndTime)
            {
                var endTime = new DateTime(maxTime.Year, maxTime.Month, maxTime.Day, maxTime.Hour, maxTime.Minute, maxTime.Second).AddSeconds(1);
                var startTime = endTime.AddSeconds(-8);

                _xRange = (startTime, endTime);

                RemoveVisual(_xAxisVisual);
                DrawXAxis(startTime, endTime);

                return true;
            }

            return false;
        }

        private void DrawXAxis(DateTime startTime, DateTime endTime)
        {
            var timeSpan = (int)(endTime - startTime).TotalSeconds;

            _xAxisVisual = new DrawingVisual();
            var brush = new Pen(new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3D4655")), 1);
            brush.Freeze();

            using (var dc = _xAxisVisual.RenderOpen())
            {
                dc.DrawLine(brush, new Point(0, ActualHeight - 30), new Point(ActualWidth, ActualHeight - 30));

                for (int i = 1; i < timeSpan; i++)
                {
                    var timeText = GetFormattedText((startTime + TimeSpan.FromSeconds(i)).ToString("HH:mm:ss"));

                    var x = ActualWidth / timeSpan * i;

                    dc.DrawLine(brush, new Point(x, 0), new Point(x, ActualHeight - 30));
                    dc.DrawText(timeText, new Point(x - timeText.Width / 2, this.ActualHeight - 17));
                }
            }

            AddVisual(_xAxisVisual);
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
    }
}
