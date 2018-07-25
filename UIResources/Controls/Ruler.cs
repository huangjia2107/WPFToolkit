using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;
using UIResources.Helps;

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

    public enum MarkDock
    {
        Up,
        Down
    }

    public class Ruler : FrameworkElement
    {
        private static readonly Type _typeofSelf = typeof(Ruler);
        private readonly DrawingGroup _drawingGroup = new DrawingGroup();

        private double _devicePixelUnit = 1;
        private Pen _markPen = null;
        private Pen _baselinePen = null;

        private int _deferLevel = 0;
        private bool _needRefresh = false; 

        public static readonly DependencyProperty MarkDockProperty =
            DependencyProperty.Register("MarkDock", typeof(MarkDock), _typeofSelf, new PropertyMetadata(MarkDock.Up, OnMarkDockPropertyChanged));
        public MarkDock MarkDock
        {
            get { return (MarkDock)GetValue(MarkDockProperty); }
            set { SetValue(MarkDockProperty, value); }
        }

        static void OnMarkDockPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var ruler = sender as Ruler;
            if (ruler._deferLevel == 0)
                ruler.Render();
            else
                ruler._needRefresh = true;
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
            else
                ruler._needRefresh = true;
        }

        public static readonly DependencyProperty ShiftProperty =
            DependencyProperty.Register("Shift", typeof(double), _typeofSelf, new PropertyMetadata(0d, OnShiftPropertyChanged));
        public double Shift
        {
            get { return (double)GetValue(ShiftProperty); }
            set { SetValue(ShiftProperty, value); }
        }

        static void OnShiftPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var ruler = sender as Ruler;
            if (ruler._deferLevel == 0)
                ruler.Render();
            else
                ruler._needRefresh = true;
        }

        public static readonly DependencyProperty ScaleProperty =
            DependencyProperty.Register("Scale", typeof(double), _typeofSelf, new FrameworkPropertyMetadata(1d, OnScalePropertyChanged));
        public double Scale
        {
            get { return (double)GetValue(ScaleProperty); }
            set { SetValue(ScaleProperty, value); }
        }
        static void OnScalePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var ruler = sender as Ruler;
            if (ruler._deferLevel == 0)
                ruler.Render();
            else
                ruler._needRefresh = true;
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            if (_markPen == null)
            {
                _devicePixelUnit = DpiUtil.GetDevicePixelUnit(this);
                _markPen = new Pen(new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF7F7F7F")), 1.0 / _devicePixelUnit);
                _markPen.Freeze();
            }

            if (_baselinePen == null)
            {
                _devicePixelUnit = DpiUtil.GetDevicePixelUnit(this);
                _baselinePen = new Pen(Brushes.Black, 1.0 / _devicePixelUnit);
                _baselinePen.Freeze();
            }

            Render();
            drawingContext.DrawDrawing(_drawingGroup);
        }

        #region DeferRefresh

        public IDisposable DeferRefresh()
        {
            ++_deferLevel;

            return new DeferRefresh(
                () =>
                {
                    --_deferLevel;

                    if (_deferLevel == 0 && _needRefresh)
                        Render();
                });
        }

        #endregion

        #region Private Func

        private double BaseLineOffset
        {
            get { return this.MarkDock == MarkDock.Up ? ActualHeight - 1 : 0d; }
        }

        private void Render()
        {
            var guidelineSet = new GuidelineSet();

            using (var dc = _drawingGroup.Open())
            {
                var mainStep = 0d;
                var miniStep = 0d;
                var miniStepCount = 0;

                InitStepInfo(ref mainStep, ref miniStep, ref miniStepCount);

                DrawOffsetRight(dc, miniStep, miniStepCount, guidelineSet);
                DrawOffsetLeft(dc, miniStep, miniStepCount, guidelineSet);

                dc.DrawLine(_baselinePen, new Point(0, BaseLineOffset), new Point(ActualWidth, BaseLineOffset));

                _needRefresh = false;
            }

            _drawingGroup.GuidelineSet = guidelineSet;
        }

        private int GetPrecision()
        {
            var result = 0;
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

        private double GetBaseStep()
        {
            var result = 0d;
            switch (Unit)
            {
                case RulerUnit.Pixel:
                    result = 1;
                    break;
                case RulerUnit.Inch:
                    result = 0.96;
                    break;
                case RulerUnit.Foot:
                    result = 1.152;
                    break;
                case RulerUnit.Millimeter:
                    result = 3.7795;
                    break;
                case RulerUnit.Centimeter:
                    result = 3.7795;
                    break;
            }

            return result;
        }

        private void InitStepInfo(ref double mainStep, ref double miniStep, ref int miniStepCount)
        {
            var tempScale = Scale * GetBaseStep();
            var tempStep = tempScale;

            while (true)
            {
                if (DoubleUtil.GreaterThanOrClose(tempStep / 4, 20))
                {
                    mainStep = tempStep;
                    miniStep = tempStep / 20;
                    miniStepCount = 20;

                    break;
                }

                if (DoubleUtil.GreaterThanOrClose(tempStep / 4, 10))
                {
                    mainStep = tempStep;
                    miniStep = tempStep / 10;
                    miniStepCount = 10;

                    break;
                }

                if (DoubleUtil.GreaterThanOrClose(tempStep / 5, 5))
                {
                    mainStep = tempStep;
                    miniStep = tempStep / 5;
                    miniStepCount = 5;

                    break;
                }

                if (DoubleUtil.LessThan(Scale, 0.1))
                {
                    if (DoubleUtil.AreClose(tempStep, tempScale))
                        tempStep = tempScale * 500;
                    else
                        tempStep += tempScale * 500;
                }
                else if (DoubleUtil.LessThan(Scale, 1))
                {
                    if (DoubleUtil.AreClose(tempStep, tempScale))
                        tempStep = tempScale * 50;
                    else
                        tempStep += tempScale * 50;
                }
                else if (DoubleUtil.LessThanOrClose(Scale, 10))
                {
                    if (DoubleUtil.AreClose(tempStep, tempScale))
                        tempStep = tempScale * 5;
                    else
                        tempStep += tempScale * 5;
                }
                else
                {
                    if (DoubleUtil.AreClose(tempStep, tempScale))
                        tempStep = tempScale * 2;
                    else
                        tempStep += tempScale * 2;
                }
            }
        }

        private FormattedText GetFormattedText(string textToFormat)
        {
            var ft = new FormattedText(
                       textToFormat,
                       CultureInfo.CurrentCulture,
                       FlowDirection.LeftToRight,
                       new Typeface("Arial"),
                       DpiUtil.PtToPixel(6),
                       Brushes.Black);
            ft.SetFontWeight(FontWeights.Regular);
            ft.TextAlignment = TextAlignment.Left;

            return ft;
        } 

        private void DrawStep(DrawingContext dc, int stepIndex, double stepOffset, int miniStepCount, GuidelineSet guidelineSet, bool ignoreFirstMark = false)
        { 
            if (stepIndex % miniStepCount == 0)
            {
                var mainstepOffset = stepOffset - Shift * Scale * DpiUtil.GetPixelPerUnit(Unit);
                var mark = Math.Round(mainstepOffset / (Scale * DpiUtil.GetPixelPerUnit(Unit)), GetPrecision());

                if (ignoreFirstMark && DoubleUtil.AreClose(mark, 0))
                    return;

                var ft = GetFormattedText(mark.ToString("#0.###"));

                DrawLine(dc, new Point(stepOffset, 0), new Point(stepOffset, BaseLineOffset), guidelineSet);
                dc.DrawText(ft, new Point(stepOffset + 1, MarkDock == MarkDock.Up ? 0 : ActualHeight - ft.Height));
            }
            else
            {
                if (miniStepCount == 5)
                {
                    DrawLine(dc, new Point(stepOffset, ActualHeight * 1 / 2), new Point(stepOffset, BaseLineOffset), guidelineSet);
                }

                if (miniStepCount == 10)
                {
                    if (stepIndex % 5 == 0)
                        DrawLine(dc, new Point(stepOffset, ActualHeight * (MarkDock == MarkDock.Up ? 1 / 5d : 4 / 5d)), new Point(stepOffset, BaseLineOffset), guidelineSet);
                    else if (stepIndex % 2 == 0)
                        DrawLine(dc, new Point(stepOffset, ActualHeight * 1 / 2), new Point(stepOffset, BaseLineOffset), guidelineSet);
                    else
                        DrawLine(dc, new Point(stepOffset, ActualHeight * (MarkDock == MarkDock.Up ? 5 / 8d : 3 / 8d)), new Point(stepOffset, BaseLineOffset), guidelineSet);
                }

                if (miniStepCount == 20)
                {
                    if (stepIndex % 10 == 0)
                        DrawLine(dc, new Point(stepOffset, ActualHeight * (MarkDock == MarkDock.Up ? 1 / 5d : 4 / 5d)), new Point(stepOffset, BaseLineOffset), guidelineSet);
                    else if (stepIndex % 5 == 0)
                        DrawLine(dc, new Point(stepOffset, ActualHeight * 1 / 2), new Point(stepOffset, BaseLineOffset), guidelineSet);
                    else if (stepIndex % 2 == 0)
                        DrawLine(dc, new Point(stepOffset, ActualHeight * (MarkDock == MarkDock.Up ? 5 / 8d : 3 / 8d)), new Point(stepOffset, BaseLineOffset), guidelineSet);
                    else
                        DrawLine(dc, new Point(stepOffset, ActualHeight * (MarkDock == MarkDock.Up ? 23 / 32d : 9 / 32d)), new Point(stepOffset, BaseLineOffset), guidelineSet);
                }
            }
        }

        private void DrawLine(DrawingContext dc, Point startPoint, Point endPoint, GuidelineSet guidelineSet)
        {
            dc.DrawLine(_markPen, startPoint, endPoint);

            guidelineSet.GuidelinesX.Add(startPoint.X + 0.5);
            if(!guidelineSet.GuidelinesY.Contains(startPoint.Y))
                guidelineSet.GuidelinesY.Add(startPoint.Y);

            guidelineSet.GuidelinesX.Add(endPoint.X + 0.5);
            if(!guidelineSet.GuidelinesY.Contains(endPoint.Y-0.5))
                guidelineSet.GuidelinesY.Add(endPoint.Y-0.5);
        }

        private void DrawOffsetRight(DrawingContext dc, double miniStep, int miniStepCount, GuidelineSet guidelineSet)
        {
            var realShift = Shift * Scale * DpiUtil.GetPixelPerUnit(Unit);
            if (DoubleUtil.GreaterThanOrClose(realShift, ActualWidth))
                return;

            int stepIndex = 0;
            for (var stepOffset = realShift; stepOffset < ActualWidth; stepOffset += miniStep)
            {
                if (DoubleUtil.LessThan(stepOffset, 0))
                    continue;

                DrawStep(dc, stepIndex, stepOffset, miniStepCount, guidelineSet);
                stepIndex++;
            }
        }

        private void DrawOffsetLeft(DrawingContext dc, double miniStep, int miniStepCount, GuidelineSet guidelineSet)
        {
            if (DoubleUtil.LessThanOrClose(Shift, 0))
                return;

            int stepIndex = 0;
            for (var stepOffset = Shift * Scale * DpiUtil.GetPixelPerUnit(Unit); stepOffset >= 0; stepOffset -= miniStep)
            {
                if (DoubleUtil.GreaterThan(stepOffset, ActualWidth))
                    continue;

                DrawStep(dc, stepIndex, stepOffset, miniStepCount, guidelineSet, true);
                stepIndex++;
            }
        }

        #endregion
    }
}
