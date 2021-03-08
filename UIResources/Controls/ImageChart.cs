using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows;
using System.Windows.Media.Imaging;
using SWM = System.Windows.Media;

using System.Globalization;
using System.Drawing.Text;
using UIResources.Datas;
using Utils.Common;

namespace UIResources.Controls
{
    [TemplatePart(Name = GridTemplateName, Type = typeof(System.Windows.Controls.Grid))]
    [TemplatePart(Name = ScrollViewerTemplateName, Type = typeof(System.Windows.Controls.ScrollViewer))]
    [TemplatePart(Name = ImageGridTemplateName, Type = typeof(System.Windows.Controls.Grid))]
    [TemplatePart(Name = ImageTemplateName, Type = typeof(System.Windows.Controls.Image))]
    public class ImageChart : System.Windows.Controls.Control
    {
        private const string GridTemplateName = "PART_Grid";
        private const string ScrollViewerTemplateName = "PART_ScrollViewer";
        private const string ImageGridTemplateName = "PART_ImageGrid";
        private const string ImageTemplateName = "PART_Image";

        private System.Windows.Controls.Grid _grid;
        private System.Windows.Controls.ScrollViewer _scrollviewer;
        private System.Windows.Controls.Grid _imageGrid;
        private System.Windows.Controls.Image _image;

        private readonly SWM.DrawingGroup _drawingGroup = null;

        private readonly Dictionary<string, PointData<Pen, long, int>> _pointsDic = new Dictionary<string, PointData<Pen, long, int>>();
        private readonly Dictionary<string, SWM.Drawing> _visualsDic = new Dictionary<string, SWM.Drawing>();

        private const int TimeWithMillisecondWidth = 80;
        private const int TimeWithSecondWidth = 55;
        private const double ZoomRatio = 1.1;
        private const double MaxZoomRatio = 48;

        private long _minTimestamp = 0;
        private long _maxTimestamp = 0;

        private WriteableBitmap _writeableBitmap = null;
        private (double X, double Y)? _dpi = null;

        public ImageChart()
        {
            _drawingGroup = new SWM.DrawingGroup();
        }

        public static readonly DependencyProperty StartTimeProperty =
            DependencyProperty.Register("StartTime", typeof(DateTime), typeof(ImageChart), new PropertyMetadata(DateTime.Now, OnStartTimePropertyChanged));
        public DateTime StartTime
        {
            get { return (DateTime)GetValue(StartTimeProperty); }
            set { SetValue(StartTimeProperty, value); }
        }

        static void OnStartTimePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = d as ImageChart;
            ctrl._minTimestamp = CommonUtil.DateTimeToTimestamp((DateTime)e.NewValue);
        }

        public static readonly DependencyProperty EndTimeProperty =
            DependencyProperty.Register("EndTime", typeof(DateTime), typeof(ImageChart), new PropertyMetadata(DateTime.Now, OnEndTimePropertyChanged));
        public DateTime EndTime
        {
            get { return (DateTime)GetValue(EndTimeProperty); }
            set { SetValue(EndTimeProperty, value); }
        }

