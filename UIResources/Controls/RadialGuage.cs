using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using UIResources.Helps;

namespace UIResources.Controls
{
    /// <summary>
    ///  http://www.codeproject.com/Articles/38361/Circular-gauge-custom-control-for-Silverlight-an
    /// </summary>
    [TemplatePart(Name = PART_LayoutRoot, Type = typeof(Grid))]
    [TemplatePart(Name = PART_IndicatorRoot, Type = typeof(Grid))]
    [TemplatePart(Name = PART_PointerCap, Type = typeof(Ellipse))]
    [TemplatePart(Name = PART_Pointer, Type = typeof(Path))]
    [TemplatePart(Name = PART_PointerRT, Type = typeof(RotateTransform))]
    public class RadialGuage : Control
    {
        private static readonly Type _typeofSelf = typeof(RadialGuage);

        private const string PART_LayoutRoot = "PART_LayoutRoot";
        private const string PART_IndicatorRoot = "PART_IndicatorRoot";
        private const string PART_Pointer = "PART_Pointer";
        private const string PART_PointerCap = "PART_PointerCap";
        private const string PART_PointerRT = "PART_PointerRT";

        private Grid _layoutRoot = null;
        private Grid _indicatorRoot = null;
        private Ellipse _pointerCap = null;
        private Path _pointer = null;
        private RotateTransform _pointerRt = null;

        static RadialGuage()
        {
            DefaultStyleKeyProperty.OverrideMetadata(_typeofSelf, new FrameworkPropertyMetadata(_typeofSelf));
        }

        public static readonly DependencyProperty TitleMarginProperty = DependencyProperty.Register("TitleMargin", typeof(Thickness), _typeofSelf, new FrameworkPropertyMetadata(new Thickness()), IsThicknessValid);
        public Thickness TitleMargin
        {
            get { return (Thickness)GetValue(TitleMarginProperty); }
            set { SetValue(TitleMarginProperty, value); }
        }
        static bool IsThicknessValid(object value)
        {
            return ((Thickness)value).IsValid(false, false, false, false);
        }

        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register("Title", typeof(string), _typeofSelf);
        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        public static readonly DependencyProperty RadiusProperty = DependencyProperty.Register("Radius", typeof(double), _typeofSelf, new UIPropertyMetadata(90d, PropertyChangedCallback));
        public double Radius
        {
            get { return (double)GetValue(RadiusProperty); }
            set { SetValue(RadiusProperty, value); }
        }

        public static readonly DependencyProperty ScaleRadiusProperty = DependencyProperty.Register("ScaleRadius", typeof(double), _typeofSelf, new UIPropertyMetadata(50d, PropertyChangedCallback));
        public double ScaleRadius
        {
            get { return (double)GetValue(ScaleRadiusProperty); }
            set { SetValue(ScaleRadiusProperty, value); }
        }

        public static readonly DependencyProperty ScaleTextRadiusProperty = DependencyProperty.Register("ScaleTextRadius", typeof(double), _typeofSelf, new UIPropertyMetadata(70d, PropertyChangedCallback));
        public double ScaleTextRadius
        {
            get { return (double)GetValue(ScaleTextRadiusProperty); }
            set { SetValue(ScaleTextRadiusProperty, value); }
        }

        public static readonly DependencyProperty IndicatorRadiusProperty = DependencyProperty.Register("IndicatorRadius", typeof(double), _typeofSelf, new UIPropertyMetadata(35d, PropertyChangedCallback));
        public double IndicatorRadius
        {
            get { return (double)GetValue(IndicatorRadiusProperty); }
            set { SetValue(IndicatorRadiusProperty, value); }
        }

        public static readonly DependencyProperty IndicatorThicknessProperty = DependencyProperty.Register("IndicatorThickness", typeof(double), _typeofSelf, new UIPropertyMetadata(8d, PropertyChangedCallback));
        public double IndicatorThickness
        {
            get { return (double)GetValue(IndicatorThicknessProperty); }
            set { SetValue(IndicatorThicknessProperty, value); }
        }

        public static readonly DependencyProperty IsShowIndicatorProperty = DependencyProperty.Register("IsShowIndicator", typeof(bool), _typeofSelf, new UIPropertyMetadata(true, PropertyChangedCallback));
        public bool IsShowIndicator
        {
            get { return (bool)GetValue(IsShowIndicatorProperty); }
            set { SetValue(IsShowIndicatorProperty, value); }
        }

