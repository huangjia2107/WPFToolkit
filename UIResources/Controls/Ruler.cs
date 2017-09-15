using System;
using System.Globalization;
using System.Windows;
using System.Windows.Media;

namespace UIResources.Controls
{
    public enum RulerUnit
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

        private readonly Pen _pen = new Pen(Brushes.Black, 1.0);
        private readonly DrawingGroup _drawingGroup = new DrawingGroup();  

        //DefferRefresh
        int _deferLevel = 0;

        public Ruler()
        {
            RenderOptions.SetEdgeMode(this, EdgeMode.Aliased); 
        }

        public static readonly DependencyProperty UnitProperty =
           DependencyProperty.Register("Unit", typeof(RulerUnit), _typeofSelf, new PropertyMetadata(RulerUnit.Pixel, OnUnitPropertyChanged));
        public RulerUnit Unit
        {
            get { return (RulerUnit)GetValue(UnitProperty); }
            set { SetValue(UnitProperty, value); }
        }

        static void OnUnitPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var ruler = sender as Ruler;
            if (ruler._deferLevel == 0)
                ruler.Render();
        }

        public static readonly DependencyProperty ShiftProperty =
            DependencyProperty.Register("Shift", typeof(decimal), _typeofSelf, new PropertyMetadata(0m, OnShiftPropertyChanged));
        public decimal Shift
        {
            get { return (decimal)GetValue(ShiftProperty); }
            set { SetValue(ShiftProperty, value); }
        }

        static void OnShiftPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var ruler = sender as Ruler;
            if (ruler._deferLevel == 0)
                ruler.Render();
        }

        public static readonly DependencyProperty ScaleProperty =
            DependencyProperty.Register("Scale", typeof(decimal), _typeofSelf, new FrameworkPropertyMetadata(1m, OnScalePropertyChanged));
        public decimal Scale
        {
            get { return (decimal)GetValue(ScaleProperty); }
            set { SetValue(ScaleProperty, value); }
        }
        static void OnScalePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var ruler = sender as Ruler;
            if (ruler._deferLevel == 0)
                ruler.Render();
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            Render();
            drawingContext.DrawDrawing(_drawingGroup);
        }

        #region DeferRefresh

        private class DeferHelper : IDisposable
        {
            public DeferHelper(Ruler ruler)
            {
                _ruler = ruler;
            }

            public void Dispose()
            {
                if (_ruler != null)
                {
                    _ruler.EndDefer();
                    _ruler = null;
                }

                GC.SuppressFinalize(this);
            }

            private Ruler _ruler;
        }

        public virtual IDisposable DeferRefresh()
        {
            ++_deferLevel;
            return new DeferHelper(this);
        }

        private void EndDefer()
        {
            --_deferLevel;

            if (_deferLevel == 0)
                Render();
        }

        #endregion

        #region Private Func

        private void Render()
        {
            using (var dc = _drawingGroup.Open())
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

        private int GetPrecision()
        {
            int result = 0;
            switch (Unit)
            {
                case RulerUnit.Pixel:
                    result = 0;
                    break;
                case RulerUnit.Inch:
                    result = 2;
                    break;
                case RulerUnit.Foot:
                    result = 3;
                    break;
                case RulerUnit.Millimeter:
                    result = 0;
                    break;
                case RulerUnit.Centimeter:
                    result = 1;
                    break;
            }

            return result;
        }

        private decimal GetPixelPerUnit()
        {
            decimal result = 0;
            switch (Unit)
            {
                case RulerUnit.Pixel:
                    result = 1;
                    break;
                case RulerUnit.Inch:
                    result = 96;
                    break;
                case RulerUnit.Foot:
                    result = 1152;
                    break;
                case RulerUnit.Millimeter:
                    result = 3.7795m;
                    break;
                case RulerUnit.Centimeter:
                    result = 37.795m;
                    break;
            }

            return result;
        }

        private decimal GetBaseStep()
        {
            decimal result = 0;
            switch (Unit)
            {
                case RulerUnit.Pixel:
                    result = 1;
                    break;
                case RulerUnit.Inch:
                    result = 0.96m;
                    break;
                case RulerUnit.Foot:
                    result = 1.152m;
                    break;
                case RulerUnit.Millimeter:
                    result = 3.7795m;
                    break;
                case RulerUnit.Centimeter:
                    result = 3.7795m;
                    break;
            }

            return result;
        }

        private void InitStepInfo(ref decimal currentStep, ref decimal miniStep, ref int miniStepCount)
        {
            //             if (Scale == 1)
            //             {
            //                 currentStep = _baseStep;
            //                 miniStep = _baseStep / 10;
            //                 miniStepCount = 10;
            // 
            //                 return;
            //             }

            decimal tempScale = Scale * GetBaseStep();
            decimal tempStep = tempScale;

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
                    if (tempStep == tempScale)
                        tempStep = tempScale * 500;
                    else
                        tempStep += tempScale * 500;
                }
                else if (Scale < 1)
                {
                    if (tempStep == tempScale)
                        tempStep = tempScale * 50;
                    else
                        tempStep += tempScale * 50;
                }
                else if (Scale <= 10)
                {
                    if (tempStep == tempScale)
                        tempStep = tempScale * 5;
                    else
                        tempStep += tempScale * 5;
                }
                else
                {
                    if (tempStep == tempScale)
                        tempStep = tempScale * 2;
                    else
                        tempStep += tempScale * 2;
                }
            }
        }

        public double PtToDip(double pt)
        {
            return (pt * 96.0 / 72.0);
        }

        private void DrawStep(DrawingContext dc, decimal stepIndex, decimal currentStep, int miniStepCount, bool ignoreFirstMark = false)
        {
            var currentStepIndex = (stepIndex - Shift * Scale * GetPixelPerUnit());
            if (currentStepIndex % currentStep == 0)
            {
                var mark = Math.Round(currentStepIndex / (Scale * GetPixelPerUnit()), GetPrecision());
                if (ignoreFirstMark && mark == 0)
                    return;

                dc.DrawLine(_pen, new Point((double)stepIndex + 0.5, ActualHeight), new Point((double)stepIndex + 0.5, 0));

                var ft = new FormattedText(
                        mark.ToString("#0.###"),
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
            if (Shift >= (decimal)ActualWidth)
                return;

            for (var stepIndex = Shift * Scale * GetPixelPerUnit(); stepIndex < (decimal)ActualWidth; stepIndex += miniStep)
            {
                if (stepIndex < 0)
                    continue;

                DrawStep(dc, stepIndex, currentStep, miniStepCount);
            }
        }

        private void DrawOffsetLeft(DrawingContext dc, decimal currentStep, decimal miniStep, int miniStepCount)
        {
            if (Shift <= 0)
                return;

            for (var stepIndex = Shift * Scale * GetPixelPerUnit(); stepIndex >= 0; stepIndex -= miniStep)
            {
                if (stepIndex > (decimal)ActualWidth)
                    continue;

                DrawStep(dc, stepIndex, currentStep, miniStepCount, true);
            }
        }

        #endregion
    }
}
