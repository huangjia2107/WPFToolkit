using System;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;
using System.Windows.Navigation;

namespace UIResources.Panels
{
    public enum AnchorDock
    {
        None = 0,

        LeftTop,
        LeftCenter,
        LeftBottom,

        TopLeft,
        TopCenter,
        TopRight,

        RightTop,
        RightCenter,
        RightBottom,

        BottomLeft,
        BottomCenter,
        BottomRight
    }

    public class AnchorBorder : Decorator
    {
        private CornerRadius _tempCornerRadius;
        private StreamGeometry _streamGeometryCache; 

        #region readonly Properties

        private static readonly DependencyPropertyKey ActualRenderWidthPropertyKey =
           DependencyProperty.RegisterReadOnly("ActualRenderWidth", typeof(double), typeof(AnchorBorder), new PropertyMetadata(0d));
        public static readonly DependencyProperty ActualRenderWidthProperty = ActualRenderWidthPropertyKey.DependencyProperty;
        public double ActualRenderWidth
        {
            get { return (double)GetValue(ActualRenderWidthProperty); }
        }

        private static readonly DependencyPropertyKey ActualRenderHeightPropertyKey =
            DependencyProperty.RegisterReadOnly("ActualRenderHeight", typeof(double), typeof(AnchorBorder), new PropertyMetadata(0d));
        public static readonly DependencyProperty ActualRenderHeightProperty = ActualRenderHeightPropertyKey.DependencyProperty;
        public double ActualRenderHeight
        {
            get { return (double)GetValue(ActualRenderHeightProperty); }
        }

        #endregion

        #region public Properties

        public static readonly DependencyProperty AnchorDockProperty = DependencyProperty.Register("AnchorDock", typeof(AnchorDock), typeof(AnchorBorder),
            new FrameworkPropertyMetadata(AnchorDock.RightTop, FrameworkPropertyMetadataOptions.AffectsRender));
        public AnchorDock AnchorDock
        {
            get { return (AnchorDock)GetValue(AnchorDockProperty); }
            set { SetValue(AnchorDockProperty, value); }
        }

        public static readonly DependencyProperty DockOffsetProperty = DependencyProperty.Register("DockOffset", typeof(double), typeof(AnchorBorder),
            new FrameworkPropertyMetadata(0d, FrameworkPropertyMetadataOptions.AffectsRender));
        public double DockOffset
        {
            get { return (double)GetValue(DockOffsetProperty); }
            set { SetValue(DockOffsetProperty, value); }
        }

        public static readonly DependencyProperty DockLengthProperty = DependencyProperty.Register("DockLength", typeof(double), typeof(AnchorBorder),
            new FrameworkPropertyMetadata(20d, FrameworkPropertyMetadataOptions.AffectsRender, null, CoerceDockLength));
        public double DockLength
        {
            get { return (double)GetValue(DockLengthProperty); }
            set { SetValue(DockLengthProperty, value); }
        }

        static object CoerceDockLength(DependencyObject d, object value)
        {
            return Math.Max(0, (double)value);
        }

        public static readonly DependencyProperty AnchorOffsetProperty = DependencyProperty.Register("AnchorOffset", typeof(Point), typeof(AnchorBorder),
            new FrameworkPropertyMetadata(new Point(50, 20), FrameworkPropertyMetadataOptions.AffectsRender));
        public Point AnchorOffset
        {
            get { return (Point)GetValue(AnchorOffsetProperty); }
            set { SetValue(AnchorOffsetProperty, value); }
        }

        public static readonly DependencyProperty BackgroundProperty = Panel.BackgroundProperty.AddOwner(typeof(AnchorBorder),
            new FrameworkPropertyMetadata(Brushes.Transparent, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.SubPropertiesDoNotAffectRender));
        public Brush Background
        {
            get { return (Brush)GetValue(BackgroundProperty); }
            set { SetValue(BackgroundProperty, value); }
        }

        public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.Register("CornerRadius", typeof(CornerRadius), typeof(AnchorBorder),
            new FrameworkPropertyMetadata(new CornerRadius(), FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender, CornerRadiusPropertyChangedCallback));
        public CornerRadius CornerRadius
        {
            get { return (CornerRadius)GetValue(CornerRadiusProperty); }
            set { SetValue(CornerRadiusProperty, value); }
        }

