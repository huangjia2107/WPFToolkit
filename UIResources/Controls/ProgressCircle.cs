using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using UIResources.Helps;

namespace UIResources.Controls
{
    public class ProgressCircle : FrameworkElement
    {
        private static readonly Type _typeofSelf = typeof(ProgressCircle);

        private readonly DrawingGroup _drawingGroup = null;
        private readonly CombinedGeometry _fgCombinedGeometry = null;


        private CornerRadius _tempCornerRadius;
        private double _tempCircleThickness;
        private double _tempProgressValue;

        private Pen _drawPen = null;
        private SolidColorBrush _backgroundBrush = null;
        private SolidColorBrush _foregroundBrush = null;

        //Background
        private StreamGeometry _outerBgStreamGeometry = null;
        private StreamGeometry _innerBgStreamGeometry = null;

        //Foreground
        private StreamGeometry _outerFgStreamGeometry = null;
        private StreamGeometry _innerFgStreamGeometry = null;

        //Corners
        private CornerInfo _topLeftCorner;
        private CornerInfo _topRightCorner;
        private CornerInfo _bottomLeftCorner;
        private CornerInfo _bottomRightCorner;

        public ProgressCircle()
        {
            _drawingGroup = new DrawingGroup();

            _outerFgStreamGeometry = new StreamGeometry();
            _innerFgStreamGeometry = new StreamGeometry();

            _fgCombinedGeometry = new CombinedGeometry(GeometryCombineMode.Xor, _outerFgStreamGeometry, _innerFgStreamGeometry);

            _drawPen = new Pen();
            _drawPen.Freeze();

            _backgroundBrush = new SolidColorBrush(Colors.LightGray);
            _foregroundBrush = new SolidColorBrush(Colors.Red);
        }

        #region Event

        public static readonly RoutedEvent ValueChangedEvent =
            EventManager.RegisterRoutedEvent("ValueChanged", RoutingStrategy.Bubble, typeof(RoutedPropertyChangedEventHandler<double>), _typeofSelf);
        [Category("Behavior")]
        public event RoutedPropertyChangedEventHandler<double> ValueChanged
        {
            add { AddHandler(ValueChangedEvent, value); }
            remove { RemoveHandler(ValueChangedEvent, value); }
        }

        #endregion

        #region Properties    

        public static readonly DependencyProperty MinimumProperty = DependencyProperty.Register("Minimum", typeof(double), _typeofSelf,
                        new FrameworkPropertyMetadata(0.0d, OnMinimumChanged), IsValidDoubleValue);
        [Bindable(true), Category("Behavior")]
        public double Minimum
        {
            get { return (double)GetValue(MinimumProperty); }
            set { SetValue(MinimumProperty, value); }
        }

        private static void OnMinimumChanged(DependencyObject dp, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = (ProgressCircle)dp;

            ctrl.CoerceValue(MaximumProperty);
            ctrl.CoerceValue(ValueProperty);

            ctrl.DrawForegroundCircle(ctrl._tempProgressValue);
        }

        public static readonly DependencyProperty MaximumProperty = DependencyProperty.Register("Maximum", typeof(double), _typeofSelf,
                       new FrameworkPropertyMetadata(100.0d, OnMaximumChanged, CoerceMaximum), IsValidDoubleValue);
        [Bindable(true), Category("Behavior")]
        public double Maximum
        {
            get { return (double)GetValue(MaximumProperty); }
            set { SetValue(MaximumProperty, value); }
        }

        private static object CoerceMaximum(DependencyObject dp, object value)
        {
            var ctrl = (ProgressCircle)dp;
            double min = ctrl.Minimum;

            if ((double)value < min)
                return min;

            return value;
        }

