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
        private decimal _baseStep = 50;

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
            DependencyProperty.Register("Scale", typeof(decimal), _typeofSelf, new FrameworkPropertyMetadata(1m, FrameworkPropertyMetadataOptions.AffectsRender));
        public decimal Scale
        {
            get { return (decimal)GetValue(ScaleProperty); }
            set { SetValue(ScaleProperty, value); }
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            Draw(drawingContext);
        }

        private void InitStepDivide(ref decimal currentStep, ref decimal miniStep, ref int divideCount)
        {
            if (Scale == 1)
            {
                currentStep = _baseStep;
                miniStep = _baseStep / 10;
                divideCount = 10;

                return;
            }

            decimal tempStep = Scale;
            while (true)
            {
                if (tempStep / 4 >= 20)
                {
                    currentStep = tempStep;
                    miniStep = tempStep / 20;
                    divideCount = 20;

                    break;
                }

                if (tempStep / 4 >= 10)
                {
                    currentStep = tempStep;
                    miniStep = tempStep / 10;
                    divideCount = 10;

                    break;
                }

                if (tempStep / 5 >= 5)
                {
                    currentStep = tempStep;
                    miniStep = tempStep / 5;
                    divideCount = 5;

                    break;
                }

                tempStep *= 2;
            }
        }

        public double PtToDip(double pt)
        {
            return (pt * 96.0 / 72.0);
        }

        private void Draw(DrawingContext dc)
        {
            decimal tempStep = 0;
            decimal miniStep = 0;
            int divideCount = 0;
            InitStepDivide(ref tempStep, ref miniStep, ref divideCount);

            dc.DrawLine(_pen, new Point(0, ActualHeight), new Point(ActualWidth, ActualHeight));
            for (decimal markIndex = 0; markIndex < (decimal)ActualWidth; markIndex += miniStep)
            {
                if (markIndex % tempStep == 0)
                {
                    dc.DrawLine(_pen, new Point((double)markIndex + 0.5, ActualHeight), new Point((double)markIndex + 0.5, 0));

                    var ft = new FormattedText(
                            Math.Round(markIndex / Scale, 0).ToString(CultureInfo.CurrentCulture),
                            CultureInfo.CurrentCulture,
                            FlowDirection.LeftToRight,
                            new Typeface("Arial"),
                            PtToDip(6),
                            Brushes.DimGray);
                    ft.SetFontWeight(FontWeights.Regular);
                    ft.TextAlignment = TextAlignment.Left;

                    dc.DrawText(ft, new Point((double)markIndex + 1, 0));
                }
                else
                {
                    if (divideCount == 5)
                    {
                        if (markIndex % (tempStep / 5) == 0)
                            dc.DrawLine(_pen, new Point((double)markIndex + 0.5, ActualHeight * 1 / 2), new Point((double)markIndex + 0.5, ActualHeight));
                    }

                    if (divideCount == 10)
                    {
                        if (markIndex % (tempStep / 2) == 0)
                            dc.DrawLine(_pen, new Point((double)markIndex + 0.5, ActualHeight * 1 / 5), new Point((double)markIndex + 0.5, ActualHeight));
                        else if (markIndex % (tempStep / 5) == 0)
                            dc.DrawLine(_pen, new Point((double)markIndex + 0.5, ActualHeight * 1 / 2), new Point((double)markIndex + 0.5, ActualHeight));
                        else
                            dc.DrawLine(_pen, new Point((double)markIndex + 0.5, ActualHeight * 5 / 8), new Point((double)markIndex + 0.5, ActualHeight));
                    }

                    if (divideCount == 20)
                    {
                        if (markIndex % (tempStep / 2) == 0)
                            dc.DrawLine(_pen, new Point((double)markIndex + 0.5, ActualHeight * 1 / 5), new Point((double)markIndex + 0.5, ActualHeight));
                        else if (markIndex % (tempStep / 4) == 0)
                            dc.DrawLine(_pen, new Point((double)markIndex + 0.5, ActualHeight * 1 / 2), new Point((double)markIndex + 0.5, ActualHeight));
                        else if (markIndex % (tempStep / 10) == 0)
                            dc.DrawLine(_pen, new Point((double)markIndex + 0.5, ActualHeight * 5 / 8), new Point((double)markIndex + 0.5, ActualHeight));
                        else
                            dc.DrawLine(_pen, new Point((double)markIndex + 0.5, ActualHeight * 23 / 32), new Point((double)markIndex + 0.5, ActualHeight));
                    }

                }
            }
        }
    }
}