        static void CornerRadiusPropertyChangedCallback(DependencyObject dp, DependencyPropertyChangedEventArgs e)
        {
            var anchorBorder = dp as AnchorBorder;
            if (anchorBorder != null)
                anchorBorder._tempCornerRadius = (CornerRadius)e.NewValue;
        }

        public static readonly DependencyProperty BorderBrushProperty = DependencyProperty.Register("BorderBrush", typeof(Brush), typeof(AnchorBorder),
            new FrameworkPropertyMetadata(Brushes.Black, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.SubPropertiesDoNotAffectRender));
        public Brush BorderBrush
        {
            get { return (Brush)GetValue(BorderBrushProperty); }
            set { SetValue(BorderBrushProperty, value); }
        }

        public static readonly DependencyProperty BorderThicknessProperty = DependencyProperty.Register("BorderThickness", typeof(double), typeof(AnchorBorder),
            new FrameworkPropertyMetadata(0.5d, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender, null, CoerceBorderThickness));
        public double BorderThickness
        {
            get { return (double)GetValue(BorderThicknessProperty); }
            set { SetValue(BorderThicknessProperty, value); }
        }
        static object CoerceBorderThickness(DependencyObject d, object value)
        {
            return Math.Max(0, (double)value);
        }

        public static readonly DependencyProperty PaddingProperty = DependencyProperty.Register("Padding", typeof(Thickness), typeof(AnchorBorder),
            new FrameworkPropertyMetadata(default(Thickness), FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));
        public Thickness Padding
        {
            get { return (Thickness)GetValue(PaddingProperty); }
            set { SetValue(PaddingProperty, value); }
        }

        #endregion

        #region Override

        protected override Size MeasureOverride(Size constraint)
        {
            var child = Child;
            // Combine into total decorating size
            var combined = GetSizeForDecorator();

            //If we have a child
            if (child != null)
            {
                // Remove size of border only from child's reference size.
                Size childConstraint = new Size(Math.Max(0.0, constraint.Width - combined.Width),
                                                Math.Max(0.0, constraint.Height - combined.Height));

                child.Measure(childConstraint);
                var childSize = child.DesiredSize;

                // Now use the returned size to drive our size, by adding back the margins, etc.
                combined.Width = childSize.Width + combined.Width;
                combined.Height = childSize.Height + combined.Height;
            }

            return combined;
        }

        protected override Size ArrangeOverride(Size arrangeSize)
        {
            var child = Child;
            var childRect = GetChildRect(arrangeSize);

            if (child != null)
                child.Arrange(childRect);

            return arrangeSize;
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            DrawDecorativeBorder(this.ActualWidth, this.ActualHeight, drawingContext);
        }

        #endregion

        #region Private Func

        private static Size HelperCollapseThickness(Thickness th)
        {
            return new Size(th.Left + th.Right, th.Top + th.Bottom);
        }

        private static Size HelperCollapseBorder(double db)
        {
            return new Size(db * 2, db * 2);
        }

        private Size GetSizeForDecorator()
        {
            var border = HelperCollapseBorder(BorderThickness);
            var padding = HelperCollapseThickness(Padding);

            return new Size(border.Width + padding.Width, border.Height + padding.Height);  //获取“边”及“Padding”所占空间大小 
        }

        private Rect GetChildRect(Size arrangeSize)
        {
            var boundRect = new Rect(arrangeSize);
            var childRect = new Rect(boundRect.Left + BorderThickness + Padding.Left,
                                      boundRect.Top + BorderThickness + Padding.Top,
                                      Math.Max(0.0, boundRect.Width - (BorderThickness * 2 + Padding.Left + Padding.Right)),
                                      Math.Max(0.0, boundRect.Height - (BorderThickness * 2 + Padding.Top + Padding.Bottom)));

            return childRect;
        }

