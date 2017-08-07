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
        private int _OptimalColumns;
        private double _OptimalWidth;

        //Vertical
        private int _OptimalRows;
        private double _OptimalHeight;

        private int _RealChildCount;

        private Dictionary<int, double> _ColumnIndexToWidthMap = new Dictionary<int, double>();
        private Dictionary<int, double> _RowIndexToHeightMap = new Dictionary<int, double>();

        private Orientation _Orientation;

        #region Properties

        public static readonly DependencyProperty OrientationProperty = WrapPanel.OrientationProperty.AddOwner(typeof(AlignWrapPanel),
                        new FrameworkPropertyMetadata(Orientation.Horizontal, FrameworkPropertyMetadataOptions.AffectsMeasure, OnOrientationChanged));
        public Orientation Orientation
        {
            get { return _Orientation; }
            set { SetValue(OrientationProperty, value); }
        }

        private static void OnOrientationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            AlignWrapPanel p = (AlignWrapPanel)d;
            p._Orientation = (Orientation)e.NewValue;
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

            IndexToSize indexToSize = new IndexToSize(_Orientation);
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

                    _ColumnIndexToWidthMap = indexToSize.GetColumnIndexToWidthMap;
                    _RowIndexToHeightMap = indexToSize.GetRowIndexToHeightMap;
                }
            }
        }

        #endregion

        #region Override

        protected override Size MeasureOverride(Size availableSize)
        {
            if (_Orientation == Orientation.Horizontal)
                Divide(availableSize, out _RealChildCount, out _OptimalColumns, out _OptimalWidth);
            else
                Divide(availableSize, out _RealChildCount, out _OptimalRows, out _OptimalHeight);

            if (_RealChildCount == 0)
                return base.MeasureOverride(availableSize);

            return new Size(_ColumnIndexToWidthMap.Sum(k => k.Value), _RowIndexToHeightMap.Sum(k => k.Value));
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            if (_RealChildCount == 0)
                return base.ArrangeOverride(finalSize);

            Rect childBounds = new Rect();
            int RealIndex = 0;

            for (int index = 0; index < InternalChildren.Count; index++)
            {
                UIElement child = InternalChildren[index];

                if (child.Visibility != Visibility.Collapsed)
                {
                    int ColumnIndex = _Orientation == Orientation.Horizontal ? (RealIndex % _OptimalColumns) : (RealIndex / _OptimalRows);
                    int RowIndex = _Orientation == Orientation.Horizontal ? (RealIndex / _OptimalColumns) : (RealIndex % _OptimalRows);

                    childBounds.X = _ColumnIndexToWidthMap.Sum(k => k.Key < ColumnIndex ? k.Value : 0);
                    childBounds.Y = _RowIndexToHeightMap.Sum(k => k.Key < RowIndex ? k.Value : 0);

                    childBounds.Width = _ColumnIndexToWidthMap[ColumnIndex];
                    childBounds.Height = _RowIndexToHeightMap[RowIndex];

                    child.Arrange(childBounds);
                    RealIndex++;
                }
            }

            return finalSize;
        }

        #endregion
    }
}

