using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using Utils.Common;

namespace UIResources.Controls
{
    public class Sector : Shape
    {
        public static readonly DependencyProperty Radius1Property = DependencyProperty.Register("Radius1", typeof(double), typeof(Sector),
            new FrameworkPropertyMetadata(10d, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender, null, CoerceRadius));
        public double Radius1
        {
            get { return (double)GetValue(Radius1Property); }
            set { SetValue(Radius1Property, value); }
        }

        public static readonly DependencyProperty Radius2Property = DependencyProperty.Register("Radius2", typeof(double), typeof(Sector),
            new FrameworkPropertyMetadata(5d, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender, null, CoerceRadius));
        public double Radius2
        {
            get { return (double)GetValue(Radius2Property); }
            set { SetValue(Radius2Property, value); }
        }

        static object CoerceRadius(DependencyObject d, object value)
        {
            return Math.Max(0, (double)value);
        }

        public static readonly DependencyProperty AngleProperty = DependencyProperty.Register("Angle", typeof(double), typeof(Sector),
            new FrameworkPropertyMetadata(45d, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender, null, CoerceAngle));
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
                if (DoubleUtil.AreClose(Radius1, Radius2))
                    return Geometry.Empty;

                var sectorGeometry = new StreamGeometry();
                var outerRadius = Math.Max(Radius1, Radius2);
                var innerRadius = Math.Min(Radius1, Radius2);

                using (var sgc = sectorGeometry.Open())
                {
                    var radian = GetRadian(Angle);

                    var outerSinLenth = outerRadius * Math.Sin(radian / 2);
                    var outerCosLenth = outerRadius * Math.Cos(radian / 2);

                    sgc.BeginFigure(new Point(0, outerRadius - outerCosLenth), true, true);
                    ArcTo(sgc, new Point(outerSinLenth * 2, outerRadius - outerCosLenth), new Size(outerRadius, outerRadius), Angle);

                    if (DoubleUtil.IsZero(innerRadius))
                    {
                        sgc.LineTo(new Point(outerSinLenth, outerRadius), true, false);
                    }
                    else
                    {
                        var innerSinLenth = innerRadius * Math.Sin(radian / 2);
                        var innerCosLenth = innerRadius * Math.Cos(radian / 2);

                        sgc.LineTo(new Point(outerSinLenth + innerSinLenth, outerRadius - innerCosLenth), true, false);
                        sgc.ArcTo(new Point(outerSinLenth - innerSinLenth, outerRadius - innerCosLenth),
                            new Size(innerRadius, innerRadius),
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

        private double GetRadian(double angle)
        {
            return Math.PI / 180 * angle;
        }
    }
}