        private void DrawBorderWithAnchor(StreamGeometryContext ctx, Point? dockOne, Point? dockAnchor, Point? dockTwo, Point? end)
        {
            if (ctx == null)
                return;

            if (dockOne != null)
                ctx.LineTo(dockOne.Value, true, false);

            if (dockAnchor != null)
                ctx.LineTo(dockAnchor.Value, true, false);

            if (dockTwo != null)
                ctx.LineTo(dockTwo.Value, true, false);

            if (end != null)
                ctx.LineTo(end.Value, true, false);
        }

        private bool CheckAnchorOffset()
        {
            bool result = false;
            switch (AnchorDock)
            {
                case AnchorDock.LeftTop:
                case AnchorDock.LeftCenter:
                case AnchorDock.LeftBottom:
                    result = (AnchorOffset.X < 0);
                    break;

                case AnchorDock.RightTop:
                case AnchorDock.RightCenter:
                case AnchorDock.RightBottom:
                    result = (AnchorOffset.X > 0);
                    break;

                case AnchorDock.TopLeft:
                case AnchorDock.TopCenter:
                case AnchorDock.TopRight:
                    result = (AnchorOffset.Y < 0);
                    break;

                case AnchorDock.BottomLeft:
                case AnchorDock.BottomCenter:
                case AnchorDock.BottomRight:
                    result = (AnchorOffset.Y > 0);
                    break;
            }

            return result;
        }

        private bool CheckDockOffset(double leftRadius, double rightRadius, double borderLength)
        {
            bool result = false;
            switch (AnchorDock)
            {
                case AnchorDock.LeftTop:
                case AnchorDock.RightTop:
                case AnchorDock.TopLeft:
                case AnchorDock.BottomLeft:
                    result = DockOffset >= leftRadius && DockLength > 0 && (DockOffset + DockLength <= borderLength - rightRadius);
                    break;

                case AnchorDock.LeftCenter:
                case AnchorDock.RightCenter:
                case AnchorDock.TopCenter:
                case AnchorDock.BottomCenter:
                    result = (Math.Abs(DockOffset) <= (borderLength - DockLength - leftRadius - rightRadius) / 2) && DockLength > 0;

                    break;
                case AnchorDock.LeftBottom:
                case AnchorDock.RightBottom:
                case AnchorDock.TopRight:
                case AnchorDock.BottomRight:
                    result = DockOffset >= rightRadius && DockLength > 0 && (DockOffset + DockLength <= borderLength - leftRadius);

                    break;
            }

            return result;
        }

        private bool ComputeDockOffset(double leftRadius, double rightRadius, double borderLength, ref double dockOneOffset, ref double dockTwoOffset)
        {
            if (!CheckAnchorOffset() || !CheckDockOffset(leftRadius, rightRadius, borderLength))
                return false;

            switch (AnchorDock)
            {
                case AnchorDock.LeftTop:
                case AnchorDock.RightTop:
                case AnchorDock.TopLeft:
                case AnchorDock.BottomLeft:
                    dockOneOffset = DockOffset;
                    dockTwoOffset = dockOneOffset + DockLength;

                    break;
                case AnchorDock.LeftCenter:
                case AnchorDock.RightCenter:
                case AnchorDock.TopCenter:
                case AnchorDock.BottomCenter:
                    dockOneOffset = (borderLength - leftRadius - rightRadius - DockLength) / 2 + leftRadius + DockOffset;
                    dockTwoOffset = dockOneOffset + DockLength;

                    break;
                case AnchorDock.LeftBottom:
                case AnchorDock.RightBottom:
                case AnchorDock.TopRight:
                case AnchorDock.BottomRight:

                    dockTwoOffset = borderLength - DockOffset;
                    dockOneOffset = dockTwoOffset - DockLength;
                    break;
            }

            return true;
        }

