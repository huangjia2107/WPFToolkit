using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Input;
using System.Collections.Generic;
using System.Globalization;
using System.Collections.Concurrent;
using UIResources.Datas;
using Utils.Common;

namespace CASApp.Theme.Controls
{
    public class LiveChartGraph : FrameworkElement
    {
        private VisualCollection _children;

        private readonly ConcurrentDictionary<string, Pen> _keyPenDic = new ConcurrentDictionary<string, Pen>();
        private readonly Queue<VisualRecord> _recordQueue = new Queue<VisualRecord>();
        private readonly List<VisualRecord> _recordPool = new List<VisualRecord>();

        private readonly double _pointDiff = 5;

        class VisualRecord
        {
            public long MaxAxisTimestamp { get; set; }
            public long MinAxisTimestamp { get; set; }

            public long MinDataTimestamp { get; set; }
            public long MaxDataTimestamp { get; set; }

            public double MinOriginalX { get; set; }
            public double MaxOriginalX { get; set; }

            public Dictionary<string, List<PointPair<int, int>>> PointDic { get; set; } = new Dictionary<string, List<PointPair<int, int>>>();

            public DrawingVisual Visual { get; set; }

            public bool IsIdle { get; set; } = true;

            public void Reset()
            {
                IsIdle = true;
                PointDic.Clear();
            }
        }

        public LiveChartGraph()
        {
            _children = new VisualCollection(this);
        }

        public static readonly RoutedEvent LineSelectedEvent = EventManager.RegisterRoutedEvent("LineSelected", RoutingStrategy.Bubble, typeof(RoutedPropertyChangedEventHandler<string>), typeof(LiveChartGraph));
        public event RoutedPropertyChangedEventHandler<string> LineSelected
        {
            add { AddHandler(LineSelectedEvent, value); }
            remove { RemoveHandler(LineSelectedEvent, value); }
        }