        private static void OnMaximumChanged(DependencyObject dp, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = (ProgressCircle)dp;

            ctrl.CoerceValue(ValueProperty);
            ctrl.DrawForegroundCircle(ctrl._tempProgressValue);
        }

        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(double), _typeofSelf,
            new FrameworkPropertyMetadata(0.0d,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault | FrameworkPropertyMetadataOptions.Journal,
                OnValueChanged, CoerceValue), IsValidDoubleValue);
        [Bindable(true), Category("Behavior")]
        public double Value
        {
            get { return (double)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        private static object CoerceValue(DependencyObject dp, object value)
        {
            var ctrl = (ProgressCircle)dp;
            double min = ctrl.Minimum;
            double v = (double)value;
            if (v < min)
                return min;

            double max = ctrl.Maximum;
            if (v > max)
                return max;

            return value;
        }

        private static void OnValueChanged(DependencyObject dp, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = (ProgressCircle)dp;

            ctrl._tempProgressValue = (double)e.NewValue;

            ctrl.DrawForegroundCircle((double)e.NewValue);
            ctrl.OnValueChanged((double)e.OldValue, (double)e.NewValue);
        }

        protected virtual void OnValueChanged(double oldValue, double newValue)
        {
            var args = new RoutedPropertyChangedEventArgs<double>(oldValue, newValue);
            args.RoutedEvent = ValueChangedEvent;
            RaiseEvent(args);
        }

        public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.Register("CornerRadius", typeof(CornerRadius), _typeofSelf,
            new FrameworkPropertyMetadata(new CornerRadius(), FrameworkPropertyMetadataOptions.AffectsRender, OnCornerRadiusChanged), IsCornerRadiusValid);
        public CornerRadius CornerRadius
        {
            get { return (CornerRadius)GetValue(CornerRadiusProperty); }
            set { SetValue(CornerRadiusProperty, value); }
        }
        static void OnCornerRadiusChanged(DependencyObject dp, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = (ProgressCircle)dp;
            ctrl._tempCornerRadius = (CornerRadius)e.NewValue;
        }

        static bool IsCornerRadiusValid(object value)
        {
            var t = (CornerRadius)value;
            return t.IsValid(false, false, false, false);
        }

        public static readonly DependencyProperty CircleThicknessProperty = DependencyProperty.Register("CircleThickness", typeof(double), _typeofSelf,
           new FrameworkPropertyMetadata(5d, FrameworkPropertyMetadataOptions.AffectsRender, OnCircleThickness, CoerceCircleThickness), IsValidDoubleValue);
        public double CircleThickness
        {
            get { return (double)GetValue(CircleThicknessProperty); }
            set { SetValue(CircleThicknessProperty, value); }
        }
        static object CoerceCircleThickness(DependencyObject d, object value)
        {
            return Math.Max(0, (double)value);
        }

        static void OnCircleThickness(DependencyObject dp, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = (ProgressCircle)dp;
            ctrl._tempCircleThickness = (double)e.NewValue;
        }

        public static readonly DependencyProperty ForegroundProperty = DependencyProperty.Register("Foreground", typeof(Color), _typeofSelf,
            new FrameworkPropertyMetadata(Colors.Red, OnForegroundChanged));
        public Color Foreground
        {
            get { return (Color)GetValue(ForegroundProperty); }
            set { SetValue(ForegroundProperty, value); }
        }

        private static void OnForegroundChanged(DependencyObject dp, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = (ProgressCircle)dp;
            ctrl._foregroundBrush.Color = (Color)e.NewValue;
        }

        public static readonly DependencyProperty BackgroundProperty = DependencyProperty.Register("Background", typeof(Color), _typeofSelf,
            new FrameworkPropertyMetadata(Colors.LightGray, OnBackgroundChanged));
        public Color Background
        {
            get { return (Color)GetValue(BackgroundProperty); }
            set { SetValue(BackgroundProperty, value); }
        }

        private static void OnBackgroundChanged(DependencyObject dp, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = (ProgressCircle)dp;
            ctrl._backgroundBrush.Color = (Color)e.NewValue;
        }

        #endregion

        #region Override

        protected override void OnRender(DrawingContext drawingContext)
        {
            using (var dc = _drawingGroup.Open())
            {
                _tempCircleThickness = CoerceCircleThickness(CircleThickness, this.ActualWidth, this.ActualHeight);
                _tempCornerRadius = CornerRadius.Coerce(Math.Max(0, this.ActualWidth - 2 * _tempCircleThickness), Math.Max(0, this.ActualHeight - 2 * _tempCircleThickness));

                InitAllCornerInfo();

                DrawBackgroundCircle(dc);

                DrawForegroundCircle(_tempProgressValue);
                dc.DrawGeometry(_foregroundBrush, _drawPen, _fgCombinedGeometry);
            }

            drawingContext.DrawDrawing(_drawingGroup);
        }

        #endregion

        #region General Method   

        private static bool IsValidDoubleValue(object value)
        {
            double d = (double)value;
            return !(DoubleUtil.IsNaN(d) || double.IsInfinity(d));
        }

        private double CoerceCircleThickness(double circleThickness, double width, double height)
        {
            var minWidthOrHeight = Math.Min(this.ActualWidth, this.ActualHeight);

            return DoubleUtil.LessThan(minWidthOrHeight, 2 * circleThickness)
                ? minWidthOrHeight / 2
                : circleThickness;
        }

        #endregion

        #region Draw Method

        private void BeginFigure(StreamGeometryContext sgc, Point startPoint, bool isFilled = true/* is filled */, bool isClosed = true/* is closed */)
        {
            sgc.BeginFigure(startPoint, isFilled, isClosed);
        }

        private void LineTo(StreamGeometryContext sgc, Point point, bool isStroked = true/* is stroked */, bool isSmoothJoin = false /* is smooth join */)
        {
            sgc.LineTo(point, isStroked, isSmoothJoin);
        }

        private void ArcTo(StreamGeometryContext sgc, Point point, Size size, double rotationAngle = 90, bool isLargeArc = false,
            SweepDirection sweepDirection = SweepDirection.Clockwise, bool isStroked = true/* is stroked */, bool isSmoothJoin = false /* is smooth join */)
        {
            sgc.ArcTo(point, size, rotationAngle, isLargeArc, sweepDirection, isStroked, isSmoothJoin);
        }

        private double GetCircumference()
        {
            //ArcLength
            var arclength = _topLeftCorner.OuterCorner.ArcLength + _topRightCorner.OuterCorner.ArcLength +
                            _bottomLeftCorner.OuterCorner.ArcLength + _bottomRightCorner.OuterCorner.ArcLength;
            var allRadius = (_topLeftCorner.OuterCorner.Radius + _topRightCorner.OuterCorner.Radius +
                          _bottomLeftCorner.OuterCorner.Radius + _bottomRightCorner.OuterCorner.Radius) * 2;

            return (ActualWidth + ActualHeight) * 2 + arclength - allRadius;
        }

        private void InitAllCornerInfo()
        {
            _topLeftCorner = new CornerInfo(CornerPos.TopLeft, _tempCornerRadius.TopLeft, _tempCircleThickness, this.ActualWidth, this.ActualHeight);
            _topRightCorner = new CornerInfo(CornerPos.TopRight, _tempCornerRadius.TopRight, _tempCircleThickness, this.ActualWidth, this.ActualHeight);
            _bottomLeftCorner = new CornerInfo(CornerPos.BottomLeft, _tempCornerRadius.BottomLeft, _tempCircleThickness, this.ActualWidth, this.ActualHeight);
            _bottomRightCorner = new CornerInfo(CornerPos.BottomRight, _tempCornerRadius.BottomRight, _tempCircleThickness, this.ActualWidth, this.ActualHeight);
        }

        private Point GetArcSpecialPoint(CornerPos cornerPos, double availableAngle, Corner corner)
        {
            var d1 = Math.Sin(availableAngle * Math.PI / 180) * corner.Radius;
            var d2 = (1 - Math.Cos(availableAngle * Math.PI / 180)) * corner.Radius;

            Point result = new Point();

            switch (cornerPos)
            {
                case CornerPos.TopLeft:
                    result = new Point(corner.ArcFromPoint.X + d2, corner.ArcFromPoint.Y - d1);
                    break;

                case CornerPos.TopRight:
                    result = new Point(corner.ArcFromPoint.X + d1, corner.ArcFromPoint.Y + d2);
                    break;

                case CornerPos.BottomRight:
                    result = new Point(corner.ArcFromPoint.X - d2, corner.ArcFromPoint.Y + d1);
                    break;
                case CornerPos.BottomLeft:
                    result = new Point(corner.ArcFromPoint.X - d1, corner.ArcFromPoint.Y - d2);
                    break;

            }

            return result;
        }

        private bool DrawTopRightProgress(StreamGeometryContext octx, StreamGeometryContext ictx, double drawLength, ref double availableLength)
        {
            //Top Right
            if (_topRightCorner.IsRightAngle)
            {
                if (DoubleUtil.GreaterThan(drawLength,
                    availableLength + ActualWidth - _topLeftCorner.OuterCorner.Radius))
                {
                    LineTo(octx, _topRightCorner.OuterCorner.RightAnglePoint);
                    LineTo(ictx, _topRightCorner.InnerCorner.RightAnglePoint);

                    availableLength += (ActualWidth - _topLeftCorner.OuterCorner.Radius);
                    //完整 上直边
                    return true;
                }
                else
                {
                    var innerEndPoint = new Point(Math.Min(_topRightCorner.InnerCorner.RightAnglePoint.X, drawLength + _topLeftCorner.OuterCorner.Radius), _tempCircleThickness);

                    LineTo(octx, new Point(drawLength + _topLeftCorner.OuterCorner.Radius, 0));
                    LineTo(octx, innerEndPoint);

                    LineTo(ictx, innerEndPoint);
                    //不完整上直边
                    return false;
                }
            }
            else
            {
                if (DoubleUtil.GreaterThan(drawLength,
                    availableLength + ActualWidth - _topLeftCorner.OuterCorner.Radius - _topRightCorner.OuterCorner.Radius))
                {
                    LineTo(octx, _topRightCorner.OuterCorner.ArcFromPoint);
                    LineTo(ictx, _topRightCorner.InnerCorner.ArcFromPoint);

                    availableLength += (ActualWidth - _topLeftCorner.OuterCorner.Radius - _topRightCorner.OuterCorner.Radius);
                    //完整 上直边

                    if (DoubleUtil.GreaterThan(drawLength,
                        availableLength + _topRightCorner.OuterCorner.ArcLength))
                    {
                        ArcTo(octx, _topRightCorner.OuterCorner.ArcToPoint, new Size(_topRightCorner.OuterCorner.Radius, _topRightCorner.OuterCorner.Radius));
                        ArcTo(ictx, _topRightCorner.InnerCorner.ArcToPoint, new Size(_topRightCorner.InnerCorner.Radius, _topRightCorner.InnerCorner.Radius));

                        availableLength += _topRightCorner.OuterCorner.ArcLength;
                        //完整 右上弧
                        return true;
                    }
                    else
                    {
                        var curOutterArcLength = drawLength - availableLength;
                        var curAngle = curOutterArcLength * 90 / _topRightCorner.OuterCorner.ArcLength;

                        var outterEndPoint = GetArcSpecialPoint(CornerPos.TopRight, curAngle, _topRightCorner.OuterCorner);
                        var innerEndPoint = GetArcSpecialPoint(CornerPos.TopRight, curAngle, _topRightCorner.InnerCorner);

                        ArcTo(octx, outterEndPoint, new Size(_topRightCorner.OuterCorner.Radius, _topRightCorner.OuterCorner.Radius), curAngle);
                        LineTo(octx, innerEndPoint);

                        ArcTo(ictx, innerEndPoint, new Size(_topRightCorner.InnerCorner.Radius, _topRightCorner.InnerCorner.Radius), curAngle);
                        //不完整 右上弧
                        return false;
                    }
                }
                else
                {
                    LineTo(octx, new Point(drawLength + _topLeftCorner.OuterCorner.Radius, 0));
                    LineTo(octx, new Point(drawLength + _topLeftCorner.OuterCorner.Radius, _tempCircleThickness));

                    LineTo(ictx, new Point(drawLength + _topLeftCorner.OuterCorner.Radius, _tempCircleThickness));
                    //不完整 上直边
                    return false;
                }
            }
        }

        private bool DrawBottomRightProgress(StreamGeometryContext octx, StreamGeometryContext ictx, double drawLength, ref double availableLength)
        {
            if (_bottomRightCorner.IsRightAngle)
            {
                if (DoubleUtil.GreaterThan(drawLength,
                    availableLength + ActualHeight - _topRightCorner.OuterCorner.Radius))
                {
                    LineTo(octx, _bottomRightCorner.OuterCorner.RightAnglePoint);
                    LineTo(ictx, _bottomRightCorner.InnerCorner.RightAnglePoint);

                    availableLength += (ActualHeight - _topRightCorner.OuterCorner.Radius);
                    //完整 右直边
                    return true;
                }
                else
                {
                    var outterYOffset = drawLength - availableLength + _topRightCorner.OuterCorner.Radius;
                    var innerEndPoint = new Point(ActualWidth - _tempCircleThickness, Math.Min(_bottomRightCorner.InnerCorner.RightAnglePoint.Y, Math.Max(_topRightCorner.InnerCorner.RightAnglePoint.Y, outterYOffset)));

                    LineTo(octx, new Point(ActualWidth, outterYOffset));
                    LineTo(octx, innerEndPoint);

                    if (_topRightCorner.IsRightAngle)
                    {
                        LineTo(ictx, innerEndPoint);
                    }
                    else
                    {
                        LineTo(ictx, innerEndPoint);
                    }

                    //不完整 右直边
                    return false;
                }
            }
            else
            {
                if (DoubleUtil.GreaterThan(drawLength,
                    availableLength + ActualHeight - _topRightCorner.OuterCorner.Radius - _bottomRightCorner.OuterCorner.Radius))
                {
                    LineTo(octx, _bottomRightCorner.OuterCorner.ArcFromPoint);
                    LineTo(ictx, _bottomRightCorner.InnerCorner.ArcFromPoint);

                    availableLength += (ActualHeight - _topRightCorner.OuterCorner.Radius - _bottomRightCorner.OuterCorner.Radius);
                    //完整 右直边

                    if (DoubleUtil.GreaterThan(drawLength,
                        availableLength + _bottomRightCorner.OuterCorner.ArcLength))
                    {
                        ArcTo(octx, _bottomRightCorner.OuterCorner.ArcToPoint, new Size(_bottomRightCorner.OuterCorner.Radius, _bottomRightCorner.OuterCorner.Radius));
                        ArcTo(ictx, _bottomRightCorner.InnerCorner.ArcToPoint, new Size(_bottomRightCorner.InnerCorner.Radius, _bottomRightCorner.InnerCorner.Radius));

                        availableLength += _bottomRightCorner.OuterCorner.ArcLength;
                        //完整 右下弧
                        return true;
                    }
                    else
                    {
                        var curOutterArcLength = drawLength - availableLength;
                        var curAngle = curOutterArcLength * 90 / _bottomRightCorner.OuterCorner.ArcLength;

                        var outterEndPoint = GetArcSpecialPoint(CornerPos.BottomRight, curAngle, _bottomRightCorner.OuterCorner);
                        var innerEndPoint = GetArcSpecialPoint(CornerPos.BottomRight, curAngle, _bottomRightCorner.InnerCorner);

                        ArcTo(octx, outterEndPoint, new Size(_bottomRightCorner.OuterCorner.Radius, _bottomRightCorner.OuterCorner.Radius), curAngle);
                        LineTo(octx, innerEndPoint);

                        ArcTo(ictx, innerEndPoint, new Size(_bottomRightCorner.InnerCorner.Radius, _bottomRightCorner.InnerCorner.Radius), curAngle);
                        //不完整 右下弧
                        return false;
                    }
                }
                else
                {
                    var outterYOffset = drawLength - availableLength + _topRightCorner.OuterCorner.Radius;
                    var innerEndPoint = new Point(ActualWidth - _tempCircleThickness, Math.Max(_topRightCorner.InnerCorner.RightAnglePoint.Y, outterYOffset));

                    LineTo(octx, new Point(ActualWidth, outterYOffset));
                    LineTo(octx, innerEndPoint);

                    if (_topRightCorner.IsRightAngle)
                    {
                        LineTo(ictx, innerEndPoint);
                    }
                    else
                    {
                        LineTo(ictx, innerEndPoint);
                    }

                    //不完整 右直边
                    return false;
                }
            }
        }

        private bool DrawBottomLeftProgress(StreamGeometryContext octx, StreamGeometryContext ictx, double drawLength, ref double availableLength)
        {
            if (_bottomLeftCorner.IsRightAngle)
            {
                if (DoubleUtil.GreaterThan(drawLength,
                    availableLength + ActualWidth - _bottomRightCorner.OuterCorner.Radius))
                {
                    LineTo(octx, _bottomLeftCorner.OuterCorner.RightAnglePoint);
                    LineTo(ictx, _bottomLeftCorner.InnerCorner.RightAnglePoint);

                    availableLength += (ActualWidth - _bottomRightCorner.OuterCorner.Radius);
                    //完整 下直边
                    return true;
                }
                else
                {
                    var outterXOffset = ActualWidth - (drawLength - availableLength + _bottomRightCorner.OuterCorner.Radius);
                    var innerEndPoint = new Point(Math.Max(_bottomLeftCorner.InnerCorner.RightAnglePoint.X, Math.Min(outterXOffset, _bottomRightCorner.InnerCorner.RightAnglePoint.X)), _bottomLeftCorner.InnerCorner.RightAnglePoint.Y);

                    LineTo(octx, new Point(outterXOffset, ActualHeight));

                    if (_bottomRightCorner.IsRightAngle)
                    {
                        LineTo(octx, innerEndPoint);
                        LineTo(ictx, innerEndPoint);
                    }
                    else
                    {
                        LineTo(octx, new Point(Math.Max(outterXOffset, _bottomLeftCorner.InnerCorner.RightAnglePoint.X), ActualHeight - _tempCircleThickness));
                        LineTo(ictx, new Point(Math.Max(outterXOffset, _bottomLeftCorner.InnerCorner.RightAnglePoint.X), _bottomLeftCorner.InnerCorner.RightAnglePoint.Y));
                    }

                    //不完整 下直边
                    return false;
                }
            }
            else
            {
                if (DoubleUtil.GreaterThan(drawLength,
                    availableLength + ActualWidth - _bottomLeftCorner.OuterCorner.Radius - _bottomRightCorner.OuterCorner.Radius))
                {
                    LineTo(octx, _bottomLeftCorner.OuterCorner.ArcFromPoint);
                    LineTo(ictx, _bottomLeftCorner.InnerCorner.ArcFromPoint);

                    availableLength += (ActualWidth - _bottomLeftCorner.OuterCorner.Radius - _bottomRightCorner.OuterCorner.Radius);
                    //完整 下直边

                    if (DoubleUtil.GreaterThan(drawLength,
                        availableLength + _bottomLeftCorner.OuterCorner.ArcLength))
                    {
                        ArcTo(octx, _bottomLeftCorner.OuterCorner.ArcToPoint, new Size(_bottomLeftCorner.OuterCorner.Radius, _bottomLeftCorner.OuterCorner.Radius));
                        ArcTo(ictx, _bottomLeftCorner.InnerCorner.ArcToPoint, new Size(_bottomLeftCorner.InnerCorner.Radius, _bottomLeftCorner.InnerCorner.Radius));

                        availableLength += _bottomLeftCorner.OuterCorner.ArcLength;
                        //完整 左下弧
                        return true;
                    }
                    else
                    {
                        var curOutterArcLength = drawLength - availableLength;
                        var curAngle = curOutterArcLength * 90 / _bottomLeftCorner.OuterCorner.ArcLength;

                        var outterEndPoint = GetArcSpecialPoint(CornerPos.BottomLeft, curAngle, _bottomLeftCorner.OuterCorner);
                        var innerEndPoint = GetArcSpecialPoint(CornerPos.BottomLeft, curAngle, _bottomLeftCorner.InnerCorner);

                        ArcTo(octx, outterEndPoint, new Size(_bottomLeftCorner.OuterCorner.Radius, _bottomLeftCorner.OuterCorner.Radius), curAngle);
                        LineTo(octx, innerEndPoint);

                        ArcTo(ictx, innerEndPoint, new Size(_bottomLeftCorner.InnerCorner.Radius, _bottomLeftCorner.InnerCorner.Radius), curAngle);
                        //不完整 左下弧
                        return false;
                    }
                }
                else
                {
                    var outterXOffset = ActualWidth - (drawLength - availableLength + _bottomRightCorner.OuterCorner.Radius);
                    var innerEndPoint = new Point(Math.Min(outterXOffset, _bottomRightCorner.InnerCorner.RightAnglePoint.X), ActualHeight - _tempCircleThickness);

                    LineTo(octx, new Point(outterXOffset, ActualHeight));
                    LineTo(octx, new Point(Math.Min(ActualWidth - _tempCircleThickness, outterXOffset), ActualHeight - _tempCircleThickness));

                    if (_bottomRightCorner.IsRightAngle)
                    {
                        LineTo(ictx, innerEndPoint);
                    }
                    else
                    {
                        LineTo(ictx, new Point(outterXOffset, ActualHeight - _tempCircleThickness));
                    }

                    //不完整 下直边
                    return false;
                }
            }
        }

        private bool DrawTopLeftProgress(StreamGeometryContext octx, StreamGeometryContext ictx, double drawLength, ref double availableLength)
        {
            if (_topLeftCorner.IsRightAngle)
            {
                if (DoubleUtil.AreClose(drawLength + _tempCircleThickness,
                    availableLength + ActualHeight - _bottomLeftCorner.OuterCorner.Radius))
                {
                    LineTo(octx, new Point(0, _topLeftCorner.InnerCorner.RightAnglePoint.Y));
                    LineTo(ictx, _topLeftCorner.InnerCorner.RightAnglePoint);

                    availableLength += (ActualHeight - _bottomLeftCorner.OuterCorner.Radius);
                    //完整 左直边
                    return true;
                }
                else
                {
                    var outterYOffset = ActualHeight - (drawLength - availableLength + _bottomLeftCorner.OuterCorner.Radius);
                    var innerEndPoint = new Point(_bottomLeftCorner.InnerCorner.RightAnglePoint.X, Math.Min(outterYOffset, _bottomLeftCorner.InnerCorner.RightAnglePoint.Y));

                    LineTo(octx, new Point(0, outterYOffset));

                    if (_bottomLeftCorner.IsRightAngle)
                    {
                        LineTo(octx, innerEndPoint);
                        LineTo(ictx, innerEndPoint);
                    }
                    else
                    {
                        LineTo(octx, new Point(_topLeftCorner.InnerCorner.RightAnglePoint.X, outterYOffset));
                        LineTo(ictx, new Point(_topLeftCorner.InnerCorner.RightAnglePoint.X, outterYOffset));
                    }

                    //不完整 左直边
                    return false;
                }
            }
            else
            {
                if (DoubleUtil.GreaterThan(drawLength,
                    availableLength + ActualHeight - _topLeftCorner.OuterCorner.Radius - _bottomLeftCorner.OuterCorner.Radius))
                {
                    LineTo(octx, _topLeftCorner.OuterCorner.ArcFromPoint);
                    LineTo(ictx, _topLeftCorner.InnerCorner.ArcFromPoint);

                    availableLength += (ActualHeight - _topLeftCorner.OuterCorner.Radius - _bottomLeftCorner.OuterCorner.Radius);
                    //完整 左直边

                    if (DoubleUtil.AreClose(drawLength,
                        availableLength + _topLeftCorner.OuterCorner.ArcLength))
                    {
                        ArcTo(octx, _topLeftCorner.OuterCorner.ArcToPoint, new Size(_topLeftCorner.OuterCorner.Radius, _topLeftCorner.OuterCorner.Radius));
                        ArcTo(ictx, _topLeftCorner.InnerCorner.ArcToPoint, new Size(_topLeftCorner.InnerCorner.Radius, _topLeftCorner.InnerCorner.Radius));

                        availableLength += _topLeftCorner.OuterCorner.ArcLength;
                        //完整 左上弧
                        return true;
                    }
                    else
                    {
                        var curOutterArcLength = drawLength - availableLength;
                        var curAngle = curOutterArcLength * 90 / _topLeftCorner.OuterCorner.ArcLength;

                        var outterEndPoint = GetArcSpecialPoint(CornerPos.TopLeft, curAngle, _topLeftCorner.OuterCorner);
                        var innerEndPoint = GetArcSpecialPoint(CornerPos.TopLeft, curAngle, _topLeftCorner.InnerCorner);

                        ArcTo(octx, outterEndPoint, new Size(_topLeftCorner.OuterCorner.Radius, _topLeftCorner.OuterCorner.Radius), curAngle);
                        LineTo(octx, innerEndPoint);

                        ArcTo(ictx, innerEndPoint, new Size(_topLeftCorner.InnerCorner.Radius, _topLeftCorner.InnerCorner.Radius), curAngle);
                        //不完整 左上弧
                        return false;
                    }
                }
                else
                {
                    var outterYOffset = ActualHeight - (drawLength - availableLength + _bottomLeftCorner.OuterCorner.Radius);
                    var innerEndPoint = new Point(_tempCircleThickness, Math.Min(outterYOffset, _bottomLeftCorner.InnerCorner.RightAnglePoint.Y));

                    LineTo(octx, new Point(0, outterYOffset));

                    if (_bottomLeftCorner.IsRightAngle)
                    {
                        LineTo(octx, innerEndPoint);
                        LineTo(ictx, innerEndPoint);
                    }
                    else
                    {
                        LineTo(octx, new Point(_tempCircleThickness, outterYOffset));
                        LineTo(ictx, new Point(_tempCircleThickness, outterYOffset));
                    }

                    //不完整 左直边
                    return false;
                }
            }
        }

        private void DrawBackgroundCircle(DrawingContext drawingContext)
        {
            _outerBgStreamGeometry = new StreamGeometry();
            _innerBgStreamGeometry = new StreamGeometry();

            using (var octx = _outerBgStreamGeometry.Open())
            {
                using (var ictx = _innerBgStreamGeometry.Open())
                {
                    //Top Left End
                    if (_topLeftCorner.IsRightAngle)
                    {
                        BeginFigure(octx, _topLeftCorner.OuterCorner.RightAnglePoint);
                        BeginFigure(ictx, _topLeftCorner.InnerCorner.RightAnglePoint);
                    }
                    else
                    {
                        BeginFigure(octx, _topLeftCorner.OuterCorner.ArcToPoint);
                        BeginFigure(ictx, _topLeftCorner.InnerCorner.ArcToPoint);
                    }

                    //Top Right
                    if (_topRightCorner.IsRightAngle)
                    {
                        LineTo(octx, _topRightCorner.OuterCorner.RightAnglePoint);
                        LineTo(ictx, _topRightCorner.InnerCorner.RightAnglePoint);
                    }
                    else
                    {
                        LineTo(octx, _topRightCorner.OuterCorner.ArcFromPoint);
                        ArcTo(octx, _topRightCorner.OuterCorner.ArcToPoint, new Size(_topRightCorner.OuterCorner.Radius, _topRightCorner.OuterCorner.Radius));

                        LineTo(ictx, _topRightCorner.InnerCorner.ArcFromPoint);
                        ArcTo(ictx, _topRightCorner.InnerCorner.ArcToPoint, new Size(_topRightCorner.InnerCorner.Radius, _topRightCorner.InnerCorner.Radius));
                    }

                    //Bottom Right
                    if (_bottomRightCorner.IsRightAngle)
                    {
                        LineTo(octx, _bottomRightCorner.OuterCorner.RightAnglePoint);
                        LineTo(ictx, _bottomRightCorner.InnerCorner.RightAnglePoint);
                    }
                    else
                    {
                        LineTo(octx, _bottomRightCorner.OuterCorner.ArcFromPoint);
                        ArcTo(octx, _bottomRightCorner.OuterCorner.ArcToPoint, new Size(_bottomRightCorner.OuterCorner.Radius, _bottomRightCorner.OuterCorner.Radius));

                        LineTo(ictx, _bottomRightCorner.InnerCorner.ArcFromPoint);
                        ArcTo(ictx, _bottomRightCorner.InnerCorner.ArcToPoint, new Size(_bottomRightCorner.InnerCorner.Radius, _bottomRightCorner.InnerCorner.Radius));
                    }

                    //Bottom Left
                    if (_bottomLeftCorner.IsRightAngle)
                    {
                        LineTo(octx, _bottomLeftCorner.OuterCorner.RightAnglePoint);
                        LineTo(ictx, _bottomLeftCorner.InnerCorner.RightAnglePoint);
                    }
                    else
                    {
                        LineTo(octx, _bottomLeftCorner.OuterCorner.ArcFromPoint);
                        ArcTo(octx, _bottomLeftCorner.OuterCorner.ArcToPoint, new Size(_bottomLeftCorner.OuterCorner.Radius, _bottomLeftCorner.OuterCorner.Radius));

                        LineTo(ictx, _bottomLeftCorner.InnerCorner.ArcFromPoint);
                        ArcTo(ictx, _bottomLeftCorner.InnerCorner.ArcToPoint, new Size(_bottomLeftCorner.InnerCorner.Radius, _bottomLeftCorner.InnerCorner.Radius));
                    }

                    //Top Left
                    if (_topLeftCorner.IsRightAngle)
                    {
                        LineTo(octx, _topLeftCorner.OuterCorner.RightAnglePoint);
                        LineTo(ictx, _topLeftCorner.InnerCorner.RightAnglePoint);
                    }
                    else
                    {
                        LineTo(octx, _topLeftCorner.OuterCorner.ArcFromPoint);
                        ArcTo(octx, _topLeftCorner.OuterCorner.ArcToPoint, new Size(_topLeftCorner.OuterCorner.Radius, _topLeftCorner.OuterCorner.Radius));

                        LineTo(ictx, _topLeftCorner.InnerCorner.ArcFromPoint);
                        ArcTo(ictx, _topLeftCorner.InnerCorner.ArcToPoint, new Size(_topLeftCorner.InnerCorner.Radius, _topLeftCorner.InnerCorner.Radius));
                    }
                }
            }

            _outerBgStreamGeometry.Freeze();
            _innerBgStreamGeometry.Freeze();

            var pathGeometry = Geometry.Combine(_outerBgStreamGeometry, _innerBgStreamGeometry,
                GeometryCombineMode.Xor, null);
            pathGeometry.Freeze();

            drawingContext.DrawGeometry(_backgroundBrush, new Pen(Brushes.Transparent, 1), pathGeometry);
        }

        private void DrawForegroundCircle(double value)
        {
            if (_outerFgStreamGeometry == null)
                _outerFgStreamGeometry = new StreamGeometry();
            else
                _outerFgStreamGeometry.Clear();

            if (_innerFgStreamGeometry == null)
                _innerFgStreamGeometry = new StreamGeometry();
            else
                _innerFgStreamGeometry.Clear();

            if (DoubleUtil.IsZero(Value))
                return;

            var drawLength = 0d;
            var availableLength = 0d;
            var circumference = GetCircumference();

            using (var octx = _outerFgStreamGeometry.Open())
            {
                using (var ictx = _innerFgStreamGeometry.Open())
                {
                    //Top Left End
                    if (_topLeftCorner.IsRightAngle)
                    {
                        drawLength = (circumference - _tempCircleThickness) * value / (Maximum - Minimum);

                        //Outter
                        BeginFigure(octx, new Point(0, _topLeftCorner.InnerCorner.RightAnglePoint.Y),
                            isClosed: false);
                        LineTo(octx, _topLeftCorner.OuterCorner.RightAnglePoint);

                        //Inner
                        if (!DoubleUtil.AreClose(drawLength, circumference - _tempCircleThickness))
                            BeginFigure(ictx, new Point(0, _topLeftCorner.InnerCorner.RightAnglePoint.Y), isClosed: false);
                        else
                            BeginFigure(ictx, _topLeftCorner.InnerCorner.RightAnglePoint, isClosed: false);

                        //availableLength = _tempCircleThickness;
                    }
                    else
                    {
                        drawLength = circumference * value / (Maximum - Minimum);

                        //Outter
                        if (!DoubleUtil.AreClose(drawLength, circumference))
                        {
                            BeginFigure(octx, _topLeftCorner.InnerCorner.ArcToPoint, isClosed: false);
                            LineTo(octx, _topLeftCorner.OuterCorner.ArcToPoint);
                        }
                        else
                            BeginFigure(octx, _topLeftCorner.OuterCorner.ArcToPoint, isClosed: false);

                        //Inner
                        BeginFigure(ictx, _topLeftCorner.InnerCorner.ArcToPoint, isClosed: false);

                        availableLength = 0;
                    }

                    while (true)
                    {
                        //Top Right
                        if (!DrawTopRightProgress(octx, ictx, drawLength, ref availableLength))
                            break;

                        //Bottom Right
                        if (!DrawBottomRightProgress(octx, ictx, drawLength, ref availableLength))
                            break;

                        if (!DrawBottomLeftProgress(octx, ictx, drawLength, ref availableLength))
                            break;

                        if (!DrawTopLeftProgress(octx, ictx, drawLength, ref availableLength))
                            break;

                        break;
                    }
                }
            }
        }

        #endregion             

        #region Struct

        enum CornerPos
        {
            TopLeft,
            TopRight,
            BottomLeft,
            BottomRight
        }

        struct CornerInfo
        {
            private CornerPos _cornerPos;
            private double _radius;
            private double _circleThickness;
            private double _width;
            private double _height;

            public bool IsRightAngle { get; private set; }
            public Corner OuterCorner { get; private set; }
            public Corner InnerCorner { get; private set; }

            public CornerInfo(CornerPos cornerPos, double radius, double circleThickness, double width, double height)
                : this()
            {
                _cornerPos = cornerPos;
                _radius = radius;
                _circleThickness = circleThickness;
                _width = width;
                _height = height;

                IsRightAngle = DoubleUtil.IsZero(_radius);
                OuterCorner = ConstructCorner(0, circleThickness);
                InnerCorner = ConstructCorner(circleThickness);
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="citcleOffset">Outter:0  Inner:_circleThickness</param>
            /// <param name="radiusOffset">Inner:_circleThickness  Inner:0</param>
            /// <returns></returns>
            private Corner ConstructCorner(double citcleOffset, double radiusOffset = 0)
            {
                var corner = new Corner();

                switch (_cornerPos)
                {
                    case CornerPos.TopLeft:
                        corner = IsRightAngle ? new Corner(new Point(citcleOffset, citcleOffset))
                            : new Corner(_radius + radiusOffset, new Point(citcleOffset, _radius + _circleThickness), new Point(_radius + _circleThickness, citcleOffset));
                        break;
                    case CornerPos.TopRight:
                        corner = IsRightAngle ? new Corner(new Point(_width - citcleOffset, citcleOffset))
                            : new Corner(_radius + radiusOffset, new Point(_width - _radius - _circleThickness, citcleOffset), new Point(_width - citcleOffset, _radius + _circleThickness));
                        break;
                    case CornerPos.BottomRight:
                        corner = IsRightAngle ? new Corner(new Point(_width - citcleOffset, _height - citcleOffset))
                            : new Corner(_radius + radiusOffset, new Point(_width - citcleOffset, _height - _radius - _circleThickness), new Point(_width - _radius - _circleThickness, _height - citcleOffset));
                        break;
                    case CornerPos.BottomLeft:
                        corner = IsRightAngle ? new Corner(new Point(citcleOffset, _height - citcleOffset))
                            : new Corner(_radius + radiusOffset, new Point(_radius + _circleThickness, _height - citcleOffset), new Point(citcleOffset, _height - _radius - _circleThickness));
                        break;
                }

                return corner;
            }
        }

        struct Corner
        {
            public double Radius { get; private set;}
            public Point ArcFromPoint { get; private set; }
            public Point ArcToPoint { get; private set; }

            public Point RightAnglePoint { get; private set; }

            private double _arcLength;
            public double ArcLength
            {
                get { return _arcLength; }
            }

            public Corner(Point rightAnglePoint)
                : this()
            {
                _arcLength = 0;

                RightAnglePoint = rightAnglePoint;
            }

            public Corner(double radius, Point arcFromPoint, Point arcToPoint)
                : this()
            {
                _arcLength = radius * Math.PI / 2;

                Radius = radius;
                ArcFromPoint = arcFromPoint;
                ArcToPoint = arcToPoint;
            }
        }

        #endregion
    }
}

