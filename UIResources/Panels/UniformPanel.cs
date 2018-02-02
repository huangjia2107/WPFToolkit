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
    public class UniformPanel : Panel
    {
        int _nonCollapsedCount = 0;
        int _realColumns = 0;
        int _realRows = 0;
        int _realFirstColumn = 0;

        Size _spaceSize;

        public static readonly DependencyProperty PaddingProperty = DependencyProperty.Register("Padding", typeof(Thickness), typeof(UniformPanel),
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

        public static readonly DependencyProperty ItemSpaceProperty = DependencyProperty.Register("ItemSpace", typeof(Size), typeof(UniformPanel),
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

        static bool ValidateRowsOrColumns(object value)
        {
            return (int)value >= 0;
        }

        public static readonly DependencyProperty RowsProperty = DependencyProperty.Register("Rows", typeof(int), typeof(UniformPanel),
            new FrameworkPropertyMetadata(
                0,
                FrameworkPropertyMetadataOptions.AffectsMeasure),
                ValidateRowsOrColumns);
        public int Rows
        {
            get { return (int)GetValue(RowsProperty); }
            set { SetValue(RowsProperty, value); }
        }

        public static readonly DependencyProperty ColumnsProperty = DependencyProperty.Register("Columns", typeof(int), typeof(UniformPanel),
            new FrameworkPropertyMetadata(
                0,
                FrameworkPropertyMetadataOptions.AffectsMeasure),
                ValidateRowsOrColumns);
        public int Columns
        {
            get { return (int)GetValue(ColumnsProperty); }
            set { SetValue(ColumnsProperty, value); }
        }

        public static readonly DependencyProperty FirstColumnProperty = DependencyProperty.Register("FirstColumn", typeof(int), typeof(UniformPanel),
            new FrameworkPropertyMetadata(0, OnFirstColumnChanged), ValidateRowsOrColumns);
        public int FirstColumn
        {
            get { return (int)GetValue(FirstColumnProperty); }
            set { SetValue(FirstColumnProperty, value); }
        }

        static void OnFirstColumnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var panel = (d as UniformPanel);

            if (panel.Columns > 0)
                panel.UpdateChildLayout();
        }

        private void UpdateChildLayout()
        {
            InvalidateMeasure();
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            UpdateComputedValues();

            if (_nonCollapsedCount == 0)
                return base.MeasureOverride(availableSize);

            _spaceSize = GetSpaceSize(_nonCollapsedCount);

            var maxChildDesiredWidth = 0d;
            var maxChildDesiredHeight = 0d;
            var childConstraint = new Size(Math.Max(availableSize.Width - _spaceSize.Width, 0) / _realColumns, Math.Max(availableSize.Height - _spaceSize.Height, 0) / _realRows);

            foreach (UIElement child in InternalChildren)
            {
                if (child.Visibility == Visibility.Collapsed)
                    continue;

                // Measure the child.
                child.Measure(childConstraint);
                var childDesiredSize = child.DesiredSize;

                maxChildDesiredWidth = Math.Max(maxChildDesiredWidth, childDesiredSize.Width);
                maxChildDesiredHeight = Math.Max(maxChildDesiredHeight, childDesiredSize.Height);
            }

            return new Size(maxChildDesiredWidth * _realColumns + _spaceSize.Width, maxChildDesiredHeight * _realRows + _spaceSize.Height);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            if (_nonCollapsedCount == 0)
                return base.ArrangeOverride(finalSize);

            var childBounds = new Rect(0, 0, Math.Max(finalSize.Width - _spaceSize.Width, 0) / _realColumns, Math.Max(finalSize.Height - _spaceSize.Height, 0) / _realRows);
            var realIndex = _realFirstColumn;

            for (var index = 0; index < InternalChildren.Count; index++)
            {
                var child = InternalChildren[index];
                if (child.Visibility == Visibility.Collapsed)
                    continue;

                var columnIndex = realIndex % _realColumns;
                var rowIndex = (uint)Math.Floor((double)realIndex / _realColumns);

                childBounds.X = columnIndex * (childBounds.Width + ItemSpace.Width) + Padding.Left;
                childBounds.Y = rowIndex * (childBounds.Height + ItemSpace.Height) + Padding.Top;

                child.Arrange(childBounds);
                realIndex++;
            }

            return new Size(childBounds.Width * _realColumns + _spaceSize.Width, childBounds.Height * _realRows + _spaceSize.Height);
        }

        private Size GetSpaceSize(int nonCollapsedCount)
        {
            if (nonCollapsedCount == 0 || nonCollapsedCount == 1)
                return new Size(Padding.Left + Padding.Right, Padding.Top + Padding.Bottom);

            return new Size
            {
                Width = (_realColumns - 1) * ItemSpace.Width + Padding.Left + Padding.Right,
                Height = (_realRows - 1) * ItemSpace.Height + Padding.Top + Padding.Bottom
            };
        }

        //当_RealChildCount ==0 则_Columns与_Rows无意义
        private void UpdateComputedValues()
        {
            _realColumns = Columns;
            _realRows = Rows;
            _realFirstColumn = FirstColumn;
            _nonCollapsedCount = 0;

            foreach (UIElement child in InternalChildren)
            {
                if (child.Visibility != Visibility.Collapsed)
                    _nonCollapsedCount++;
            }

            if (_nonCollapsedCount == 0)
                return;

            //parameter checking. 
            if (_realFirstColumn >= _realColumns)
                _realFirstColumn = 0;

            if (_realRows == 0 && _realColumns == 0)
            {
                _realColumns = (int)Math.Sqrt(_nonCollapsedCount);
                if ((_realColumns * _realColumns) < _nonCollapsedCount)
                    _realColumns++;

                _realRows = (_nonCollapsedCount + (_realColumns - 1)) / _realColumns;
            }

            if (_realRows == 0 && _realColumns > 0)
                _realRows = (_realFirstColumn + _nonCollapsedCount + (_realColumns - 1)) / _realColumns;

            if (_realRows > 0 && _realColumns == 0)
                _realColumns = (_nonCollapsedCount + (_realRows - 1)) / _realRows;

            if (_realRows > 0 && _realColumns > 0)
                _realRows = Math.Max(_realRows, (_realFirstColumn + _nonCollapsedCount + (_realColumns - 1)) / _realColumns);

            /*
             * 
             * for (int index = 0; index < realChildCount; index++)
                    {
                        if (index == 0)
                            realColumns = realRows = 1;
                        else
                        {
                            if (index == realColumns * realRows)
                            {
                                if (realColumns == realRows)
                                    realColumns++;
                                else
                                    realRows++;
                            }
                        }
                    }
             * 
             *  
             */
        }
    }
}