        private static readonly DependencyPropertyKey PointBrushPropertyKey =
          DependencyProperty.RegisterReadOnly("PointBrush", typeof(Brush), typeof(LiveChartGraph), new PropertyMetadata(Brushes.Transparent));
        public static readonly DependencyProperty PointBrushProperty = PointBrushPropertyKey.DependencyProperty;
        public Brush PointBrush
        {
            get { return (Brush)GetValue(PointBrushProperty); }
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

        private void AddVisual(DrawingVisual visual)
        {
            if(!_children.Contains(visual))
                _children.Add(visual);

            var tt = (visual.Transform as TranslateTransform);
            if (tt != null)
                tt.X = 0;

            visual.Opacity = 1;
        }

        private void RemoveVisual(DrawingVisual visual)
        {
            if (visual == null)
                return;

            visual.Opacity = 0;
            //_children.Remove(visual);
        }

        private Brush GetUniqueBrush()
        {
            Brush b;

            do
            {
                b = UIResources.Helps.Utils.GetBrush();
            } while (_keyPenDic.Any(kvp => kvp.Value.Brush.ToString() == b.ToString()));

            return b;
        }

        private bool TryFindClosePoint(List<PointPair<int, int>> points, int xOffset, int x, out PointPair<int, int> point)
        {
            var low = 0;
            var high = points.Count - 1;

            while (low <= high)
            {
                var middle = (low + high) / 2;
                if (Math.Abs(points[middle].X + xOffset - x) <= _pointDiff)
                {
                    point = points[middle];
                    return true;
                }
                else if (x > points[middle].X + xOffset)
                {
                    low = middle + 1;
                }
                else
                {
                    high = middle - 1;
                }
            }

            point = new PointPair<int, int>();
            return false;
        }

        private void FindKeyByPoint(DrawingVisual dv, Point pos)
        {
            var vr = _recordQueue.FirstOrDefault(v => v.Visual == dv);

            foreach (var dic in vr.PointDic)
            {
                var tt = dv.Transform as TranslateTransform;
                var xOffset = tt == null ? 0 : tt.X;

                if (TryFindClosePoint(dic.Value, (int)xOffset, (int)pos.X, out var p))
                {
                    if (DoubleUtil.LessThanOrClose(Math.Abs(p.Y - pos.Y), _pointDiff))
                    {
                        SetValue(PointBrushPropertyKey, _keyPenDic[dic.Key].Brush);
                        RaiseEvent(new RoutedPropertyChangedEventArgs<string>(null, dic.Key, LineSelectedEvent));

                        return;
                    }
                }
            }

            SetValue(PointBrushPropertyKey, null);
            RaiseEvent(new RoutedPropertyChangedEventArgs<string>(null, null, LineSelectedEvent));
        }

        private VisualRecord GetRecord(long minDataTimestamp, long maxDataTimestamp, long minAxisTimestamp, long maxAxisTimestamp)
        {
            var idleRecord = _recordPool.FirstOrDefault(r => r.IsIdle);
            if (idleRecord == null)
            {
                idleRecord = new VisualRecord();
                _recordPool.Add(idleRecord);
            }

            idleRecord.IsIdle = false;
            idleRecord.MinDataTimestamp = minDataTimestamp;
            idleRecord.MaxDataTimestamp = maxDataTimestamp;
            idleRecord.MinAxisTimestamp = minAxisTimestamp;
            idleRecord.MaxAxisTimestamp = maxAxisTimestamp;

            return idleRecord;
        }

        public void FindKeyByPoint(MouseButtonEventArgs e)
        {
            var pos = e.GetPosition(this);

            foreach (var vr in _recordQueue)
            {
                var tt = vr.Visual.Transform as TranslateTransform;
                var xOffset = tt == null ? 0 : tt.X;

                if (DoubleUtil.GreaterThanOrClose(pos.X, vr.MinOriginalX + xOffset) && DoubleUtil.LessThanOrClose(pos.X, vr.MaxOriginalX + xOffset))
                {
                    FindKeyByPoint(vr.Visual, pos);
                    return;
                }
            }

            SetValue(PointBrushPropertyKey, null);
            RaiseEvent(new RoutedPropertyChangedEventArgs<string>(null, null, LineSelectedEvent));
        }

        public void Reset()
        {
            _keyPenDic.Clear();

            while (_recordQueue.Count > 0)
            {
                var vr = _recordQueue.Dequeue();

                vr.Reset();
                RemoveVisual(vr.Visual);
            }

            GC.Collect();
        }

        public Pen GetPenByKey(string key)
        {
            if (!_keyPenDic.ContainsKey(key))
            {
                var pen = new Pen(UIResources.Helps.Utils.GetBrush(), 1);
                pen.Freeze();

                _keyPenDic.TryAdd(key, pen);

                return pen;
            }

            return _keyPenDic[key];
        }

        public void AppendPoints(IDictionary<string, PointData<string, long, int>> dic, int yMin, int yMax, long minDataTimestamp, long maxDataTimestamp, (DateTime StartTime, DateTime EndTime) xRange,bool xAxisChanged, params string[] visibleKeys)
        {
            var minAxisTimestamp = CommonUtil.DateTimeToTimestamp(xRange.StartTime);
            var maxAxisTimestamp = CommonUtil.DateTimeToTimestamp(xRange.EndTime);

            var disPerX = this.ActualWidth / (maxAxisTimestamp - minAxisTimestamp);
            var disPerY = (this.ActualHeight) / (yMax - yMin);

            if (xAxisChanged)
            {
                //移除不在当前时间范围内的对象
                while (_recordQueue.Count > 0)
                {
                    var vr = _recordQueue.Peek();
                    if (vr.MaxDataTimestamp <= minAxisTimestamp)
                    {
                        _recordQueue.Dequeue();
                        vr.Reset();

                        RemoveVisual(vr.Visual);
                        continue;
                    }

                    break;
                }

                //平移到当前时间范围的相对位置上
                foreach (var vr in _recordQueue)
                {
                    var tt = vr.Visual.Transform as TranslateTransform;

                    if (tt == null)
                        vr.Visual.Transform = new TranslateTransform(0, 0) { X = -disPerX * (minAxisTimestamp - vr.MinAxisTimestamp) };
                    else
                        tt.X = -disPerX * (minAxisTimestamp - vr.MinAxisTimestamp);
                }
            }

            var visualRecord = GetRecord(minDataTimestamp, maxDataTimestamp, minAxisTimestamp, maxAxisTimestamp);

            //画当前数据
            if (visualRecord.Visual == null)
                visualRecord.Visual = new DrawingVisual { CacheMode = new BitmapCache() };

            var dv = visualRecord.Visual;
            using (var dc = dv.RenderOpen())
            {
                var minX = double.MaxValue;
                var maxX = 0d;

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

                        minX = Math.Min(x, minX);
                        maxX = Math.Max(x, maxX);

                        //缓存坐标点
                        if (visualRecord.PointDic.ContainsKey(kvp.Key))
                            visualRecord.PointDic[kvp.Key].Add(new PointPair<int, int> { X = (int)x, Y = (int)y });
                        else
                            visualRecord.PointDic.Add(kvp.Key, new List<PointPair<int, int>> { new PointPair<int, int> { X = (int)x, Y = (int)y } });

                        dc.DrawLine(usedPen, new Point(preX, preY), new Point(x, y));

                        prePoint = point;
                        preX = x;
                        preY = y;
                    }
                }

                visualRecord.MinOriginalX = minX;
                visualRecord.MaxOriginalX = maxX;
            }

            dv.Drawing?.Freeze();
            AddVisual(dv);

            //记录
            _recordQueue.Enqueue(visualRecord);
        }
    }
}
