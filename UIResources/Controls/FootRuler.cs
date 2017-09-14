using System;
using System.Globalization;
using System.Windows;
using System.Windows.Media;

namespace UIResources.Controls
{
    public class FootRuler : FrameworkElement
    {
        private static readonly Type _typeofSelf = typeof(FootRuler);

        private readonly Pen _pen = new Pen(Brushes.Black, 1.0);
        private readonly DrawingGroup _drawingGroup = new DrawingGroup();
        // 
        private decimal _baseStep = 1152;


        //DefferRefresh
        int _deferLevel = 0;

        public FootRuler()
        {
            RenderOptions.SetEdgeMode(this, EdgeMode.Aliased);
        }

        public static readonly DependencyProperty OffsetProperty =
            DependencyProperty.Register("Offset", typeof(decimal), _typeofSelf, new FrameworkPropertyMetadata(0m, OnOffsetPropertyChanged));
        public decimal Offset
        {
            get { return (decimal)GetValue(OffsetProperty); }
            set { SetValue(OffsetProperty, value); }
        }

        static void OnOffsetPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var ruler = sender as FootRuler;
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
            var ruler = sender as FootRuler;
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
            public DeferHelper(FootRuler ruler)
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

            private FootRuler _ruler;
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

        private void InitStepInfo(ref decimal currentStep, ref decimal miniStep, ref int miniStepCount)
        {
//             if (Scale == 1)
//             {
//                 currentStep = _baseStep;
//                 miniStep = _baseStep / 20;
//                 miniStepCount = 20;
// 
//                 return;
//             }

            decimal tempScale = Scale * 1.152m;
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
            var currentStepIndex = (stepIndex - Offset * Scale * 1152m);
            if (currentStepIndex % currentStep == 0)
            {
                var mark = Math.Round(currentStepIndex / Scale, 0);
                if (ignoreFirstMark && mark == 0)
                    return;

                dc.DrawLine(_pen, new Point((double)stepIndex + 0.5, ActualHeight), new Point((double)stepIndex + 0.5, 0));

                var ft = new FormattedText(
                        Math.Round(currentStepIndex / (Scale * 1152m), 3).ToString("#0.###"),
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

            for (var stepIndex = Offset * Scale * 1152m; stepIndex < (decimal)ActualWidth; stepIndex += miniStep)
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

            for (var stepIndex = Offset * Scale * 1152m; stepIndex >= 0; stepIndex -= miniStep)
            {
                if (stepIndex > (decimal)ActualWidth)
                    continue;

                DrawStep(dc, stepIndex, currentStep, miniStepCount, true);
            }
        }

        #endregion
    }
}
