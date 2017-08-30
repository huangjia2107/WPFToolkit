using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using UIResources.Helps;

namespace UIResources.Panels
{
    public class FlowPanel : Panel
    {
        private static readonly Type _typeofSelf = typeof(FlowPanel);

        private Dictionary<int, double> _columnIndexToHeightMap = new Dictionary<int, double>();
        private Dictionary<int, ChildRect> _childIndexToRectMap = new Dictionary<int, ChildRect>();

        struct ChildRect
        {
            public int ColumnIndex { get; set; }

            public double X { get; set; }
            public double Y { get; set; }

            public double Width { get; set; }
            public double Height { get; set; }
        }

        #region Properties

        public static readonly DependencyProperty ColumnsProperty = DependencyProperty.Register("Columns", typeof(int), _typeofSelf,
           new FrameworkPropertyMetadata(1, FrameworkPropertyMetadataOptions.AffectsMeasure, null, CoerceColumnValue));
        public int Columns
        {
            get { return (int)GetValue(ColumnsProperty); }
            set { SetValue(ColumnsProperty, value); }
        }

        static object CoerceColumnValue(DependencyObject d, object value)
        {
            return Math.Max(1, (int)value);
        }

        public static readonly DependencyProperty PaddingProperty = DependencyProperty.Register("Padding", typeof(Thickness), _typeofSelf,
            new FrameworkPropertyMetadata(new Thickness(), FrameworkPropertyMetadataOptions.AffectsMeasure), IsThicknessValid);
        public Thickness Padding
        {
            get { return (Thickness)GetValue(PaddingProperty); }
            set { SetValue(PaddingProperty, value); }
        }
        static bool IsThicknessValid(object value)
        {
            return ((Thickness)value).IsValid(false, false, false, false);
        }

        public static readonly DependencyProperty ItemSpaceProperty = DependencyProperty.Register("ItemSpace", typeof(Size), _typeofSelf,
            new FrameworkPropertyMetadata(new Size(), FrameworkPropertyMetadataOptions.AffectsMeasure, null, CoerceItemSpaceValue));
        public Size ItemSpace
        {
            get { return (Size)GetValue(ItemSpaceProperty); }
            set { SetValue(ItemSpaceProperty, value); }
        }

        static object CoerceItemSpaceValue(DependencyObject d, object value)
        {
            var itemSpace = (Size)value;

            if (itemSpace.Width < 0)
                itemSpace.Width = 0;

            if (itemSpace.Height < 0)
                itemSpace.Height = 0;

            return itemSpace;
        }

        #endregion

        #region Override

        protected override Size MeasureOverride(Size availableSize)
        {
            ConstructColumnIndexToHeightMap();
            _childIndexToRectMap.Clear();

            double horizontalSpace = Padding.Left + Padding.Right + (Columns - 1) * ItemSpace.Width;
            double childBoundsWidth = Math.Max(availableSize.Width - horizontalSpace, 0) / Columns;

            int columnIndex = 0;
            int realIndex = 0;

            for (int index = 0; index < InternalChildren.Count; index++)
            {
                var child = InternalChildren[index];
                if (child.Visibility == Visibility.Collapsed)
                    continue;

                // Measure the child.
                child.Measure(availableSize);

                columnIndex = GetColumnIndexWithFirstMinHeight();
                _childIndexToRectMap.Add(realIndex,
                    new ChildRect
                    {
                        ColumnIndex = columnIndex,
                        X = columnIndex * (childBoundsWidth + ItemSpace.Width) + Padding.Left,
                        Y = _columnIndexToHeightMap[columnIndex],
                        Width = childBoundsWidth,
                        Height = child.DesiredSize.Height
                    });

                _columnIndexToHeightMap[columnIndex] += (child.DesiredSize.Height + ItemSpace.Height);
                realIndex++;
            }

            return new Size(
                availableSize.Width,
                _childIndexToRectMap.Values.GroupBy(childRect => childRect.ColumnIndex).Max(columnIndexToRect => columnIndexToRect.Sum(rect => rect.Height) + ItemSpace.Height * (columnIndexToRect.Count() - 1) + Padding.Top + Padding.Bottom)
                );
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            Rect childBounds = new Rect();
            int realIndex = 0;

            for (int index = 0; index < InternalChildren.Count; index++)
            {
                var child = InternalChildren[index];
                if (child.Visibility == Visibility.Collapsed)
                    continue;

                childBounds.X = _childIndexToRectMap[realIndex].X;
                childBounds.Y = _childIndexToRectMap[realIndex].Y;
                childBounds.Width = _childIndexToRectMap[realIndex].Width;
                childBounds.Height = _childIndexToRectMap[realIndex].Height;

                child.Arrange(childBounds);
                realIndex++;
            }                              

            return finalSize;
        }

        #endregion

        #region Func

        private int GetColumnIndexWithFirstMinHeight()
        {
            int columnIndex = -1;
            double minHeight = 0;
            foreach (var map in _columnIndexToHeightMap)
            {
                if (columnIndex < 0)
                {
                    columnIndex = map.Key;
                    minHeight = map.Value;
                    continue;
                }

                if (DoubleUtil.LessThan(map.Value, minHeight))
                {
                    columnIndex = map.Key;
                    minHeight = map.Value;
                }
            }

            return Math.Max(0, columnIndex);
        }

        private void ConstructColumnIndexToHeightMap()
        {
            _columnIndexToHeightMap.Clear();

            for (int columnIndex = 0; columnIndex < Columns; columnIndex++)
            {
                if (!_columnIndexToHeightMap.ContainsKey(columnIndex))
                    _columnIndexToHeightMap.Add(columnIndex, Padding.Top);
                else
                    _columnIndexToHeightMap[columnIndex] = Padding.Top;
            }
        }

        #endregion
    }
}
