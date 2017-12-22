using System;
using System.Collections.Generic;
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

        private CornerRadius _tempCornerRadius;
        private double _tempCircleThickness;

        private StreamGeometry _outerStreamGeometryCache = null;
        private StreamGeometry _innerStreamGeometryCache = null;

        public ProgressCircle()
        {

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
            var progressCircle = dp as ProgressCircle;
            if (progressCircle != null)
                progressCircle._tempCornerRadius = (CornerRadius)e.NewValue;
        }

        static bool IsCornerRadiusValid(object value)
        {
            var t = (CornerRadius)value;
            return t.IsValid(false, false, false, false);
        }

        public static readonly DependencyProperty ForegroundProperty = DependencyProperty.Register("Foreground", typeof(Brush), _typeofSelf,
            new FrameworkPropertyMetadata(Brushes.Black, FrameworkPropertyMetadataOptions.AffectsRender));
        public Brush Foreground
        {
            get { return (Brush)GetValue(ForegroundProperty); }
            set { SetValue(ForegroundProperty, value); }
        }

        public static readonly DependencyProperty BackgroundProperty = DependencyProperty.Register("Background", typeof(Brush), _typeofSelf,
            new FrameworkPropertyMetadata(Brushes.Black, FrameworkPropertyMetadataOptions.AffectsRender));
        public Brush Background
        {
            get { return (Brush)GetValue(BackgroundProperty); }
            set { SetValue(BackgroundProperty, value); }
        }

        public static readonly DependencyProperty CircleThicknessProperty = DependencyProperty.Register("CircleThickness", typeof(double), _typeofSelf,
            new FrameworkPropertyMetadata(5d, FrameworkPropertyMetadataOptions.AffectsRender, OnCircleThickness, CoerceCircleThickness));
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
            var progressCircle = dp as ProgressCircle;
            if (progressCircle != null)
                progressCircle._tempCircleThickness = (double)e.NewValue;
        }

        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(double), _typeofSelf,
            new FrameworkPropertyMetadata(0d, FrameworkPropertyMetadataOptions.AffectsRender, null, CoerceValue));
        public double Value
        {
            get { return (double)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        static object CoerceValue(DependencyObject d, object value)
        {
            return Math.Max(0, (double)value);
        }



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

        protected override void OnRender(DrawingContext drawingContext)
        {
            _tempCircleThickness = CoerceCircleThickness(CircleThickness, this.ActualWidth, this.ActualHeight);
            _tempCornerRadius = CoerceCornerRadius(CornerRadius, Math.Max(0, this.ActualWidth - 2 * _tempCircleThickness), Math.Max(0, this.ActualHeight - 2 * _tempCircleThickness));

            ConstructAllCornerInfo();
            DrawCircle(drawingContext);

            if (DoubleUtil.IsZero(Value))
                return;

            DrawCircleWithValue(drawingContext, Value);
        }

        public double Maximum = 100;
        public double Minimum = 0;

        private StreamGeometry _outerProgressStreamGeometry = null;
        private StreamGeometry _innerProgressStreamGeometry = null;

        private double GetCircumference()
        {
            //ArcLength
            var arclength = _topLeftCorner.OutterCorner.ArcLength + _topRightCorner.OutterCorner.ArcLength +
                            _bottomLeftCorner.OutterCorner.ArcLength + _bottomRightCorner.OutterCorner.ArcLength;
            var allRadius = (_topLeftCorner.OutterCorner.Radius + _topRightCorner.OutterCorner.Radius +
                          _bottomLeftCorner.OutterCorner.Radius + _bottomRightCorner.OutterCorner.Radius) * 2;

            return (ActualWidth + ActualHeight) * 2 + arclength - allRadius;
        }

        private bool DrawTopRightProgress(StreamGeometryContext octx, StreamGeometryContext ictx, double drawLength, ref double availableLength)
        {
            //Top Right
            if (_topRightCorner.IsRightAngle)
            {
                if (DoubleUtil.GreaterThan(drawLength,
                    availableLength + ActualWidth - _topLeftCorner.OutterCorner.Radius))
                {
                    LineTo(octx, _topRightCorner.OutterCorner.RightAnglePoint);
                    LineTo(ictx, _topRightCorner.InnerCorner.RightAnglePoint);

                    availableLength += (ActualWidth - _topLeftCorner.OutterCorner.Radius);
                    //完整 上直边
                    return true;
                }
                else
                {
                    var innerEndPoint = new Point(Math.Min(_topRightCorner.InnerCorner.RightAnglePoint.X, drawLength + _topLeftCorner.OutterCorner.Radius), _tempCircleThickness);

                    LineTo(octx, new Point(drawLength + _topLeftCorner.OutterCorner.Radius, 0));
                    LineTo(octx, innerEndPoint);

                    LineTo(ictx, innerEndPoint);
                    //不完整上直边
                    return false;
                }
            }
            else
            {
                if (DoubleUtil.GreaterThan(drawLength,
                    availableLength + ActualWidth - _topLeftCorner.OutterCorner.Radius - _topRightCorner.OutterCorner.Radius))
                {
                    LineTo(octx, _topRightCorner.OutterCorner.ArcFromPoint);
                    LineTo(ictx, _topRightCorner.InnerCorner.ArcFromPoint);

                    availableLength += (ActualWidth - _topLeftCorner.OutterCorner.Radius - _topRightCorner.OutterCorner.Radius);
                    //完整 上直边

                    if (DoubleUtil.GreaterThan(drawLength,
                        availableLength + _topRightCorner.OutterCorner.ArcLength))
                    {
                        ArcTo(octx, _topRightCorner.OutterCorner.ArcToPoint, new Size(_topRightCorner.OutterCorner.Radius, _topRightCorner.OutterCorner.Radius));
                        ArcTo(ictx, _topRightCorner.InnerCorner.ArcToPoint, new Size(_topRightCorner.InnerCorner.Radius, _topRightCorner.InnerCorner.Radius));

                        availableLength += _topRightCorner.OutterCorner.ArcLength;
                        //完整 右上弧
                        return true;
                    }
                    else
                    {
                        var curOutterArcLength = drawLength - availableLength;
                        var curAngle = curOutterArcLength * 90 / _topRightCorner.OutterCorner.ArcLength;

                        var outterEndPoint = GetSpecialPoint(CornerPos.TopRight, curAngle, _topRightCorner.OutterCorner);
                        var innerEndPoint = GetSpecialPoint(CornerPos.TopRight, curAngle, _topRightCorner.InnerCorner);

                        ArcTo(octx, outterEndPoint, new Size(_topRightCorner.OutterCorner.Radius, _topRightCorner.OutterCorner.Radius), curAngle);
                        LineTo(octx, innerEndPoint);

                        ArcTo(ictx, innerEndPoint, new Size(_topRightCorner.InnerCorner.Radius, _topRightCorner.InnerCorner.Radius), curAngle);
                        //不完整 右上弧
                        return false;
                    }
                }
                else
                {
                    LineTo(octx, new Point(drawLength + _topLeftCorner.OutterCorner.Radius, 0));
                    LineTo(octx, new Point(drawLength + _topLeftCorner.OutterCorner.Radius, _tempCircleThickness));

                    LineTo(ictx, new Point(drawLength + _topLeftCorner.OutterCorner.Radius, _tempCircleThickness));
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
                    availableLength + ActualHeight - _topRightCorner.OutterCorner.Radius))
                {
                    LineTo(octx, _bottomRightCorner.OutterCorner.RightAnglePoint);
                    LineTo(ictx, _bottomRightCorner.InnerCorner.RightAnglePoint);

                    availableLength += (ActualHeight - _topRightCorner.OutterCorner.Radius);
                    //完整 右直边
                    return true;
                }
                else
                {
                    var outterYOffset = drawLength - availableLength + _topRightCorner.OutterCorner.Radius;
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
                    availableLength + ActualHeight - _topRightCorner.OutterCorner.Radius - _bottomRightCorner.OutterCorner.Radius))
                {
                    LineTo(octx, _bottomRightCorner.OutterCorner.ArcFromPoint);
                    LineTo(ictx, _bottomRightCorner.InnerCorner.ArcFromPoint);

                    availableLength += (ActualHeight - _topRightCorner.OutterCorner.Radius - _bottomRightCorner.OutterCorner.Radius);
                    //完整 右直边

                    if (DoubleUtil.GreaterThan(drawLength,
                        availableLength + _bottomRightCorner.OutterCorner.ArcLength))
                    {
                        ArcTo(octx, _bottomRightCorner.OutterCorner.ArcToPoint, new Size(_bottomRightCorner.OutterCorner.Radius, _bottomRightCorner.OutterCorner.Radius));
                        ArcTo(ictx, _bottomRightCorner.InnerCorner.ArcToPoint, new Size(_bottomRightCorner.InnerCorner.Radius, _bottomRightCorner.InnerCorner.Radius));

                        availableLength += _bottomRightCorner.OutterCorner.ArcLength;
                        //完整 右下弧
                        return true;
                    }
                    else
                    {
                        var curOutterArcLength = drawLength - availableLength;
                        var curAngle = curOutterArcLength * 90 / _bottomRightCorner.OutterCorner.ArcLength;

                        var outterEndPoint = GetSpecialPoint(CornerPos.BottomRight, curAngle, _bottomRightCorner.OutterCorner);
                        var innerEndPoint = GetSpecialPoint(CornerPos.BottomRight, curAngle, _bottomRightCorner.InnerCorner);

                        ArcTo(octx, outterEndPoint, new Size(_bottomRightCorner.OutterCorner.Radius, _bottomRightCorner.OutterCorner.Radius), curAngle);
                        LineTo(octx, innerEndPoint);

                        ArcTo(ictx, innerEndPoint, new Size(_bottomRightCorner.InnerCorner.Radius, _bottomRightCorner.InnerCorner.Radius), curAngle);
                        //不完整 右下弧
                        return false;
                    }
                }
                else
                {
                    var outterYOffset = drawLength - availableLength + _topRightCorner.OutterCorner.Radius;
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
                    availableLength + ActualWidth - _bottomRightCorner.OutterCorner.Radius))
                {
                    LineTo(octx, _bottomLeftCorner.OutterCorner.RightAnglePoint);
                    LineTo(ictx, _bottomLeftCorner.InnerCorner.RightAnglePoint);

                    availableLength += (ActualWidth - _bottomRightCorner.OutterCorner.Radius);
                    //完整 下直边
                    return true;
                }
                else
                {
                    var outterXOffset = ActualWidth - (drawLength - availableLength + _bottomRightCorner.OutterCorner.Radius);
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
                    availableLength + ActualWidth - _bottomLeftCorner.OutterCorner.Radius - _bottomRightCorner.OutterCorner.Radius))
                {
                    LineTo(octx, _bottomLeftCorner.OutterCorner.ArcFromPoint);
                    LineTo(ictx, _bottomLeftCorner.InnerCorner.ArcFromPoint);

                    availableLength += (ActualWidth - _bottomLeftCorner.OutterCorner.Radius - _bottomRightCorner.OutterCorner.Radius);
                    //完整 下直边

                    if (DoubleUtil.GreaterThan(drawLength,
                        availableLength + _bottomLeftCorner.OutterCorner.ArcLength))
                    {
                        ArcTo(octx, _bottomLeftCorner.OutterCorner.ArcToPoint, new Size(_bottomLeftCorner.OutterCorner.Radius, _bottomLeftCorner.OutterCorner.Radius));
                        ArcTo(ictx, _bottomLeftCorner.InnerCorner.ArcToPoint, new Size(_bottomLeftCorner.InnerCorner.Radius, _bottomLeftCorner.InnerCorner.Radius));

                        availableLength += _bottomLeftCorner.OutterCorner.ArcLength;
                        //完整 左下弧
                        return true;
                    }
                    else
                    {
                        var curOutterArcLength = drawLength - availableLength;
                        var curAngle = curOutterArcLength * 90 / _bottomLeftCorner.OutterCorner.ArcLength;

                        var outterEndPoint = GetSpecialPoint(CornerPos.BottomLeft, curAngle, _bottomLeftCorner.OutterCorner);
                        var innerEndPoint = GetSpecialPoint(CornerPos.BottomLeft, curAngle, _bottomLeftCorner.InnerCorner);

                        ArcTo(octx, outterEndPoint, new Size(_bottomLeftCorner.OutterCorner.Radius, _bottomLeftCorner.OutterCorner.Radius), curAngle);
                        LineTo(octx, innerEndPoint);

                        ArcTo(ictx, innerEndPoint, new Size(_bottomLeftCorner.InnerCorner.Radius, _bottomLeftCorner.InnerCorner.Radius), curAngle);
                        //不完整 左下弧
                        return false;
                    }
                }
                else
                {
                    var outterXOffset = ActualWidth - (drawLength - availableLength + _bottomRightCorner.OutterCorner.Radius);
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
                    availableLength + ActualHeight - _bottomLeftCorner.OutterCorner.Radius))
                {
                    LineTo(octx, new Point(0, _topLeftCorner.InnerCorner.RightAnglePoint.Y));
                    LineTo(ictx, _topLeftCorner.InnerCorner.RightAnglePoint);

                    availableLength += (ActualHeight - _bottomLeftCorner.OutterCorner.Radius);
                    //完整 左直边
                    return true;
                }
                else
                {
                    var outterYOffset = ActualHeight - (drawLength - availableLength + _bottomLeftCorner.OutterCorner.Radius);
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
                    availableLength + ActualHeight - _topLeftCorner.OutterCorner.Radius - _bottomLeftCorner.OutterCorner.Radius))
                {
                    LineTo(octx, _topLeftCorner.OutterCorner.ArcFromPoint);
                    LineTo(ictx, _topLeftCorner.InnerCorner.ArcFromPoint);

                    availableLength += (ActualHeight - _topLeftCorner.OutterCorner.Radius - _bottomLeftCorner.OutterCorner.Radius);
                    //完整 左直边

                    if (DoubleUtil.AreClose(drawLength,
                        availableLength + _topLeftCorner.OutterCorner.ArcLength))
                    {
                        ArcTo(octx, _topLeftCorner.OutterCorner.ArcToPoint, new Size(_topLeftCorner.OutterCorner.Radius, _topLeftCorner.OutterCorner.Radius));
                        ArcTo(ictx, _topLeftCorner.InnerCorner.ArcToPoint, new Size(_topLeftCorner.InnerCorner.Radius, _topLeftCorner.InnerCorner.Radius));

                        availableLength += _topLeftCorner.OutterCorner.ArcLength;
                        //完整 左上弧
                        return true;
                    }
                    else
                    {
                        var curOutterArcLength = drawLength - availableLength;
                        var curAngle = curOutterArcLength * 90 / _topLeftCorner.OutterCorner.ArcLength;

                        var outterEndPoint = GetSpecialPoint(CornerPos.TopLeft, curAngle, _topLeftCorner.OutterCorner);
                        var innerEndPoint = GetSpecialPoint(CornerPos.TopLeft, curAngle, _topLeftCorner.InnerCorner);

                        ArcTo(octx, outterEndPoint, new Size(_topLeftCorner.OutterCorner.Radius, _topLeftCorner.OutterCorner.Radius), curAngle);
                        LineTo(octx, innerEndPoint);

                        ArcTo(ictx, innerEndPoint, new Size(_topLeftCorner.InnerCorner.Radius, _topLeftCorner.InnerCorner.Radius), curAngle);
                        //不完整 左上弧
                        return false;
                    }
                }
                else
                {
                    var outterYOffset = ActualHeight - (drawLength - availableLength + _bottomLeftCorner.OutterCorner.Radius);
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

        private void DrawCircleWithValue(DrawingContext drawingContext, double value)
        {
            var circumference = GetCircumference();

            if (_outerProgressStreamGeometry == null)
                _outerProgressStreamGeometry = new StreamGeometry();
            else
                _outerProgressStreamGeometry.Clear();

            if (_innerProgressStreamGeometry == null)
                _innerProgressStreamGeometry = new StreamGeometry();
            else
                _innerProgressStreamGeometry.Clear();

            if (DoubleUtil.IsZero(Value))
                return;

            double drawLength = 0;
            double availableLength = 0;

            using (var octx = _outerProgressStreamGeometry.Open())
            {
                using (var ictx = _innerProgressStreamGeometry.Open())
                {
                    //Top Left End
                    if (_topLeftCorner.IsRightAngle)
                    {
                        drawLength = (circumference - _tempCircleThickness) * value / (Maximum - Minimum);

                        //Outter
                        BeginFigure(octx, new Point(0, _topLeftCorner.InnerCorner.RightAnglePoint.Y),
                            isClosed: false);
                        LineTo(octx, _topRightCorner.OutterCorner.RightAnglePoint);

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
                            LineTo(octx, _topLeftCorner.OutterCorner.ArcToPoint);
                        }
                        else
                            BeginFigure(octx, _topLeftCorner.OutterCorner.ArcToPoint, isClosed: false);

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

            var pathGeometry = Geometry.Combine(_outerProgressStreamGeometry, _innerProgressStreamGeometry, GeometryCombineMode.Xor, null);
            pathGeometry.Freeze();
            drawingContext.DrawGeometry(Brushes.Red, new Pen(Brushes.Transparent, 1), pathGeometry);

            //             drawingContext.DrawGeometry(Brushes.Transparent, new Pen(Brushes.Red, 1), _outerProgressStreamGeometry);
            //             drawingContext.DrawGeometry(Brushes.Transparent, new Pen(Brushes.Blue, 1), _innerProgressStreamGeometry);
        }

        private Point GetSpecialPoint(CornerPos cornerPos, double availableAngle, Corner corner)
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

        private void DrawCircle(DrawingContext drawingContext)
        {
            _outerStreamGeometryCache = new StreamGeometry();
            _innerStreamGeometryCache = new StreamGeometry();

            using (var octx = _outerStreamGeometryCache.Open())
            {
                using (var ictx = _innerStreamGeometryCache.Open())
                {
                    //Top Left End
                    if (_topLeftCorner.IsRightAngle)
                    {
                        BeginFigure(octx, _topLeftCorner.OutterCorner.RightAnglePoint);
                        BeginFigure(ictx, _topLeftCorner.InnerCorner.RightAnglePoint);
                    }
                    else
                    {
                        BeginFigure(octx, _topLeftCorner.OutterCorner.ArcToPoint);
                        BeginFigure(ictx, _topLeftCorner.InnerCorner.ArcToPoint);
                    }

                    //Top Right
                    if (_topRightCorner.IsRightAngle)
                    {
                        LineTo(octx, _topRightCorner.OutterCorner.RightAnglePoint);
                        LineTo(ictx, _topRightCorner.InnerCorner.RightAnglePoint);
                    }
                    else
                    {
                        LineTo(octx, _topRightCorner.OutterCorner.ArcFromPoint);
                        ArcTo(octx, _topRightCorner.OutterCorner.ArcToPoint, new Size(_topRightCorner.OutterCorner.Radius, _topRightCorner.OutterCorner.Radius));

                        LineTo(ictx, _topRightCorner.InnerCorner.ArcFromPoint);
                        ArcTo(ictx, _topRightCorner.InnerCorner.ArcToPoint, new Size(_topRightCorner.InnerCorner.Radius, _topRightCorner.InnerCorner.Radius));
                    }

                    //Bottom Right
                    if (_bottomRightCorner.IsRightAngle)
                    {
                        LineTo(octx, _bottomRightCorner.OutterCorner.RightAnglePoint);
                        LineTo(ictx, _bottomRightCorner.InnerCorner.RightAnglePoint);
                    }
                    else
                    {
                        LineTo(octx, _bottomRightCorner.OutterCorner.ArcFromPoint);
                        ArcTo(octx, _bottomRightCorner.OutterCorner.ArcToPoint, new Size(_bottomRightCorner.OutterCorner.Radius, _bottomRightCorner.OutterCorner.Radius));

                        LineTo(ictx, _bottomRightCorner.InnerCorner.ArcFromPoint);
                        ArcTo(ictx, _bottomRightCorner.InnerCorner.ArcToPoint, new Size(_bottomRightCorner.InnerCorner.Radius, _bottomRightCorner.InnerCorner.Radius));
                    }

                    //Bottom Left
                    if (_bottomLeftCorner.IsRightAngle)
                    {
                        LineTo(octx, _bottomLeftCorner.OutterCorner.RightAnglePoint);
                        LineTo(ictx, _bottomLeftCorner.InnerCorner.RightAnglePoint);
                    }
                    else
                    {
                        LineTo(octx, _bottomLeftCorner.OutterCorner.ArcFromPoint);
                        ArcTo(octx, _bottomLeftCorner.OutterCorner.ArcToPoint, new Size(_bottomLeftCorner.OutterCorner.Radius, _bottomLeftCorner.OutterCorner.Radius));

                        LineTo(ictx, _bottomLeftCorner.InnerCorner.ArcFromPoint);
                        ArcTo(ictx, _bottomLeftCorner.InnerCorner.ArcToPoint, new Size(_bottomLeftCorner.InnerCorner.Radius, _bottomLeftCorner.InnerCorner.Radius));
                    }

                    //Top Left
                    if (_topLeftCorner.IsRightAngle)
                    {
                        LineTo(octx, _topLeftCorner.OutterCorner.RightAnglePoint);
                        LineTo(ictx, _topLeftCorner.InnerCorner.RightAnglePoint);
                    }
                    else
                    {
                        LineTo(octx, _topLeftCorner.OutterCorner.ArcFromPoint);
                        ArcTo(octx, _topLeftCorner.OutterCorner.ArcToPoint, new Size(_topLeftCorner.OutterCorner.Radius, _topLeftCorner.OutterCorner.Radius));

                        LineTo(ictx, _topLeftCorner.InnerCorner.ArcFromPoint);
                        ArcTo(ictx, _topLeftCorner.InnerCorner.ArcToPoint, new Size(_topLeftCorner.InnerCorner.Radius, _topLeftCorner.InnerCorner.Radius));
                    }
                }
            }

            _outerStreamGeometryCache.Freeze();
            _innerStreamGeometryCache.Freeze();

            var pathGeometry = Geometry.Combine(_outerStreamGeometryCache, _innerStreamGeometryCache,
                GeometryCombineMode.Xor, null);
            pathGeometry.Freeze();

            drawingContext.DrawGeometry(Brushes.DarkGray, new Pen(Brushes.Transparent, 1), pathGeometry);
        }

        private double CoerceCircleThickness(double circleThickness, double width, double height)
        {
            var minWidthOrHeight = Math.Min(this.ActualWidth, this.ActualHeight);

            return DoubleUtil.LessThan(minWidthOrHeight, 2 * circleThickness)
                ? minWidthOrHeight / 2
                : circleThickness;
        }

        private CornerRadius CoerceCornerRadius(CornerRadius cornerRadius, double availableWidth, double availableHeight)
        {
            double? topLeft = null;
            double? bottomLeft = null;
            if (availableHeight < cornerRadius.TopLeft + cornerRadius.BottomLeft)
            {
                topLeft = cornerRadius.TopLeft * availableHeight / (cornerRadius.TopLeft + cornerRadius.BottomLeft);
                bottomLeft = cornerRadius.BottomLeft * availableHeight / (cornerRadius.TopLeft + cornerRadius.BottomLeft);
            }

            double? topRight = null;
            double? bottomRight = null;
            if (availableHeight < cornerRadius.TopRight + cornerRadius.BottomRight)
            {
                topRight = cornerRadius.TopRight * availableHeight / (cornerRadius.TopRight + cornerRadius.BottomRight);
                bottomRight = cornerRadius.BottomRight * availableHeight / (cornerRadius.TopRight + cornerRadius.BottomRight);
            }

            if (availableWidth < cornerRadius.TopLeft + cornerRadius.TopRight)
            {
                var tl = cornerRadius.TopLeft * availableWidth / (cornerRadius.TopLeft + cornerRadius.TopRight);
                topLeft = topLeft == null ? tl : Math.Min(tl, topLeft.Value);

                var tr = cornerRadius.TopRight * availableWidth / (cornerRadius.TopLeft + cornerRadius.TopRight);
                topRight = topRight == null ? tr : Math.Min(tr, topRight.Value);
            }

            if (availableWidth < cornerRadius.BottomLeft + cornerRadius.BottomRight)
            {
                var bl = cornerRadius.BottomLeft * availableWidth / (cornerRadius.BottomLeft + cornerRadius.BottomRight);
                bottomLeft = bottomLeft == null ? bl : Math.Min(bl, bottomLeft.Value);

                var br = cornerRadius.BottomRight * availableWidth / (cornerRadius.BottomLeft + cornerRadius.BottomRight);
                bottomRight = bottomRight == null ? br : Math.Min(br, bottomRight.Value);
            }

            if (topLeft != null || topRight != null || bottomLeft != null || bottomRight != null)
                return new CornerRadius(topLeft.Value, topRight.Value, bottomRight.Value, bottomLeft.Value);

            return cornerRadius;
        }

        private CornerInfo _topLeftCorner;
        private CornerInfo _topRightCorner;
        private CornerInfo _bottomLeftCorner;
        private CornerInfo _bottomRightCorner;

        private void ConstructAllCornerInfo()
        {
            _topLeftCorner = new CornerInfo(CornerPos.TopLeft, _tempCornerRadius.TopLeft, _tempCircleThickness, this.ActualWidth, this.ActualHeight);
            _topRightCorner = new CornerInfo(CornerPos.TopRight, _tempCornerRadius.TopRight, _tempCircleThickness, this.ActualWidth, this.ActualHeight);
            _bottomLeftCorner = new CornerInfo(CornerPos.BottomLeft, _tempCornerRadius.BottomLeft, _tempCircleThickness, this.ActualWidth, this.ActualHeight);
            _bottomRightCorner = new CornerInfo(CornerPos.BottomRight, _tempCornerRadius.BottomRight, _tempCircleThickness, this.ActualWidth, this.ActualHeight);
        }

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

            public bool IsRightAngle { get; set; }
            public Corner OutterCorner { get; set; }
            public Corner InnerCorner { get; set; }

            public CornerInfo(CornerPos cornerPos, double radius, double circleThickness, double width, double height)
                : this()
            {
                _cornerPos = cornerPos;
                _radius = radius;
                _circleThickness = circleThickness;
                _width = width;
                _height = height;

                IsRightAngle = DoubleUtil.IsZero(_radius);
                OutterCorner = ConstructCorner(0, circleThickness);
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
            public double Radius { get; private set; }
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

            public Point GetSpecialPoint(double availableAngle)
            {
                if (DoubleUtil.IsZero(_arcLength))
                    return RightAnglePoint;

                var d1 = Math.Sin(availableAngle * Math.PI / 180) * Radius;
                var d2 = (1 - Math.Cos(availableAngle * Math.PI / 180)) * Radius;

                return new Point(ArcFromPoint.X + d1, ArcFromPoint.Y + d2);
            }
        }
    }
}

