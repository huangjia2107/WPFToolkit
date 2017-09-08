using System;
using System.Globalization;
using System.Windows;
using System.Windows.Media;

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

        public static readonly DependencyProperty OffsetProperty =
            DependencyProperty.Register("Offset", typeof(decimal), _typeofSelf, new FrameworkPropertyMetadata(12m, FrameworkPropertyMetadataOptions.AffectsRender));
        public decimal Offset
        {
            get { return (decimal)GetValue(OffsetProperty); }
            set { SetValue(OffsetProperty, value); }
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

        private void InitStepInfo(ref decimal currentStep, ref decimal miniStep, ref int miniStepCount)
        {
            if (Scale == 1)
            {
                currentStep = _baseStep;
                miniStep = _baseStep / 10;
                miniStepCount = 10;

                return;
            }

            decimal tempStep = Scale;
            while (true)
            {
                if (tempStep / 4 >= 20)
                {
                    currentStep = tempStep;
                    miniStep = tempStep / 20;
                    miniStepCount = 20;

                    break;
                }

                if (tempStep / 4 >= 10)
                {
                    currentStep = tempStep;
                    miniStep = tempStep / 10;
                    miniStepCount = 10;

                    break;
                }

                if (tempStep / 5 >= 5)
                {
                    currentStep = tempStep;
                    miniStep = tempStep / 5;
                    miniStepCount = 5;

                    break;
                }

                if (Scale < (decimal)0.1)
                {
                    if (tempStep == Scale)
                        tempStep = Scale * 500;
                    else
                        tempStep += Scale * 500;
                }
                else if (Scale < 1)
                {
                    if (tempStep == Scale)
                        tempStep = Scale * 50;
                    else
                        tempStep += Scale * 50;
                }
                else if (Scale <= 10)
                {
                    if (tempStep == Scale)
                        tempStep = Scale * 5;
                    else
                        tempStep += Scale * 5;
                }
                else
                {
                    if (tempStep == Scale)
                        tempStep = Scale * 2;
                    else
                        tempStep += Scale * 2;
                }
            }
        }

        public double PtToDip(double pt)
        {
            return (pt * 96.0 / 72.0);
        }

        private void DrawStep(DrawingContext dc, decimal stepIndex, decimal currentStep, int miniStepCount, bool ignoreFirstMark = false)
        {
            var currentStepIndex = (stepIndex - Offset * Scale);
            if (currentStepIndex % currentStep == 0)
            {
                var mark = Math.Round(currentStepIndex / Scale, 0);
                if (ignoreFirstMark && mark == 0)
                    return;

                dc.DrawLine(_pen, new Point((double)stepIndex + 0.5, ActualHeight), new Point((double)stepIndex + 0.5, 0));

                var ft = new FormattedText(
                        Math.Round(currentStepIndex / Scale, 0).ToString(CultureInfo.CurrentCulture),
                        CultureInfo.CurrentCulture,
                        FlowDirection.LeftToRight,
                        new Typeface("Arial"),
                        PtToDip(6),
                        Brushes.DimGray);
                ft.SetFontWeight(FontWeights.Regular);
                ft.TextAlignment = TextAlignment.Left;

                dc.DrawText(ft, new Point((double)stepIndex + 1.5, 0));
            }
            else
            {
                if (miniStepCount == 5)
                {
                    if (currentStepIndex % (currentStep / 5) == 0)
                        dc.DrawLine(_pen, new Point((double)stepIndex + 0.5, ActualHeight * 1 / 2), new Point((double)stepIndex + 0.5, ActualHeight));
                }

                if (miniStepCount == 10)
                {
                    if (currentStepIndex % (currentStep / 2) == 0)
                        dc.DrawLine(_pen, new Point((double)stepIndex + 0.5, ActualHeight * 1 / 5), new Point((double)stepIndex + 0.5, ActualHeight));
                    else if (currentStepIndex % (currentStep / 5) == 0)
                        dc.DrawLine(_pen, new Point((double)stepIndex + 0.5, ActualHeight * 1 / 2), new Point((double)stepIndex + 0.5, ActualHeight));
                    else
                        dc.DrawLine(_pen, new Point((double)stepIndex + 0.5, ActualHeight * 5 / 8), new Point((double)stepIndex + 0.5, ActualHeight));
                }

                if (miniStepCount == 20)
                {
                    if (currentStepIndex % (currentStep / 2) == 0)
                        dc.DrawLine(_pen, new Point((double)stepIndex + 0.5, ActualHeight * 1 / 5), new Point((double)stepIndex + 0.5, ActualHeight));
                    else if (currentStepIndex % (currentStep / 4) == 0)
                        dc.DrawLine(_pen, new Point((double)stepIndex + 0.5, ActualHeight * 1 / 2), new Point((double)stepIndex + 0.5, ActualHeight));
                    else if (currentStepIndex % (currentStep / 10) == 0)
                        dc.DrawLine(_pen, new Point((double)stepIndex + 0.5, ActualHeight * 5 / 8), new Point((double)stepIndex + 0.5, ActualHeight));
                    else
                        dc.DrawLine(_pen, new Point((double)stepIndex + 0.5, ActualHeight * 23 / 32), new Point((double)stepIndex + 0.5, ActualHeight));
                }
            }
        }

        private void DrawOffsetRight(DrawingContext dc, decimal currentStep, decimal miniStep, int miniStepCount)
        {
            if (Offset >= (decimal)ActualWidth)
                return;

            for (var stepIndex = Offset * Scale; stepIndex < (decimal)ActualWidth; stepIndex += miniStep)
            {
                if (stepIndex < 0)
                    continue;

                DrawStep(dc, stepIndex, currentStep, miniStepCount);
            }
        }

        private void DrawOffsetLeft(DrawingContext dc, decimal currentStep, decimal miniStep, int miniStepCount)
        {
            if (Offset <= 0)
                return;

            for (var stepIndex = Offset * Scale; stepIndex >= 0; stepIndex -= miniStep)
            {
                if (stepIndex > (decimal)ActualWidth)
                    continue;

                DrawStep(dc, stepIndex, currentStep, miniStepCount, true);
            }
        }

        private void Draw(DrawingContext dc)
        {
            decimal currentStep = 0;
            decimal miniStep = 0;
            int miniStepCount = 0;
            InitStepInfo(ref currentStep, ref miniStep, ref miniStepCount);

            dc.DrawLine(_pen, new Point(0, ActualHeight), new Point(ActualWidth, ActualHeight));

            DrawOffsetRight(dc, currentStep, miniStep, miniStepCount);
            DrawOffsetLeft(dc, currentStep, miniStep, miniStepCount);
        }
    }
}