        public static readonly DependencyProperty PointeCapDiameterProperty = DependencyProperty.Register("PointeCapDiameter", typeof(double), _typeofSelf, new UIPropertyMetadata(25d, PropertyChangedCallback));
        public double PointeCapDiameter
        {
            get { return (double)GetValue(PointeCapDiameterProperty); }
            set { SetValue(PointeCapDiameterProperty, value); }
        }

        public static readonly DependencyProperty ScaleFontSizeProperty = DependencyProperty.Register("ScaleFontSize", typeof(double), _typeofSelf, new UIPropertyMetadata(10d, PropertyChangedCallback));
        public double ScaleFontSize
        {
            get { return (double)GetValue(ScaleFontSizeProperty); }
            set { SetValue(ScaleFontSizeProperty, value); }
        }

        public static readonly DependencyProperty MinScaleProperty = DependencyProperty.Register("MinScale", typeof(double), _typeofSelf, new FrameworkPropertyMetadata(0.0, PropertyChangedCallback));
        public double MinScale
        {
            get { return (double)GetValue(MinScaleProperty); }
            set { SetValue(MinScaleProperty, value); }
        }

        public static readonly DependencyProperty MaxScaleProperty = DependencyProperty.Register("MaxScale", typeof(double), _typeofSelf, new FrameworkPropertyMetadata(60d, PropertyChangedCallback));
        public double MaxScale
        {
            get { return (double)GetValue(MaxScaleProperty); }
            set { SetValue(MaxScaleProperty, value); }
        }

        public static readonly DependencyProperty MajorScaleCountProperty = DependencyProperty.Register("MajorScaleCount", typeof(int), _typeofSelf, new UIPropertyMetadata(6, PropertyChangedCallback));
        public int MajorScaleCount
        {
            get { return (int)GetValue(MajorScaleCountProperty); }
            set { SetValue(MajorScaleCountProperty, value); }
        }

        public static readonly DependencyProperty MinorScaleCountProperty = DependencyProperty.Register("MinorScaleCount", typeof(int), _typeofSelf, new UIPropertyMetadata(5, PropertyChangedCallback));
        public int MinorScaleCount
        {
            get { return (int)GetValue(MinorScaleCountProperty); }
            set { SetValue(MinorScaleCountProperty, value); }
        }

        public static readonly DependencyProperty ScaleStartAngleProperty = DependencyProperty.Register("ScaleStartAngle", typeof(double), _typeofSelf, new UIPropertyMetadata(120d, PropertyChangedCallback));
        public double ScaleStartAngle
        {
            get { return (double)GetValue(ScaleStartAngleProperty); }
            set { SetValue(ScaleStartAngleProperty, value); }
        }

        public static readonly DependencyProperty ScaleSweepAngleProperty = DependencyProperty.Register("ScaleSweepAngle", typeof(double), _typeofSelf, new UIPropertyMetadata(300d, PropertyChangedCallback));
        public double ScaleSweepAngle
        {
            get { return (double)GetValue(ScaleSweepAngleProperty); }
            set { SetValue(ScaleSweepAngleProperty, value); }
        }

        public static readonly DependencyProperty IndicatorOptimalStartScaleProperty = DependencyProperty.Register("IndicatorOptimalStartScale", typeof(double), _typeofSelf, new UIPropertyMetadata(20d, PropertyChangedCallback, CoerceIndicatorOptimalValue));
        public double IndicatorOptimalStartScale
        {
            get { return (double)GetValue(IndicatorOptimalStartScaleProperty); }
            set { SetValue(IndicatorOptimalStartScaleProperty, value); }
        }

        static object CoerceIndicatorOptimalValue(DependencyObject d, object value)
        {
            var obj = d as RadialGuage;
            var curValue = (double)value;

            if (curValue < obj.MinScale)
                return obj.MinScale;
            if (curValue > obj.MaxScale)
                return obj.MaxScale;

            return curValue;
        }

        public static readonly DependencyProperty IndicatorOptimalEndScaleProperty = DependencyProperty.Register("IndicatorOptimalEndScale", typeof(double), _typeofSelf, new UIPropertyMetadata(50d, PropertyChangedCallback, CoerceIndicatorOptimalValue));
        public double IndicatorOptimalEndScale
        {
            get { return (double)GetValue(IndicatorOptimalEndScaleProperty); }
            set { SetValue(IndicatorOptimalEndScaleProperty, value); }
        }

