using System;
using System.Collections.Generic;
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
            DependencyProperty.Register("Scale", typeof(double), _typeofSelf, new FrameworkPropertyMetadata(1d, FrameworkPropertyMetadataOptions.AffectsRender));
        public double Scale
        {
            get { return (double)GetValue(ScaleProperty); }
            set { SetValue(ScaleProperty, value); }
        }


        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            Draw(drawingContext);
        }

        private void GetMinStepDivide(ref double miniStep, ref double divideCount)
        {
            if (DoubleUtil.GreaterThanOrClose(_step / 20, 3))
            {
                miniStep = _step / 20;
                divideCount = 20;
            }


            if (DoubleUtil.GreaterThanOrClose(_step / 10, 3))
            {
                miniStep = _step / 10;
                divideCount = 10;
            }

            if (DoubleUtil.GreaterThanOrClose(_step / 5, 3))
            {
                miniStep = _step / 5;
                divideCount = 5;
            }

            miniStep = _step / 5;
            divideCount = 5;
        }

        private void Draw(DrawingContext dc)
        {
            double miniStep = 0;
            double divideCount = 0;
            GetMinStepDivide(ref miniStep, ref divideCount);

            dc.DrawLine(_pen, new Point(-0.5, ActualHeight), new Point(ActualWidth, ActualHeight));
            for (double markIndex = 0; markIndex < ActualWidth; markIndex += miniStep)
            {
                if (DoubleUtil.AreClose(markIndex % _step, 0))
                    dc.DrawLine(_pen, new Point(markIndex, ActualHeight), new Point(markIndex, 0));
                else if (DoubleUtil.AreClose(markIndex % (_step / 2), 0))
                    dc.DrawLine(_pen, new Point(markIndex, ActualHeight * 1 / 5), new Point(markIndex, ActualHeight));
                else if (DoubleUtil.AreClose(markIndex % (_step / 4), 0))
                    dc.DrawLine(_pen, new Point(markIndex, ActualHeight * 1 / 2), new Point(markIndex, ActualHeight));
                else if (DoubleUtil.AreClose(markIndex % (_step / 10), 0))
                    dc.DrawLine(_pen, new Point(markIndex, ActualHeight * 5 / 8), new Point(markIndex, ActualHeight));
                else
                    dc.DrawLine(_pen, new Point(markIndex, ActualHeight * 23 / 32), new Point(markIndex, ActualHeight));
            }
        }
    }
}