        private void DrawLeftBorderWithArc(StreamGeometryContext ctx, double H)
        {
            if (ctx == null)
                return;

            double topLeftRadius = _tempCornerRadius.TopLeft;
            Size topLeftSize = new Size(topLeftRadius, topLeftRadius);

            double bottomLeftRadius = _tempCornerRadius.BottomLeft;
            Size bottomLeftSize = new Size(bottomLeftRadius, bottomLeftRadius);

            Point? dockOne = null;
            Point? dockAnchor = null;
            Point? dockTwo = null;
            Point? end = null;

            double dockOneOffset = 0d;
            double dockTwoOffset = 0d;
            if ((AnchorDock == AnchorDock.LeftTop || AnchorDock == AnchorDock.LeftCenter || AnchorDock == AnchorDock.LeftBottom)
                && ComputeDockOffset(topLeftRadius, bottomLeftRadius, H, ref dockOneOffset, ref dockTwoOffset))
            {
                dockOne = new Point(0, dockOneOffset);
                dockAnchor = AnchorOffset;
                dockTwo = new Point(0, dockTwoOffset);
            }

            end = new Point(0, H - bottomLeftRadius);
            DrawBorderWithAnchor(ctx, dockOne, dockAnchor, dockTwo, end);
            ctx.ArcTo(new Point(bottomLeftRadius, H), bottomLeftSize, 90, false, SweepDirection.Counterclockwise, true, false);
        }

        private void DrawBottomBorderWithArc(StreamGeometryContext ctx, double W, double H)
        {
            if (ctx == null)
                return;

            double bottomLeftRadius = _tempCornerRadius.BottomLeft;
            Size bottomLeftSize = new Size(bottomLeftRadius, bottomLeftRadius);

            double bottomRightRadius = _tempCornerRadius.BottomRight;
            Size bottomRightSize = new Size(bottomRightRadius, bottomRightRadius);

            Point? dockOne = null;
            Point? dockAnchor = null;
            Point? dockTwo = null;
            Point? end = null;

            double dockOneOffset = 0d;
            double dockTwoOffset = 0d;
            if ((AnchorDock == AnchorDock.BottomLeft || AnchorDock == AnchorDock.BottomCenter || AnchorDock == AnchorDock.BottomRight)
                && ComputeDockOffset(bottomLeftRadius, bottomRightRadius, W, ref dockOneOffset, ref dockTwoOffset))
            {
                dockOne = new Point(dockOneOffset, H);
                dockAnchor = new Point(AnchorOffset.X, AnchorOffset.Y + H);
                dockTwo = new Point(dockTwoOffset, H);
            }

            end = new Point(W - bottomRightRadius, H);
            DrawBorderWithAnchor(ctx, dockOne, dockAnchor, dockTwo, end);
            ctx.ArcTo(new Point(W, H - bottomRightRadius), bottomRightSize, 90, false, SweepDirection.Counterclockwise, true, false);
        }

        private void DrawRightBorderWithArc(StreamGeometryContext ctx, double W, double H)
        {
            if (ctx == null)
                return;

            double rightTopRadius = _tempCornerRadius.TopRight;
            Size rightTopSize = new Size(rightTopRadius, rightTopRadius);

            double rightBottomRadius = _tempCornerRadius.BottomRight;
            Size rightBottomSize = new Size(rightBottomRadius, rightBottomRadius);

            Point? dockOne = null;
            Point? dockAnchor = null;
            Point? dockTwo = null;
            Point? end = null;

            double dockOneOffset = 0d;
            double dockTwoOffset = 0d;
            if ((AnchorDock == AnchorDock.RightTop || AnchorDock == AnchorDock.RightCenter || AnchorDock == AnchorDock.RightBottom)
                && ComputeDockOffset(rightTopRadius, rightBottomRadius, H, ref dockOneOffset, ref dockTwoOffset))
            {
                dockOne = new Point(W, dockOneOffset);
                dockAnchor = new Point(AnchorOffset.X + W, AnchorOffset.Y);
                dockTwo = new Point(W, dockTwoOffset);
            }

            end = new Point(W, rightTopRadius);
            DrawBorderWithAnchor(ctx, dockTwo, dockAnchor, dockOne, end);
            ctx.ArcTo(new Point(W - rightTopRadius, 0), rightTopSize, 90, false, SweepDirection.Counterclockwise, true, false);
        }