        public static readonly DependencyProperty PointerLengthProperty = DependencyProperty.Register("PointerLength", typeof(double), _typeofSelf, new UIPropertyMetadata(50d, PropertyChangedCallback));
        public double PointerLength
        {
            get { return (double)GetValue(PointerLengthProperty); }
            set { SetValue(PointerLengthProperty, value); }
        }

        public static readonly DependencyProperty CurrentScaleProperty = DependencyProperty.Register("CurrentScale", typeof(double), _typeofSelf, new FrameworkPropertyMetadata(20d, CurrentScalePropertyChanged, CoerceCurrentScaleValue));
        public double CurrentScale
        {
            get { return (double)GetValue(CurrentScaleProperty); }
            set { SetValue(CurrentScaleProperty, value); }
        }

        static void CurrentScalePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var obj = d as RadialGuage;
            var newValue = (double)e.NewValue;
            var oldValue = (double)e.OldValue;

            if (!DoubleUtil.AreClose(oldValue, newValue))
                obj.MovePointerBetweenScaleWithAnimation(oldValue, newValue);
        }
        static object CoerceCurrentScaleValue(DependencyObject d, object value)
        {
            var obj = d as RadialGuage;

            if ((double)value < obj.MinScale)
                return obj.MinScale;

            if ((double)value > obj.MaxScale)
                return obj.MaxScale;

            return value;
        }

        static void PropertyChangedCallback(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var obj = sender as RadialGuage;
            obj.DrawUiElements();
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _layoutRoot = GetTemplateChild(PART_LayoutRoot) as Grid;
            _indicatorRoot = GetTemplateChild(PART_IndicatorRoot) as Grid;
            _pointerCap = GetTemplateChild(PART_PointerCap) as Ellipse;
            _pointer = GetTemplateChild(PART_Pointer) as Path;
            _pointerRt = GetTemplateChild(PART_PointerRT) as RotateTransform;

            DrawUiElements();

            if (IsShowIndicator)
            {
                var v = DependencyPropertyDescriptor.FromProperty(RotateTransform.AngleProperty, typeof(RotateTransform));
                v.AddValueChanged(_pointerRt, OnAnglePropertyChanged);
            }
        }

        private void OnAnglePropertyChanged(object sender, EventArgs e)
        {
            var obj = sender as RotateTransform;
            DrawIndicatorByScale(GetScaleByAngle(obj.Angle));
        }

        private void DrawUiElements()
        {
            if (_layoutRoot == null)
                return;

            _layoutRoot.Children.Clear();

            //画刻度
            DrawScale();
            //指针
            MovePointerToScale(CurrentScale);
            //指示器
            //DrawIndicator();
            DrawIndicatorByScale(CurrentScale);

        }

