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

        public static readonly DependencyProperty DurationProperty = DependencyProperty.Register("Duration", typeof(int), typeof(LiveChartGraph), new FrameworkPropertyMetadata(8));
        public int Duration
        {
            get { return (int)GetValue(DurationProperty); }
            set { SetValue(DurationProperty, value); }
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

        private void InsertVisual(Visual visual)
        {
            _children.Insert(0, visual);
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

        public void Reset()
        {
            _pointsDic.Clear();

            while (_visualQueue.Count>0)
            {
                var vr = _visualQueue.Dequeue();
                RemoveVisual(vr.Visual);
            }
        }

        public Pen GetPenByKey(string key)
        {
            if (!_pointsDic.ContainsKey(key))
            {
                var pen = new Pen(UIResources.Helps.Utils.GetBrush(), 1);
                pen.Freeze();

                _pointsDic.Add(key, pen);

                return pen;
            }

            return _pointsDic[key];
        }

        public void AppendPoints(IDictionary<string, PointData<string, long, int>> dic, int yMin, int yMax, long minDataTimestamp, long maxDataTimestamp, (DateTime StartTime, DateTime EndTime) xRange,bool xAxisChanged, params string[] visibleKeys)
        {
            var minAxisTimestamp = CommonUtil.DateTimeToTimestamp(xRange.StartTime);
            var maxAxisTimestamp = CommonUtil.DateTimeToTimestamp(xRange.EndTime);

            var disPerX = this.ActualWidth / (maxAxisTimestamp - minAxisTimestamp);
            var disPerY = (this.ActualHeight) / (yMax - yMin);

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
                    if (visibleKeys != null && !visibleKeys.Contains(kvp.Key))
                        continue;

                    var usedPen = GetPenByKey(kvp.Key);

                    PointPair<long, int>? prePoint = null;
                    var preX = 0d;
                    var preY = 0d;

                    foreach (var point in kvp.Value.Points)
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