        private void DrawTopBorderWithArc(StreamGeometryContext ctx, double W)
        {
            if (ctx == null)
                return;

            double topLeftRadius = _tempCornerRadius.TopLeft;
            Size topLeftSize = new Size(topLeftRadius, topLeftRadius);

            double topRightRadius = _tempCornerRadius.TopRight;
            Size topRightSize = new Size(topRightRadius, topRightRadius);

            Point? dockOne = null;
            Point? dockAnchor = null;
            Point? dockTwo = null;
            Point? end = null;

            double dockOneOffset = 0d;
            double dockTwoOffset = 0d;
            if ((AnchorDock == AnchorDock.TopLeft || AnchorDock == AnchorDock.TopCenter || AnchorDock == AnchorDock.TopRight)
                && ComputeDockOffset(topLeftRadius, topRightRadius, W, ref dockOneOffset, ref dockTwoOffset))
            {
                dockOne = new Point(dockOneOffset, 0);
                dockAnchor = AnchorOffset;
                dockTwo = new Point(dockTwoOffset, 0);
            }

            end = new Point(topLeftRadius, 0);
            DrawBorderWithAnchor(ctx, dockTwo, dockAnchor, dockOne, end);
            ctx.ArcTo(new Point(0, topLeftRadius), topLeftSize, 90, false, SweepDirection.Counterclockwise, true, false);
        }

        private void AdjustCornerRadius(double W, double H)
        {
            double? topLeft = null;
            double? bottomLeft = null;
            if (H < CornerRadius.TopLeft + CornerRadius.BottomLeft)
            {
                topLeft = CornerRadius.TopLeft * H / (CornerRadius.TopLeft + CornerRadius.BottomLeft);
                bottomLeft = CornerRadius.BottomLeft * H / (CornerRadius.TopLeft + CornerRadius.BottomLeft);
            }

            double? topRight = null;
            double? bottomRight = null;
            if (H < CornerRadius.TopRight + CornerRadius.BottomRight)
            {
                topRight = CornerRadius.TopRight * H / (CornerRadius.TopRight + CornerRadius.BottomRight);
                bottomRight = CornerRadius.TopRight * H / (CornerRadius.TopRight + CornerRadius.BottomRight);
            }

            if (W < CornerRadius.TopLeft + CornerRadius.TopRight)
            {
                var tl = CornerRadius.TopLeft * W / (CornerRadius.TopLeft + CornerRadius.TopRight);
                topLeft = topLeft == null ? tl : Math.Min(tl, topLeft.Value);

                var tr = CornerRadius.TopRight * W / (CornerRadius.TopLeft + CornerRadius.TopRight);
                topRight = topRight == null ? tr : Math.Min(tr, topRight.Value);
            }

            if (W < CornerRadius.BottomLeft + CornerRadius.BottomRight)
            {
                var bl = CornerRadius.BottomLeft * W / (CornerRadius.BottomLeft + CornerRadius.BottomRight);
                bottomLeft = bottomLeft == null ? bl : Math.Min(bl, bottomLeft.Value);

                var br = CornerRadius.BottomRight * W / (CornerRadius.BottomLeft + CornerRadius.BottomRight);
                bottomRight = bottomRight == null ? br : Math.Min(br, bottomRight.Value);
            }

            if (topLeft != null || topRight != null || bottomLeft != null || bottomRight != null)
                _tempCornerRadius = new CornerRadius(topLeft.Value, topRight.Value, bottomRight.Value, bottomLeft.Value);
            else
                _tempCornerRadius = CornerRadius;
        }

        private void DrawDecorativeBorder(double W, double H, DrawingContext drawingContext)
        {
            AdjustCornerRadius(W, H);

            if (_streamGeometryCache == null)
                _streamGeometryCache = new StreamGeometry();
            else
                _streamGeometryCache.Clear();

            using (var ctx = _streamGeometryCache.Open())
            {
                ctx.BeginFigure(new Point(0, _tempCornerRadius.TopLeft), true, true);

                DrawLeftBorderWithArc(ctx, H);
                DrawBottomBorderWithArc(ctx, W, H);
                DrawRightBorderWithArc(ctx, W, H);
                DrawTopBorderWithArc(ctx, W);
            }

            drawingContext.DrawGeometry(Background, new Pen(BorderBrush, BorderThickness), _streamGeometryCache);

            SetValue(ActualRenderWidthPropertyKey, _streamGeometryCache.Bounds.Width);
            SetValue(ActualRenderHeightPropertyKey, _streamGeometryCache.Bounds.Height);
        }

        #endregion
    }
}