        //创建刻度线控件
        private Rectangle GetScaleRect(double width, double height, Brush fill)
        {
            return new Rectangle
            {
                Height = height,
                Width = width,
                Fill = fill,
                RenderTransformOrigin = new Point(0.5, 0.5),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
        }

        //创建刻度值控件
        private TextBlock GetScaleText(string text, Brush fg)
        {
            var ScaleText = new TextBlock();

            ScaleText.Text = text;
            ScaleText.FontSize = ScaleFontSize;
            ScaleText.Foreground = fg;
            //ScaleText.RenderTransformOrigin = new Point(0.5, 0.5);    //刻度值不需要旋转
            ScaleText.TextAlignment = TextAlignment.Center;
            ScaleText.VerticalAlignment = VerticalAlignment.Center;
            ScaleText.HorizontalAlignment = HorizontalAlignment.Center;

            return ScaleText;
        }

        /// <summary>
        /// 计算刻度线及刻度值的旋转或偏移位置
        /// </summary> 
        /// <param name="uiElementAngle">当前元素所在角度</param>
        /// <param name="uiElementRadius">当前元素所在半径</param>
        /// <param name="isNeedRotate">是否需要旋转</param>
        private TransformGroup GetUiElementTransform(double uiElementAngle, double uiElementRadius, bool isNeedRotate)
        {
            var transformGroup = new TransformGroup();

            if (isNeedRotate)
            {
                //计算旋转角度
                var rotateTransform = new RotateTransform();
                rotateTransform.Angle = uiElementAngle;

                transformGroup.Children.Add(rotateTransform);
            }

            //计算偏移
            var translateTransform = new TranslateTransform();
            var pos = GetSpecifiedPosByAngle(uiElementAngle, uiElementRadius, true);
            translateTransform.X = pos.X;
            translateTransform.Y = pos.Y;

            transformGroup.Children.Add(translateTransform);

            return transformGroup;
        }

        private void DrawScale()
        {
            if (_layoutRoot == null || _pointerCap == null || _pointer == null)
                return;

            //画主刻度
            for (var majorScaleIndex = 0; majorScaleIndex <= MajorScaleCount; majorScaleIndex++)
            {
                //刻度线
                var majorScaleRect = GetScaleRect(10, 3, Foreground);
                //刻度值
                var scaleText = majorScaleIndex == MajorScaleCount ? MaxScale : MinScale + majorScaleIndex * Math.Round((MaxScale - MinScale) / MajorScaleCount);
                var scaleTextBlock = GetScaleText(scaleText.ToString(), Foreground);

                //计算位置
                var majorScaleAngle = majorScaleIndex == MajorScaleCount ? ScaleStartAngle + ScaleSweepAngle : ScaleStartAngle + majorScaleIndex * (ScaleSweepAngle / MajorScaleCount);
                majorScaleRect.RenderTransform = GetUiElementTransform(majorScaleAngle, ScaleRadius, true);
                scaleTextBlock.RenderTransform = GetUiElementTransform(majorScaleAngle, ScaleTextRadius, false);

                _layoutRoot.Children.Insert(0, majorScaleRect);
                _layoutRoot.Children.Insert(0, scaleTextBlock);

                //最后一个主刻度后面没有次刻度线
                if (majorScaleIndex == MajorScaleCount)
                    break;

                //画次刻度线
                for (var minorScaleIndex = 1; minorScaleIndex < MinorScaleCount; minorScaleIndex++)
                {
                    //创建刻度控件
                    var minorScaleRect = GetScaleRect(3, 1, Foreground);

                    //计算位置
                    var minorScaleAngle = majorScaleAngle + minorScaleIndex * (ScaleSweepAngle / MajorScaleCount / MinorScaleCount);
                    minorScaleRect.RenderTransform = GetUiElementTransform(minorScaleAngle, ScaleRadius, true);

                    _layoutRoot.Children.Insert(0, minorScaleRect);
                }
            }

            _layoutRoot.Children.Add(_pointer);
            _layoutRoot.Children.Add(_pointerCap);
        }

        //移动到指定角度
        private void MovePointerToAngle(double angleValue)
        {
            if (_pointer != null)
            {
                var transformGroup = _pointer.RenderTransform as TransformGroup;
                var rotateTransform = transformGroup.Children[0] as RotateTransform;
                rotateTransform.Angle = angleValue;
            }
        }

        //移动到指定刻度
        private void MovePointerToScale(double scaleValue)
        {
            if (_pointer != null)
            {
                MovePointerToAngle(GetAngleByScale(scaleValue));
            }
        }

        //刻度之间的移动
        private void MovePointerBetweenScaleWithAnimation(double oldScale, double newScale)
        {
            if (_pointer != null && !DoubleUtil.AreClose(oldScale, newScale))
            {
                MovePointerBetweenAngleWithAnimation(GetAngleByScale(oldScale), GetAngleByScale(newScale));
            }
        }

        //角度之间的移动
        private void MovePointerBetweenAngleWithAnimation(double oldAngle, double newAngle)
        {
            if (_pointer != null && !DoubleUtil.AreClose(oldAngle, newAngle))
            {
                var da = new DoubleAnimation
                {
                    From = oldAngle,
                    To = newAngle,
                    Duration = new Duration(TimeSpan.FromMilliseconds(Math.Abs(oldAngle - newAngle) * 6))
                };

                var sb = new Storyboard();
                sb.Completed += sb_Completed;
                sb.Children.Add(da);
                Storyboard.SetTarget(da, _pointer);
                Storyboard.SetTargetProperty(da, new PropertyPath("(Path.RenderTransform).(TransformGroup.Children)[0].(RotateTransform.Angle)"));

                sb.Begin();
            }
        }

        void sb_Completed(object sender, EventArgs e)
        {
        }

        private void DrawIndicatorByScale(double currentScale)
        {
            if (IsShowIndicator == false)
                return;

            _indicatorRoot.Children.Clear();

            var belowRangeStartOutPoint = GetSpecifiedPosByAngle(ScaleStartAngle, IndicatorRadius + IndicatorThickness, false);
            var belowRangeStartInPoint = GetSpecifiedPosByAngle(ScaleStartAngle, IndicatorRadius, false);

            var currentScaleOutPoint = GetSpecifiedPosByScale(currentScale, IndicatorRadius + IndicatorThickness, false);
            var currentScaleInPoint = GetSpecifiedPosByScale(currentScale, IndicatorRadius, false);

            //值处于第一段的范围内，则只画起始值到当前值的弧度
            if (currentScale <= IndicatorOptimalStartScale)
            {
                var isLargeArcCurrent = GetAngleByScale(currentScale) - ScaleStartAngle > 180d;
                DrawIndicatorSegment(belowRangeStartOutPoint, currentScaleOutPoint, currentScaleInPoint, belowRangeStartInPoint, isLargeArcCurrent, Brushes.Yellow);

                return;
            }

            var optimalRangeStartOutPoint = GetSpecifiedPosByScale(IndicatorOptimalStartScale, IndicatorRadius + IndicatorThickness, false);
            var optimalRangeStartInPoint = GetSpecifiedPosByScale(IndicatorOptimalStartScale, IndicatorRadius, false);

            //画第一段
            var isLargeArcOne = GetAngleByScale(IndicatorOptimalStartScale) - ScaleStartAngle > 180d;
            DrawIndicatorSegment(belowRangeStartOutPoint, optimalRangeStartOutPoint, optimalRangeStartInPoint, belowRangeStartInPoint, isLargeArcOne, Brushes.Yellow);

            //值处于第二段的范围内，则画第一段及第二段起始值到当前值的弧度
            if (currentScale <= IndicatorOptimalEndScale)
            {
                //画第二段
                var isLargeArcCurrent = GetAngleByScale(currentScale) - GetAngleByScale(IndicatorOptimalStartScale) > 180d;
                DrawIndicatorSegment(optimalRangeStartOutPoint, currentScaleOutPoint, currentScaleInPoint, optimalRangeStartInPoint, isLargeArcCurrent, Brushes.Green);

                return;
            }

            //值处于第三段的范围内，则画第一段，第二段及第三段起始值到当前值的弧度
            var aboveRangeStartOutPoint = GetSpecifiedPosByScale(IndicatorOptimalEndScale, IndicatorRadius + IndicatorThickness, false);
            var aboveRangeStartInPoint = GetSpecifiedPosByScale(IndicatorOptimalEndScale, IndicatorRadius, false);
            var isLargeArcTwo = GetAngleByScale(IndicatorOptimalEndScale) - GetAngleByScale(IndicatorOptimalStartScale) > 180d;

            //画第二段
            DrawIndicatorSegment(optimalRangeStartOutPoint, aboveRangeStartOutPoint, aboveRangeStartInPoint, optimalRangeStartInPoint, isLargeArcTwo, Brushes.Green);

            //画第三段
            var isLargeArcThree = GetAngleByScale(currentScale) - GetAngleByScale(IndicatorOptimalEndScale) > 180d;
            DrawIndicatorSegment(aboveRangeStartOutPoint, currentScaleOutPoint, currentScaleInPoint, aboveRangeStartInPoint, isLargeArcThree, Brushes.Red);
        }

        private void DrawIndicator()
        {
            if (IsShowIndicator == false)
                return;

            _indicatorRoot.Children.Clear();

            var belowRangeStartOutPoint = GetSpecifiedPosByAngle(ScaleStartAngle, IndicatorRadius + IndicatorThickness, false);
            var belowRangeStartInPoint = GetSpecifiedPosByAngle(ScaleStartAngle, IndicatorRadius, false);

            var optimalRangeStartOutPoint = GetSpecifiedPosByScale(IndicatorOptimalStartScale, IndicatorRadius + IndicatorThickness, false);
            var optimalRangeStartInPoint = GetSpecifiedPosByScale(IndicatorOptimalStartScale, IndicatorRadius, false);
            var isLargeArcOne = GetAngleByScale(IndicatorOptimalStartScale) - ScaleStartAngle > 180d;

            //画第一段
            DrawIndicatorSegment(belowRangeStartOutPoint, optimalRangeStartOutPoint, optimalRangeStartInPoint, belowRangeStartInPoint, isLargeArcOne, Brushes.Yellow);

            var aboveRangeStartOutPoint = GetSpecifiedPosByScale(IndicatorOptimalEndScale, IndicatorRadius + IndicatorThickness, false);
            var aboveRangeStartInPoint = GetSpecifiedPosByScale(IndicatorOptimalEndScale, IndicatorRadius, false);
            var isLargeArcTwo = GetAngleByScale(IndicatorOptimalEndScale) - GetAngleByScale(IndicatorOptimalStartScale) > 180d;

            //画第二段
            DrawIndicatorSegment(optimalRangeStartOutPoint, aboveRangeStartOutPoint, aboveRangeStartInPoint, optimalRangeStartInPoint, isLargeArcTwo, Brushes.Green);

            var aboveRangeEndOutPoint = GetSpecifiedPosByAngle(ScaleStartAngle + ScaleSweepAngle, IndicatorRadius + IndicatorThickness, false);
            var aboveRangeEndInPoint = GetSpecifiedPosByAngle(ScaleStartAngle + ScaleSweepAngle, IndicatorRadius, false);
            var isLargeArcThree = ScaleStartAngle + ScaleSweepAngle - GetAngleByScale(IndicatorOptimalEndScale) > 180d;

            //画第三段
            DrawIndicatorSegment(aboveRangeStartOutPoint, aboveRangeEndOutPoint, aboveRangeEndInPoint, aboveRangeStartInPoint, isLargeArcThree, Brushes.Red);
        }

        private void DrawIndicatorSegment(Point out1Point, Point out2Point, Point in2Point, Point in1Point, bool isLargeArc, Brush fill)
        {
            if (_layoutRoot == null)
                return;

            var outRadius = IndicatorRadius + IndicatorThickness;
            var inRadius = IndicatorRadius;

            var segmentCollection = new PathSegmentCollection();

            var pathFigure = new PathFigure()
            {
                //确定起点（外弧线左侧点）
                StartPoint = out1Point,
                Segments = segmentCollection,
                IsClosed = true
            };

            //连到外弧线右侧点
            segmentCollection.Add(new ArcSegment()
            {
                Size = new Size(outRadius, outRadius),
                Point = out2Point,
                SweepDirection = SweepDirection.Clockwise,
                IsLargeArc = isLargeArc
            });

            //连到内弧线右侧点
            segmentCollection.Add(new LineSegment() { Point = in2Point });

            //连到内弧线左侧点
            segmentCollection.Add(new ArcSegment()
            {
                Size = new Size(inRadius, inRadius),
                Point = in1Point,
                SweepDirection = SweepDirection.Counterclockwise,
                IsLargeArc = isLargeArc
            });

            var indicatorPath = new Path()
            {
                RenderTransformOrigin = new Point(0.5, 0.5),
                Fill = fill,
                Data = new PathGeometry()
                {
                    Figures = new PathFigureCollection() { pathFigure }
                }
            };

            _indicatorRoot.Children.Insert(0, indicatorPath);

        }

        /// <summary>
        /// 根据刻度值获取角度值
        /// </summary>
        /// <param name="scale">刻度值</param>
        private double GetAngleByScale(double scale)
        {
            var anglePerScale = ScaleSweepAngle / (MaxScale - MinScale);

            var moveScaleValue = Math.Abs(MinScale) + scale;
            var moveAngleValue = ((double)(moveScaleValue * anglePerScale));

            return moveAngleValue + ScaleStartAngle;
        }

        private double GetScaleByAngle(double angle)
        {
            var scalePerAngle = (MaxScale - MinScale) / ScaleSweepAngle;

            var moveAngleValue = angle - ScaleStartAngle;
            var moveScaleValue = moveAngleValue * scalePerAngle;

            return MinScale + moveScaleValue;
        }

        /// <summary>
        /// 根据刻度值获取指定位置
        /// </summary>
        /// <param name="posScale">刻度</param>
        /// <param name="posRadius">半径</param>
        /// <param name="isRelativeCenter">是否相对于中心点，否则相对于左上角</param>
        private Point GetSpecifiedPosByScale(double posScale, double posRadius, bool isRelativeCenter)
        {
            return GetSpecifiedPosByAngle(GetAngleByScale(posScale), posRadius, isRelativeCenter);
        }

        /// <summary>
        /// 根据角度值获取指定位置
        /// </summary>
        /// <param name="posAngle">角度</param>
        /// <param name="posRadius">半径</param>
        /// <param name="isRelativeCenter">是否相对于中心点，否则相对于左上角</param>
        private Point GetSpecifiedPosByAngle(double posAngle, double posRadius, bool isRelativeCenter)
        {
            var angleRadian = (posAngle * Math.PI) / 180;   //计算得到该角度的弧度
            var pos = new Point(posRadius * Math.Cos(angleRadian), posRadius * Math.Sin(angleRadian));

            if (!isRelativeCenter)
            {
                pos.X += Radius;
                pos.Y += Radius;
            }

            return pos;
        }
    }
}