        static void OnEndTimePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = d as ImageChart;
            ctrl._maxTimestamp = CommonUtil.DateTimeToTimestamp((DateTime)e.NewValue);
        }

        public static readonly DependencyProperty YMaxProperty =
            DependencyProperty.Register("YMax", typeof(int), typeof(ImageChart), new FrameworkPropertyMetadata(-40, FrameworkPropertyMetadataOptions.AffectsArrange, OnYMaxPropertyChanged));
        public int YMax
        {
            get { return (int)GetValue(YMaxProperty); }
            set { SetValue(YMaxProperty, value); }
        }

        static void OnYMaxPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = d as ImageChart;
            ctrl.DrawYAxis();
        }

        public static readonly DependencyProperty YMinProperty =
            DependencyProperty.Register("YMin", typeof(int), typeof(ImageChart), new FrameworkPropertyMetadata(-120, FrameworkPropertyMetadataOptions.AffectsArrange, OnYMinPropertyChanged));
        public int YMin
        {
            get { return (int)GetValue(YMinProperty); }
            set { SetValue(YMinProperty, value); }
        }

        static void OnYMinPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = d as ImageChart;
            ctrl.DrawYAxis();
        }

        #region Override

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (_scrollviewer != null)
                _scrollviewer.PreviewMouseWheel -= OnPreviewMouseWheel;

            _grid = GetTemplateChild(GridTemplateName) as System.Windows.Controls.Grid;
            _scrollviewer = GetTemplateChild(ScrollViewerTemplateName) as System.Windows.Controls.ScrollViewer;
            _imageGrid = GetTemplateChild(ImageGridTemplateName) as System.Windows.Controls.Grid;
            _image = GetTemplateChild(ImageTemplateName) as System.Windows.Controls.Image;

            if (_scrollviewer != null)
            {
                _scrollviewer.PreviewMouseWheel -= OnPreviewMouseWheel;
                _scrollviewer.PreviewMouseWheel += OnPreviewMouseWheel;
            }
        }

        protected override void OnRender(SWM.DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            DrawYAxis();
            drawingContext.DrawDrawing(_drawingGroup);

            Redraw();
        }

        private void OnPreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
                ZoomIn();
            else
                ZoomOut();
        }

        #endregion

        #region Public

        public void AppendLine(string key, PointPair<long, int>[] points, bool isDeferDraw = false)
        {
            if (_pointsDic.ContainsKey(key))
            {
                _pointsDic[key].Points = points;
            }
            else
            {
                var pointData = new PointData<Pen, long, int> { Pen = new Pen(Helps.Utils.GetDrawingColor(), 1), Points = points };

                _pointsDic.Add(key, pointData);
            }

            if (!isDeferDraw)
                Redraw();
        }

        public void RemoveLine(string key)
        {
            if (_visualsDic.ContainsKey(key))
                _visualsDic.Remove(key);

            if (_pointsDic.ContainsKey(key))
                _pointsDic.Remove(key);

            Redraw();
        }

        public void ClearLines()
        {
            _visualsDic.Clear();
            _pointsDic.Clear();

            _writeableBitmap = null;

            if (_image != null)
                _image.Source = null;

            if (_imageGrid != null)
                _imageGrid.Width = double.NaN;
        }

        public void Dispose()
        {
            ClearLines();

            if (_scrollviewer != null)
                _scrollviewer.PreviewMouseWheel -= OnPreviewMouseWheel;
        }

        public void ZoomIn()
        {
            if (_pointsDic.Count == 0)
                return;

            var newWidth = GetCurrentWidth() * ZoomRatio;
            var maxWidth = _scrollviewer.ViewportWidth * MaxZoomRatio;

            if (DoubleUtil.AreClose(GetCurrentWidth(), maxWidth))
                return;

            if (DoubleUtil.LessThanOrClose(maxWidth, newWidth))
                _imageGrid.Width = maxWidth;
            else
                _imageGrid.Width = newWidth;

            Redraw();
        }

        public void ZoomOut()
        {
            if (_pointsDic.Count == 0)
                return;

            var newWidth = GetCurrentWidth() / ZoomRatio;

            if (DoubleUtil.AreClose(GetCurrentWidth(), _scrollviewer.ViewportWidth))
                return;

            if (DoubleUtil.GreaterThanOrClose(_scrollviewer.ViewportWidth, newWidth))
                _imageGrid.Width = _scrollviewer.ViewportWidth;
            else
                _imageGrid.Width = newWidth;

            Redraw();
        }

        #endregion

        #region Private

        private double GetCurrentWidth()
        {
            return _imageGrid.ActualWidth;// DoubleUtil.IsZero(_scrollviewer.ExtentWidth) ? _scrollviewer.ViewportWidth : _scrollviewer.ExtentWidth;
        }

        private void Redraw()
        {
            if (_pointsDic.Count == 0)
            {
                _image.Source = null;
                _writeableBitmap = null;

                return;
            }

            //在不初始化 _imageGrid.Width 的前提下，每次生成 WriteableBitmap 都会导致 _imageGrid 的 ActualWidth 自动增大 2 像素左右
            //暂未找到原因，故通过赋初值的方式解决
            if (DoubleUtil.IsNaN(_imageGrid.Width) || DoubleUtil.IsZero(_imageGrid.Width))
                _imageGrid.Width = GetCurrentWidth();

            //当最大化窗体时后，此时，_scrollviewer.ViewportWidth 的值仍然为最大化之前的值，但 _scrollviewer.ActualWidth 已经是变更后的值，且有可能会大于 _scrollviewer.ViewportWidth
            //导致只能显示 _scrollviewer.ViewportWidth 大小区域的波形，故通过重置 _imageGrid.Width，间接变更 _scrollviewer.ViewportWidth，与 _scrollviewer.ActualWidth 保持一致
            if (_scrollviewer != null && DoubleUtil.GreaterThan(_scrollviewer.ActualWidth, _scrollviewer.ViewportWidth))
                _imageGrid.Width = _scrollviewer.ActualWidth;

            if (!_dpi.HasValue)
                _dpi = Helps.Utils.GetDpi(this);

            var width = (int)GetCurrentWidth();
            var height = (int)_imageGrid.ActualHeight;

            _writeableBitmap = new WriteableBitmap(width, height, _dpi.Value.X, _dpi.Value.Y, SWM.PixelFormats.Pbgra32, null);

            _writeableBitmap.Lock();
            using (var backBitmap = new Bitmap(width, height, _writeableBitmap.BackBufferStride, System.Drawing.Imaging.PixelFormat.Format32bppPArgb, _writeableBitmap.BackBuffer))
            {
                using (var graphics = Graphics.FromImage(backBitmap))
                {
                    graphics.TextRenderingHint = TextRenderingHint.SingleBitPerPixelGridFit;
                    graphics.SmoothingMode = SmoothingMode.HighQuality;
                    graphics.Clear(Color.Transparent);

                    DrawBgLine(graphics, width);
                    DrawX(graphics, width);

                    foreach (var kvp in _pointsDic)
                    {
                        DrawLine(kvp.Value, graphics, width);
                    }

                    graphics.Flush();
                }
            }

            GC.Collect();

            _writeableBitmap.AddDirtyRect(new Int32Rect(0, 0, width, height));
            _writeableBitmap.Unlock();

            _image.Source = _writeableBitmap;
        }

        private void DrawBgLine(Graphics g, int width)
        {
            var disPerY = (_imageGrid.ActualHeight - 30) / (YMax - YMin);
            var pen = new Pen(ColorTranslator.FromHtml("#3D4655"), 1);

            for (int i = YMin; i <= YMax; i += 10)
            {
                var y = (YMax - i) * disPerY;

                if (DoubleUtil.GreaterThan(y, _imageGrid.ActualHeight))
                    break;

                g.DrawLine(pen, 0, (float)y, (float)width, (float)y);
            }
        }

        private void DrawX(Graphics g, int width)
        {
            var disPerX = width * 1.0 / (_maxTimestamp - _minTimestamp);
            var formatAndInterval = GetXFormatAndInterval(StartTime, EndTime, width);

            var pen = new Pen(ColorTranslator.FromHtml("#3D4655"), 1);

            for (var i = StartTime + TimeSpan.FromMilliseconds(formatAndInterval.interval); i < EndTime; i += TimeSpan.FromMilliseconds(formatAndInterval.interval))
            {
                var timestamp = CommonUtil.DateTimeToTimestamp(i);

                var x = (timestamp - _minTimestamp) * disPerX;
                var y = _imageGrid.ActualHeight - 25;
                var text = i.ToString(formatAndInterval.format);

                //line
                g.DrawLine(pen, (float)x, 0, (float)x, (float)y - 5);

                var font = new Font("Arial", 13F, System.Drawing.FontStyle.Regular, GraphicsUnit.Pixel);
                var fontSize = g.MeasureString(text, font);

                //text
                g.DrawString(text, font, Brushes.White, (float)(x - fontSize.Width / 2), (float)y);
            }
        }

        private void DrawYAxis()
        {
            if (_grid == null)
                return;

            var maxWidth = 0d;
            var disPerY = (_imageGrid.ActualHeight - 30) / (YMax - YMin);

            var texts = new List<(SWM.FormattedText, double)>();

            for (int i = YMin; i <= YMax; i += 10)
            {
                var y = (YMax - i) * disPerY;

                if (DoubleUtil.GreaterThan(y, _imageGrid.ActualHeight))
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

        private void DrawLine(PointData<Pen, long, int> pointData, Graphics g, int width)
        {
            var disPerX = width * 1.0 / (_maxTimestamp - _minTimestamp);
            var disPerY = (_imageGrid.ActualHeight - 30) / (YMax - YMin);

            PointPair<long, int>? prePoint = null;
            var preX = 0d;
            var preY = 0d;

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

                g.DrawLine(pointData.Pen, (float)preX, (float)preY, (float)x, (float)y);

                prePoint = point;
                preX = x;
                preY = y;
            }
        }

        private (string format, int interval) GetXFormatAndInterval(DateTime startTime, DateTime endTime, int width)
        {
            var milliseconds = (endTime - startTime).TotalMilliseconds;

            var list = new int[] { 10, 20, 30, 40, 50, 100, 200, 300, 400, 500, 1000, 2000, 3000, 5000, 10000, 15000, 20000, 30000, 60000, 120000, 180000, 300000, 600000, 1800000, 3600000 };

            foreach (var i in list)
            {
                if (i < 1000)
                {
                    if (TimeWithMillisecondWidth * (milliseconds / i) < width)
                        return ("HH:mm:ss.fff", i);
                }
                else
                {
                    if (TimeWithSecondWidth * (milliseconds / i) < width)
                        return ("HH:mm:ss", i);
                }
            }

            return ("HH:mm:ss", 5000);
        }

        private SWM.FormattedText GetFormattedText(string textToFormat)
        {
            var ft = new SWM.FormattedText(
                       textToFormat,
                       CultureInfo.CurrentCulture,
                       FlowDirection.LeftToRight,
                       new SWM.Typeface("Arial"),
                       13,
                       SWM.Brushes.White);

            ft.SetFontWeight(FontWeights.Regular);
            ft.TextAlignment = TextAlignment.Left;

            return ft;
        }

        #endregion
    }
}
