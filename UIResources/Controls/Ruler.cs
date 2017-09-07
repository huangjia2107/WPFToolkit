using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using UIResources.Helps;

namespace UIResources.Controls
{
    public enum Unit
    {
        Pixel,
        Millimeter,
        Centimeter,
        Inch,
        Foot
    }

    public class Ruler : FrameworkElement
    {
        private static readonly Type _typeofSelf = typeof(Ruler);

        // 
        private double _step = 50;

        private double _tempStep = 50;

        private readonly Pen _pen = new Pen(Brushes.Black, 1.0);

        public Ruler()
        {
            RenderOptions.SetEdgeMode(this, EdgeMode.Aliased);
        }


        public static readonly DependencyProperty OriginOffsetProperty =
            DependencyProperty.Register("OriginOffset", typeof(double), _typeofSelf, new FrameworkPropertyMetadata(0d, FrameworkPropertyMetadataOptions.AffectsRender));
        public double OriginOffset
        {
            get { return (double)GetValue(OriginOffsetProperty); }
            set { SetValue(OriginOffsetProperty, value); }
        }

        public static readonly DependencyProperty ScaleProperty =
            DependencyProperty.Register("Scale", typeof(double), _typeofSelf, new FrameworkPropertyMetadata(1d, FrameworkPropertyMetadataOptions.AffectsRender, OnScalePropertyChanged));
        public double Scale
        {
            get { return (double)GetValue(ScaleProperty); }
            set { SetValue(ScaleProperty, value); }
        }

        static void OnScalePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var ruler = (Ruler)sender;
            // ruler._tempStep = Math.Floor();
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            Draw(drawingContext);
        }

        private double GetMiniStep()
        {
            if (DoubleUtil.AreClose(Scale, 1))
                return _step;

            double tempStep = Scale;
            while (true)
            {
                if (DoubleUtil.GreaterThanOrClose(tempStep / 3, 20) || DoubleUtil.GreaterThanOrClose(tempStep / 3, 10) || DoubleUtil.GreaterThanOrClose(tempStep / 5, 5))
                    break;

                tempStep *= 2;
            }

            return tempStep;
        }

        private void GetMinStepDivide(double tempStep, ref double miniStep, ref double divideCount)
        {
            if (DoubleUtil.GreaterThanOrClose(tempStep / 20, 4))
            {
                miniStep = tempStep / 20;
                divideCount = 20;

                return;
            }


            if (DoubleUtil.GreaterThanOrClose(tempStep / 10, 4))
            {
                miniStep = tempStep / 10;
                divideCount = 10;

                return;
            }

            if (DoubleUtil.GreaterThanOrClose(tempStep / 5, 5))
            {
                miniStep = tempStep / 5;
                divideCount = 5;

                return;
            }

            miniStep = tempStep / 5;
            divideCount = 5;
        }

        public double PtToDip(double pt)
        {
            return (pt * 96.0 / 72.0);
        }

        private void Draw(DrawingContext dc)
        {
            double tempStep = GetMiniStep();
            double miniStep = 0;
            double divideCount = 0;
            GetMinStepDivide(tempStep, ref miniStep, ref divideCount);

            dc.DrawLine(_pen, new Point(-0.5, ActualHeight), new Point(ActualWidth - 0.5, ActualHeight));
            for (double markIndex = 0; markIndex < ActualWidth; markIndex = Math.Round(markIndex + miniStep, 1, MidpointRounding.AwayFromZero))
            {
                if (DoubleUtil.AreClose(markIndex % tempStep, 0))
                {
                    dc.DrawLine(_pen, new Point(markIndex, ActualHeight), new Point(markIndex, 0));

                    var ft = new FormattedText(
                            Math.Round(markIndex / Scale, 0).ToString(CultureInfo.CurrentCulture),
                            CultureInfo.CurrentCulture,
                            FlowDirection.LeftToRight,
                            new Typeface("Arial"),
                            PtToDip(6),
                            Brushes.DimGray);
                    ft.SetFontWeight(FontWeights.Regular);
                    ft.TextAlignment = TextAlignment.Left;

                    dc.DrawText(ft, new Point(markIndex + 1, 0));
                }
                else
                {
                    if (divideCount == 5)
                    {
                        if (DoubleUtil.AreClose(markIndex % (tempStep / 5), 0))
                            dc.DrawLine(_pen, new Point(markIndex, ActualHeight * 1 / 2), new Point(markIndex, ActualHeight));
                    }

                    if (divideCount == 10)
                    {
                        if (DoubleUtil.AreClose(markIndex % (tempStep / 2), 0))
                            dc.DrawLine(_pen, new Point(markIndex, ActualHeight * 1 / 5), new Point(markIndex, ActualHeight));
                        else if (DoubleUtil.AreClose(markIndex % (tempStep / 5), 0))
                            dc.DrawLine(_pen, new Point(markIndex, ActualHeight * 1 / 2), new Point(markIndex, ActualHeight));
                        else
                            dc.DrawLine(_pen, new Point(markIndex, ActualHeight * 5 / 8), new Point(markIndex, ActualHeight));
                    }

                    if (divideCount == 20)
                    {
                        if (DoubleUtil.AreClose(markIndex % (tempStep / 2), 0))
                            dc.DrawLine(_pen, new Point(markIndex, ActualHeight * 1 / 5), new Point(markIndex, ActualHeight));
                        else if (DoubleUtil.AreClose(markIndex % (tempStep / 4), 0))
                            dc.DrawLine(_pen, new Point(markIndex, ActualHeight * 1 / 2), new Point(markIndex, ActualHeight));
                        else if (DoubleUtil.AreClose(markIndex % (tempStep / 10), 0))
                            dc.DrawLine(_pen, new Point(markIndex, ActualHeight * 5 / 8), new Point(markIndex, ActualHeight));
                        else
                            dc.DrawLine(_pen, new Point(markIndex, ActualHeight * 23 / 32), new Point(markIndex, ActualHeight));
                    }

                }
            }
        }
    }
}
