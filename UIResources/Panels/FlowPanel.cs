﻿using System;
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

        int _realChildCount = 0;
        Size _spaceSize;

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


        Dictionary<int, ColumnInfo> _columnIndexMap = new Dictionary<int, ColumnInfo>();
        Dictionary<int, double> _columnIndexToPositionMap = new Dictionary<int, double>();

        class ColumnInfo
        {
            public int ChildCount { get; set; }
            public double ColumnHeight { get; set; }


            public double DesiredHeight(double spaceHeight, double paddingHeight)
            {
                return spaceHeight * (ChildCount - 1) + paddingHeight;
            }

            public void Reset()
            {
                ChildCount = 0;
                ColumnHeight = 0;
            }
        }

        #region Override

        protected override Size MeasureOverride(Size availableSize)
        {
            ConstructColumnIndexMap();

            int realIndex = 0;
            for (int index = 0; index < InternalChildren.Count; index++)
            {
                var child = InternalChildren[index];
                if (child.Visibility == Visibility.Collapsed)
                    continue;

                // Measure the child.
                child.Measure(availableSize);

                _columnIndexMap[realIndex % Columns].ChildCount++;
                _columnIndexMap[realIndex % Columns].ColumnHeight += child.DesiredSize.Height;

                realIndex++;
            }

            return new Size(availableSize.Width, _columnIndexMap.Values.Max(c => c.DesiredHeight(ItemSpace.Height, Padding.Top + Padding.Bottom)));
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            ConstructColumnIndexToPositionMap();

            double horizontalSpace = Padding.Left + Padding.Right + (Columns - 1) * ItemSpace.Width;

            Rect childBounds = new Rect(0, 0, (finalSize.Width - horizontalSpace) / Columns, 0);
            int realIndex = 0;

            for (int index = 0; index < InternalChildren.Count; index++)
            {
                var child = InternalChildren[index];
                if (child.Visibility == Visibility.Collapsed)
                    continue;

                int columnIndex = realIndex % Columns;

                childBounds.X = columnIndex * (childBounds.Width + ItemSpace.Width) + Padding.Left;
                childBounds.Y = _columnIndexToPositionMap[columnIndex];

                childBounds.Height = finalSize.Height - _columnIndexToPositionMap[columnIndex] - Padding.Bottom;

                child.Arrange(childBounds);

                _columnIndexToPositionMap[columnIndex] += child.RenderSize.Height + ItemSpace.Height;
                realIndex++;
            }

            return finalSize;
        }

        #endregion

        #region Func

        private void ConstructColumnIndexMap()
        {
            _columnIndexMap.Clear();

            for (int columnIndex = 0; columnIndex < Columns; columnIndex++)
            {
                if (!_columnIndexMap.ContainsKey(columnIndex))
                    _columnIndexMap.Add(columnIndex, new ColumnInfo());
                else
                    _columnIndexMap[columnIndex].Reset();
            }
        }

        private void ConstructColumnIndexToPositionMap()
        {
            _columnIndexToPositionMap.Clear();

            for (int columnIndex = 0; columnIndex < Columns; columnIndex++)
            {
                if (!_columnIndexToPositionMap.ContainsKey(columnIndex))
                    _columnIndexToPositionMap.Add(columnIndex, Padding.Top);
                else
                    _columnIndexToPositionMap[columnIndex] = Padding.Top;
            }
        }     

        #endregion
    }
}
