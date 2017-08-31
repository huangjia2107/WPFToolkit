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
    public enum DivideMode
    {
        Auto,
        Row,
        Column,
        Both
    }

    public class UniformPanel : Panel
    {
        uint _realChildCount = 0;
        uint _realColumns = 1;
        uint _realRows = 1;

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

        public static readonly DependencyProperty DivideModeProperty = DependencyProperty.Register("DivideMode", typeof(DivideMode), typeof(UniformPanel),
            new FrameworkPropertyMetadata(DivideMode.Auto, DivideModePropertyChanged));
        public DivideMode DivideMode
        {
            get { return (DivideMode)GetValue(DivideModeProperty); }
            set { SetValue(DivideModeProperty, value); }
        }

        static void DivideModePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as UniformPanel).UpdateChildLayout();
        }

        static object CoerceRowAndColumnValue(DependencyObject d, object value)
        {
            return Math.Max(1, (uint)value);
        }

        public static readonly DependencyProperty RowsProperty = DependencyProperty.Register("Rows", typeof(uint), typeof(UniformPanel),
            new FrameworkPropertyMetadata(
                1u,
                RowsPropertyChanged,
                CoerceRowAndColumnValue));
        public uint Rows
        {
            get { return (uint)GetValue(RowsProperty); }
            set { SetValue(RowsProperty, value); }
        }

        static void RowsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            UniformPanel panel = (d as UniformPanel);
            if (panel.DivideMode != DivideMode.Column)
                panel.UpdateChildLayout();
        }

        public static readonly DependencyProperty ColumnsProperty = DependencyProperty.Register("Columns", typeof(uint), typeof(UniformPanel),
            new FrameworkPropertyMetadata(
                1u,
                ColumnsPropertyChanged,
                CoerceRowAndColumnValue));
        public uint Columns
        {
            get { return (uint)GetValue(ColumnsProperty); }
            set { SetValue(ColumnsProperty, value); }
        }

        static void ColumnsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            UniformPanel panel = (d as UniformPanel);
            if (panel.DivideMode != DivideMode.Row)
                panel.UpdateChildLayout();
        }

        private void UpdateChildLayout()
        {
            InvalidateMeasure();
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            DivideRowAndColumn(DivideMode, out _realChildCount, out _realColumns, out _realRows);

            if (_realChildCount == 0)
                return base.MeasureOverride(availableSize);

            _spaceSize = GetSpaceSize(_realChildCount);

            double maxChildDesiredWidth = 0.0;
            double maxChildDesiredHeight = 0.0;
            Size childConstraint = new Size(Math.Max(availableSize.Width - _spaceSize.Width, 0) / _realColumns, Math.Max(availableSize.Height - _spaceSize.Height, 0) / _realRows);

            foreach (UIElement child in InternalChildren)
            {
                if (child.Visibility == Visibility.Collapsed)
                    continue;

                // Measure the child.
                child.Measure(childConstraint);
                Size childDesiredSize = child.DesiredSize;

                maxChildDesiredWidth = Math.Max(maxChildDesiredWidth, childDesiredSize.Width);
                maxChildDesiredHeight = Math.Max(maxChildDesiredHeight, childDesiredSize.Height);
            }

            return new Size(maxChildDesiredWidth * _realColumns + _spaceSize.Width, maxChildDesiredHeight * _realRows + _spaceSize.Height);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            if (_realChildCount == 0)
                return base.ArrangeOverride(finalSize);

            Rect childBounds = new Rect(0, 0, Math.Max(finalSize.Width - _spaceSize.Width, 0) / _realColumns, Math.Max(finalSize.Height - _spaceSize.Height, 0) / _realRows);
            uint realIndex = 0;

            for (int index = 0; index < InternalChildren.Count; index++)
            {
                var child = InternalChildren[index];
                if (child.Visibility == Visibility.Collapsed)
                    continue;

                uint columnIndex = realIndex % _realColumns;
                uint rowIndex = (uint)Math.Floor((double)realIndex / _realColumns);

                childBounds.X = columnIndex * (childBounds.Width + ItemSpace.Width) + Padding.Left;
                childBounds.Y = rowIndex * (childBounds.Height + ItemSpace.Height) + Padding.Top;

                child.Arrange(childBounds);
                realIndex++;
            }

            return new Size(childBounds.Width * _realColumns + _spaceSize.Width, childBounds.Height * _realRows + _spaceSize.Height);
        }

        private Size GetSpaceSize(uint realChildCount)
        {
            if (realChildCount == 0 || realChildCount == 1)
                return new Size(Padding.Left + Padding.Right, Padding.Top + Padding.Bottom);

            return new Size
            {
                Width = (_realColumns - 1) * ItemSpace.Width + Padding.Left + Padding.Right,
                Height = (_realRows - 1) * ItemSpace.Height + Padding.Top + Padding.Bottom
            };
        }

        //当_RealChildCount ==0 则_Columns与_Rows无意义
        private void DivideRowAndColumn(DivideMode devideMode, out uint realChildCount, out uint realColumns, out uint realRows)
        {
            realRows = realColumns = realChildCount = 0;

            foreach (UIElement child in InternalChildren)
            {
                if (child.Visibility != Visibility.Collapsed)
                    realChildCount++;
            }

            if (realChildCount == 0)
                return;

            switch (devideMode)
            {
                case DivideMode.Row:
                    realRows = Rows;
                    realColumns = (uint)(realChildCount / Rows + (realChildCount % Rows == 0 ? 0 : 1));
                    break;

                case DivideMode.Column:
                    realColumns = Columns;
                    realRows = (uint)(realChildCount / Columns + (realChildCount % Columns == 0 ? 0 : 1));
                    break;

                case DivideMode.Both:
                    realColumns = Columns;
                    realRows = (uint)Math.Max(Rows, realChildCount / Columns + (realChildCount % Columns == 0 ? 0 : 1));
                    break;

                case DivideMode.Auto:
                    for (int index = 0; index < realChildCount; index++)
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
                    break;
            }
        }
    }
}
