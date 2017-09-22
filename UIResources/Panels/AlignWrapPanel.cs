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
    public class AlignWrapPanel : Panel
    {
        //Horizontal
        private int _optimalColumns;
        private double _optimalWidth;

        //Vertical
        private int _optimalRows;
        private double _optimalHeight;

        private int _realChildCount;

        private Dictionary<int, double> _columnIndexToWidthMap = new Dictionary<int, double>();
        private Dictionary<int, double> _rowIndexToHeightMap = new Dictionary<int, double>();

        private Orientation _orientation;

        #region Properties

        public static readonly DependencyProperty OrientationProperty = WrapPanel.OrientationProperty.AddOwner(typeof(AlignWrapPanel),
                        new FrameworkPropertyMetadata(Orientation.Horizontal, FrameworkPropertyMetadataOptions.AffectsMeasure, OnOrientationChanged));
        public Orientation Orientation
        {
            get { return _orientation; }
            set { SetValue(OrientationProperty, value); }
        }

        private static void OnOrientationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            AlignWrapPanel p = (AlignWrapPanel)d;
            p._orientation = (Orientation)e.NewValue;
        }

        #endregion

        #region Method

        private struct IndexToSize
        {
            internal Dictionary<int, double> _divideIndexToSizeMap;
            internal Dictionary<int, double> _anotherDirDivideIndexToSizeMap;
            private Orientation _orientation;

            internal IndexToSize(Orientation orientation)
            {
                _orientation = orientation;

                _divideIndexToSizeMap = new Dictionary<int, double>();
                _anotherDirDivideIndexToSizeMap = new Dictionary<int, double>();
            }

            internal void ResetMap()
            {
                _divideIndexToSizeMap = new Dictionary<int, double>();
                _anotherDirDivideIndexToSizeMap = new Dictionary<int, double>();
            }

            internal Dictionary<int, double> GetColumnIndexToWidthMap
            {
                get { return _orientation == Orientation.Horizontal ? _divideIndexToSizeMap : _anotherDirDivideIndexToSizeMap; }
            }

            internal Dictionary<int, double> GetRowIndexToHeightMap
            {
                get { return _orientation == Orientation.Horizontal ? _anotherDirDivideIndexToSizeMap : _divideIndexToSizeMap; }
            }

            internal void UpdateDivideIndexToSizeMap(int index, Size size)
            {
                if (!_divideIndexToSizeMap.ContainsKey(index))
                    _divideIndexToSizeMap.Add(index, _orientation == Orientation.Horizontal ? size.Width : size.Height);
                else
                    _divideIndexToSizeMap[index] = Math.Max(_divideIndexToSizeMap[index], _orientation == Orientation.Horizontal ? size.Width : size.Height);
            }

            internal void UpdateAnotherDirDivideIndexToSizeMap(int index, Size size)
            {
                if (!_anotherDirDivideIndexToSizeMap.ContainsKey(index))
                    _anotherDirDivideIndexToSizeMap.Add(index, _orientation == Orientation.Horizontal ? size.Height : size.Width);
                else
                    _anotherDirDivideIndexToSizeMap[index] = Math.Max(_anotherDirDivideIndexToSizeMap[index], _orientation == Orientation.Horizontal ? size.Height : size.Width);
            }

            internal double CurDivideOptimalSize
            {
                get { return _divideIndexToSizeMap.Sum(k => k.Value); }
            }

            internal bool IsCurDivideOptimalSizeValid(double curDivideOptimalSize, Size size)
            {
                return DoubleUtil.LessThanOrClose(curDivideOptimalSize, _orientation == Orientation.Horizontal ? size.Width : size.Height);
            }

        }

        private void Divide(Size constraint, out int realChildCount, out int optimalDivides, out double optimalDivideSize)
        {
            realChildCount = optimalDivides = 0;
            optimalDivideSize = 0;

            foreach (UIElement child in InternalChildren)
            {
                if (child.Visibility != Visibility.Collapsed)
                    realChildCount++;
            }

            if (realChildCount == 0)
                return;

            IndexToSize indexToSize = new IndexToSize(_orientation);
            double curOptimalDivideSize = 0;

            int arrangeCount = 0;
            int anotherDirDivideIndex = 0;

            for (int divideIndex = 1; divideIndex <= realChildCount; divideIndex++)
            {
                arrangeCount = 0;
                anotherDirDivideIndex = 0;

                indexToSize.ResetMap();

                for (int childIndex = 0; childIndex < InternalChildren.Count; childIndex++)
                {
                    UIElement child = InternalChildren[childIndex];
                    if (child.Visibility == Visibility.Collapsed)
                        continue;

                    child.Measure(constraint);

                    indexToSize.UpdateDivideIndexToSizeMap(arrangeCount, child.DesiredSize);
                    indexToSize.UpdateAnotherDirDivideIndexToSizeMap(anotherDirDivideIndex, child.DesiredSize);

                    arrangeCount++;
                    if (arrangeCount == divideIndex)
                    {
                        curOptimalDivideSize = indexToSize.CurDivideOptimalSize;
                        if (!indexToSize.IsCurDivideOptimalSizeValid(curOptimalDivideSize, constraint))
                            break;

                        arrangeCount = 0;
                        anotherDirDivideIndex++;
                    }
                }

                curOptimalDivideSize = indexToSize.CurDivideOptimalSize;
                if (indexToSize.IsCurDivideOptimalSizeValid(curOptimalDivideSize, constraint) && DoubleUtil.GreaterThanOrClose(curOptimalDivideSize, optimalDivideSize))
                {
                    optimalDivideSize = curOptimalDivideSize;
                    optimalDivides = divideIndex;

                    _columnIndexToWidthMap = indexToSize.GetColumnIndexToWidthMap;
                    _rowIndexToHeightMap = indexToSize.GetRowIndexToHeightMap;
                }
            }
        }

        #endregion

        #region Override

        protected override Size MeasureOverride(Size availableSize)
        {
            if (_orientation == Orientation.Horizontal)
                Divide(availableSize, out _realChildCount, out _optimalColumns, out _optimalWidth);
            else
                Divide(availableSize, out _realChildCount, out _optimalRows, out _optimalHeight);

            if (_realChildCount == 0)
                return base.MeasureOverride(availableSize);

            return new Size(_columnIndexToWidthMap.Sum(k => k.Value), _rowIndexToHeightMap.Sum(k => k.Value));
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            if (_realChildCount == 0)
                return base.ArrangeOverride(finalSize);

            Rect childBounds = new Rect();
            int RealIndex = 0;

            for (int index = 0; index < InternalChildren.Count; index++)
            {
                UIElement child = InternalChildren[index];

                if (child.Visibility != Visibility.Collapsed)
                {
                    int ColumnIndex = _orientation == Orientation.Horizontal ? (RealIndex % _optimalColumns) : (RealIndex / _optimalRows);
                    int RowIndex = _orientation == Orientation.Horizontal ? (RealIndex / _optimalColumns) : (RealIndex % _optimalRows);

                    childBounds.X = _columnIndexToWidthMap.Sum(k => k.Key < ColumnIndex ? k.Value : 0);
                    childBounds.Y = _rowIndexToHeightMap.Sum(k => k.Key < RowIndex ? k.Value : 0);

                    childBounds.Width = _columnIndexToWidthMap[ColumnIndex];
                    childBounds.Height = _rowIndexToHeightMap[RowIndex];

                    child.Arrange(childBounds);
                    RealIndex++;
                }
            }

            return finalSize;
        }

        #endregion
    }
}

