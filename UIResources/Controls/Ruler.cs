using System;
using System.Globalization;
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

        public Ruler()
        {
            RenderOptions.SetEdgeMode(this, EdgeMode.Aliased);
        }

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
            else
                ruler._needRefresh = true;
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

			if(_baselinePen == null)
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
            get { return this.MarkDock == MarkDock.Up ? ActualHeight : 0d; }
        }

        private void Render()
        {
            using (var dc = _drawingGroup.Open())
            {
                decimal currentStep = 0;
                decimal miniStep = 0;
                var miniStepCount = 0;
                InitStepInfo(ref currentStep, ref miniStep, ref miniStepCount);

                DrawOffsetRight(dc, currentStep, miniStep, miniStepCount);
                DrawOffsetLeft(dc, currentStep, miniStep, miniStepCount);
				
				dc.DrawLine(_baselinePen, new Point(0, BaseLineOffset), new Point(ActualWidth, BaseLineOffset));

                _needRefresh = false;
            }
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
            var tempScale = Scale * GetBaseStep();
            var tempStep = tempScale;

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

        private void DrawStep(DrawingContext dc, decimal stepIndex, decimal currentStep, int miniStepCount, bool ignoreFirstMark = false)
        {
            var currentStepIndex = (stepIndex - Shift * Scale * DpiUtil.GetPixelPerUnit(Unit));
            if (currentStepIndex % currentStep == 0)
            {
                var mark = Math.Round(currentStepIndex / (Scale * DpiUtil.GetPixelPerUnit(Unit)), GetPrecision());
                if (ignoreFirstMark && mark == 0)
                    return;

                var ft = GetFormattedText(mark.ToString("#0.###"));

                dc.DrawLine(_markPen, new Point((double)stepIndex + 0.5, ActualHeight), new Point((double)stepIndex + 0.5, 0));
                dc.DrawText(ft, new Point((double)stepIndex + 1.5, MarkDock == MarkDock.Up ? 0 : ActualHeight - ft.Height));
            }
            else
            {
                if (miniStepCount == 5)
                {
                    if (currentStepIndex % (currentStep / 5) == 0)
                        dc.DrawLine(_markPen, new Point((double)stepIndex + 0.5, ActualHeight * 1 / 2), new Point((double)stepIndex + 0.5, BaseLineOffset));
                }

                if (miniStepCount == 10)
                {
                    if (currentStepIndex % (currentStep / 2) == 0)
                        dc.DrawLine(_markPen, new Point((double)stepIndex + 0.5, ActualHeight * (MarkDock == MarkDock.Up ? 1 / 5d : 4 / 5d)), new Point((double)stepIndex + 0.5, BaseLineOffset));
                    else if (currentStepIndex % (currentStep / 5) == 0)
                        dc.DrawLine(_markPen, new Point((double)stepIndex + 0.5, ActualHeight * 1 / 2), new Point((double)stepIndex + 0.5, BaseLineOffset));
                    else
                        dc.DrawLine(_markPen, new Point((double)stepIndex + 0.5, ActualHeight * (MarkDock == MarkDock.Up ? 5 / 8d : 3 / 8d)), new Point((double)stepIndex + 0.5, BaseLineOffset));
                }

                if (miniStepCount == 20)
                {
                    if (currentStepIndex % (currentStep / 2) == 0)
                        dc.DrawLine(_markPen, new Point((double)stepIndex + 0.5, ActualHeight * (MarkDock == MarkDock.Up ? 1 / 5d : 4 / 5d)), new Point((double)stepIndex + 0.5, BaseLineOffset));
                    else if (currentStepIndex % (currentStep / 4) == 0)
                        dc.DrawLine(_markPen, new Point((double)stepIndex + 0.5, ActualHeight * 1 / 2), new Point((double)stepIndex + 0.5, BaseLineOffset));
                    else if (currentStepIndex % (currentStep / 10) == 0)
                        dc.DrawLine(_markPen, new Point((double)stepIndex + 0.5, ActualHeight * (MarkDock == MarkDock.Up ? 5 / 8d : 3 / 8d)), new Point((double)stepIndex + 0.5, BaseLineOffset));
                    else
                        dc.DrawLine(_markPen, new Point((double)stepIndex + 0.5, ActualHeight * (MarkDock == MarkDock.Up ? 23 / 32d : 9 / 32d)), new Point((double)stepIndex + 0.5, BaseLineOffset));
                }
            }
        }

        private void DrawOffsetRight(DrawingContext dc, decimal currentStep, decimal miniStep, int miniStepCount)
        {
            if (Shift >= (decimal)ActualWidth)
                return;

            for (var stepIndex = Shift * Scale * DpiUtil.GetPixelPerUnit(Unit); stepIndex < (decimal)ActualWidth; stepIndex += miniStep)
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

            for (var stepIndex = Shift * Scale * DpiUtil.GetPixelPerUnit(Unit); stepIndex >= 0; stepIndex -= miniStep)
            {
                if (stepIndex > (decimal)ActualWidth)
                    continue;

                DrawStep(dc, stepIndex, currentStep, miniStepCount, true);
            }
        }

        #endregion
    }
}
