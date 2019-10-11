using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using Utils.Common;

namespace UIResources.Controls
{
    public class Sector : Shape
    {
        public static readonly DependencyProperty OuterRadiusProperty =
            DependencyProperty.Register("OuterRadius", typeof(double), typeof(Sector), new FrameworkPropertyMetadata(60d, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender, null, CoerceOuterRadius));
        public double OuterRadius
        {
            get { return (double)GetValue(OuterRadiusProperty); }
            set { SetValue(OuterRadiusProperty, value); }
        }
        static object CoerceOuterRadius(DependencyObject d, object value)
        {
            var ctrl = (Sector)d;
            var v = (double)value;

            return Math.Max(ctrl.InnerRadius, v);
        }

        public static readonly DependencyProperty InnerRadiusProperty =
            DependencyProperty.Register("InnerRadius", typeof(double), typeof(Sector), new FrameworkPropertyMetadata(20d, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender, null, CoerceInnerRadius));
        public double InnerRadius
        {
            get { return (double)GetValue(InnerRadiusProperty); }
            set { SetValue(InnerRadiusProperty, value); }
        }
        static object CoerceInnerRadius(DependencyObject d, object value)
        {
            var ctrl = (Sector)d;
            var v = (double)value;

            return Math.Min(Math.Max(0, v), ctrl.OuterRadius);
        }

        public static readonly DependencyProperty AngleProperty =
            DependencyProperty.Register("Angle", typeof(double), typeof(Sector), new FrameworkPropertyMetadata(45d, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender, null, CoerceAngle));
        public double Angle
        {
            get { return (double)GetValue(AngleProperty); }
            set { SetValue(AngleProperty, value); }
        }
        static object CoerceAngle(DependencyObject d, object value)
        {
            var v = (double)value;
            return Math.Min(Math.Max(0, v), 360);
        }

        protected override Geometry DefiningGeometry
        {
            get
            {
                if (DoubleUtil.LessThanOrClose(OuterRadius, InnerRadius))
                    return Geometry.Empty;

                var sectorGeometry = new StreamGeometry();
                using (var sgc = sectorGeometry.Open())
                {
                    var radian = GetRadian(Angle);

                    var outerSinLenth = OuterRadius * Math.Sin(radian / 2);
                    var outerCosLenth = OuterRadius * Math.Cos(radian / 2);

                    sgc.BeginFigure(new Point(0, OuterRadius - outerCosLenth), true, true);
                    ArcTo(sgc, new Point(outerSinLenth * 2, OuterRadius - outerCosLenth), new Size(OuterRadius, OuterRadius), Angle);

                    if (DoubleUtil.IsZero(InnerRadius))
                    {
                        sgc.LineTo(new Point(outerSinLenth, OuterRadius), true, false);
                    }
                    else
                    {
                        var innerSinLenth = InnerRadius * Math.Sin(radian / 2);
                        var innerCosLenth = InnerRadius * Math.Cos(radian / 2);

                        sgc.LineTo(new Point(outerSinLenth + innerSinLenth, OuterRadius - innerCosLenth), true, false);
                        sgc.ArcTo(new Point(outerSinLenth - innerSinLenth, OuterRadius - innerCosLenth),
                            new Size(InnerRadius, InnerRadius),
                            Angle,
                            DoubleUtil.GreaterThanOrClose(Angle, 180),
                            SweepDirection.Counterclockwise, true, false);
                    }
                }

                return sectorGeometry;
            }
        }

        private void ArcTo(StreamGeometryContext sgc, Point point, Size size, double rotationAngle)
        {
            sgc.ArcTo(point, size, rotationAngle, DoubleUtil.GreaterThanOrClose(Angle, 180), SweepDirection.Clockwise, true, false);
        }

        private void LineTo(StreamGeometryContext sgc, Point point)
        {
            sgc.LineTo(point, true, false);
        }

        private double GetRadian(double angle)
        {
            return Math.PI / 180 * angle;
        }
    }
}
