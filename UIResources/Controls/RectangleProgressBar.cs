using System;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Shapes;

namespace UIResources.Controls
{
    [TemplatePart(Name = PART_Path, Type = typeof(Path))]
    public class RectangleProgressBar : RangeBase
    {
        private const string PART_Path = "PART_Path";
        private Path path = null;

        static RectangleProgressBar()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RectangleProgressBar), new FrameworkPropertyMetadata(typeof(RectangleProgressBar)));

            MinimumProperty.OverrideMetadata(typeof(RectangleProgressBar), new FrameworkPropertyMetadata(0.0d));
            MaximumProperty.OverrideMetadata(typeof(RectangleProgressBar), new FrameworkPropertyMetadata(100.0d));
            ValueProperty.OverrideMetadata(typeof(RectangleProgressBar), new FrameworkPropertyMetadata(0d));
        }

        public RectangleProgressBar()
            : base()
        {
            WeakEventManager<FrameworkElement, RoutedEventArgs>.AddHandler(this, "Unloaded", OnUnloaded);
            WeakEventManager<FrameworkElement, SizeChangedEventArgs>.AddHandler(this, "SizeChanged", OnSizeChanged); 
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateUI();
        }

        private void OnUnloaded(object sender, EventArgs e)
        { 
            WeakEventManager<FrameworkElement, RoutedEventArgs>.RemoveHandler(this, "Unloaded", OnUnloaded);
            WeakEventManager<FrameworkElement, SizeChangedEventArgs>.RemoveHandler(this, "SizeChanged", OnSizeChanged);
        }

        public new double BorderThickness
        {
            get { return (double)GetValue(BorderThicknessProperty); }
            set { SetValue(BorderThicknessProperty, value); }
        }

        public new static readonly DependencyProperty BorderThicknessProperty =
            DependencyProperty.Register("BorderThickness", typeof(double), typeof(RectangleProgressBar), new PropertyMetadata(10d));

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            path = base.GetTemplateChild(PART_Path) as Path;
        }

        protected override void OnMaximumChanged(double oldMaximum, double newMaximum)
        {
            base.OnMaximumChanged(oldMaximum, newMaximum);
            UpdateUI();
        }

        protected override void OnMinimumChanged(double oldMinimum, double newMinimum)
        {
            base.OnMinimumChanged(oldMinimum, newMinimum);
            UpdateUI();
        }

        protected override void OnValueChanged(double oldValue, double newValue)
        {
            base.OnValueChanged(oldValue, newValue);
            UpdateUI();
        }

        private void UpdateUI()
        {
            if (path == null)
                return;

            double circumference = (this.ActualHeight + this.ActualWidth) * 2;
            double drawLength = circumference * Value / (Maximum - Minimum);

            PathSegmentCollection outSideSegmentCollection = new PathSegmentCollection();
            PathSegmentCollection inSideSegmentCollection = new PathSegmentCollection();

            DrawStartToEnd(outSideSegmentCollection, inSideSegmentCollection, circumference, drawLength);

            for (int index = inSideSegmentCollection.Count - 1; index >= 0; index--)
            {
                outSideSegmentCollection.Add(inSideSegmentCollection[index]);
            }

            outSideSegmentCollection.Add(new LineSegment { Point = new Point(0, BorderThickness) });
            path.Data = new PathGeometry()
            {
                Figures = new PathFigureCollection()
                {
                    new PathFigure()
                    {
                        //确定起点（外弧线左侧点）
                        StartPoint = new Point(0, 0),
                        Segments = outSideSegmentCollection,
                        IsClosed = true
                    }
                }
            };
        }

        private void DrawStartToEnd(PathSegmentCollection outSideSegmentCollection, PathSegmentCollection inSideSegmentCollection, double circumference, double drawLength)
        {
            Point outSideEndPoint;
            Point inSideEndPoint;
            if (drawLength > this.ActualWidth)
            {
                outSideSegmentCollection.Add(new LineSegment { Point = new Point(this.ActualWidth, 0) });
                inSideSegmentCollection.Add(new LineSegment { Point = new Point(this.ActualWidth - BorderThickness, BorderThickness) });

                if (drawLength > this.ActualWidth + this.ActualHeight)
                {
                    outSideSegmentCollection.Add(new LineSegment() { Point = new Point(this.ActualWidth, this.ActualHeight) });
                    inSideSegmentCollection.Add(new LineSegment { Point = new Point(this.ActualWidth - BorderThickness, this.ActualHeight - BorderThickness) });

                    if (drawLength > this.ActualWidth * 2 + this.ActualHeight)
                    {
                        outSideSegmentCollection.Add(new LineSegment() { Point = new Point(0, this.ActualHeight) });
                        outSideEndPoint = new Point(0, Math.Max(BorderThickness, circumference - drawLength));

                        inSideSegmentCollection.Add(new LineSegment { Point = new Point(BorderThickness, this.ActualHeight - BorderThickness) });
                        inSideEndPoint = new Point(BorderThickness, Math.Min(this.ActualHeight - BorderThickness, Math.Max(BorderThickness, circumference - drawLength)));
                    }
                    else
                    {
                        outSideEndPoint = new Point(circumference - drawLength - this.ActualHeight, this.ActualHeight);
                        inSideEndPoint = new Point(Math.Min(this.ActualWidth - BorderThickness, Math.Max(BorderThickness, circumference - drawLength - this.ActualHeight)), this.ActualHeight - BorderThickness);
                    }
                }
                else
                {
                    outSideEndPoint = new Point(this.ActualWidth, drawLength - this.ActualWidth);
                    inSideEndPoint = new Point(this.ActualWidth - BorderThickness, Math.Min(this.ActualHeight - BorderThickness, Math.Max(BorderThickness, drawLength - this.ActualWidth)));
                }
            }
            else
            {
                outSideEndPoint = new Point(drawLength, 0);
                inSideEndPoint = new Point(Math.Min(this.ActualWidth - BorderThickness, drawLength), BorderThickness);
            }

            outSideSegmentCollection.Add(new LineSegment() { Point = outSideEndPoint });
            inSideSegmentCollection.Add(new LineSegment() { Point = inSideEndPoint });
        }
    }
}

