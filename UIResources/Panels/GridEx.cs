using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using UIResources.Helps;

namespace UIResources.Panels
{
    public class GridEx : Grid
    {
        private Pen _pen = null;
        private List<SpanInfo> _rowSpanInfos = null;
        private List<SpanInfo> _columnSpanInfos = null;

        struct SpanInfo
        {
            public int ColumnIndex { get; set; }
            public int RowIndex { get; set; }
            public int SpanCount { get; set; }
            public double Offset { get; set; }
            public double Size { get; set; }
        }

        #region Properties

        public static readonly DependencyProperty LineThicknessProperty = DependencyProperty.Register("LineThickness", typeof(double), typeof(GridEx), new FrameworkPropertyMetadata(1d, FrameworkPropertyMetadataOptions.AffectsRender, null, CoerceLineThickness), ValidateLineThickness);
        public double LineThickness
        {
            get { return (double)GetValue(LineThicknessProperty); }
            set { SetValue(LineThicknessProperty, value); }
        }

        static object CoerceLineThickness(DependencyObject d, object baseValue)
        {
            return Math.Max(0, (double)baseValue);
        }

        static bool ValidateLineThickness(object value)
        {
            return ((double)value).IsValid(false, false, false, false);
        }

        public static readonly DependencyProperty LineBrushProperty = DependencyProperty.Register("LineBrush", typeof(Brush), typeof(GridEx), new FrameworkPropertyMetadata(Brushes.Black, FrameworkPropertyMetadataOptions.AffectsRender));
        public Brush LineBrush
        {
            get { return (Brush)GetValue(LineBrushProperty); }
            set { SetValue(LineBrushProperty, value); }
        }

        #endregion

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);

            if (DoubleUtil.IsZero(LineThickness) || Equals(LineBrush, Brushes.Transparent) || LineBrush == null)
            {
                _pen = null;
                ClearSpanInfos();

                return;
            }

            _pen = GetPen();
            DrawLine(dc);
        }

        private Pen GetPen()
        {
            var pen = new Pen(LineBrush, LineThickness);
            pen.Freeze();

            return pen;
        }

        private void DrawLine(DrawingContext dc)
        {
            DrawInnerLine(dc);
            DrawOuterLine(dc);
        }

        private void DrawInnerLine(DrawingContext dc)
        {
            InitSpanInfos();

            DrawRowInnerLine(dc);
            DrawColumnInnerLine(dc);
        }

        private void DrawRowInnerLine(DrawingContext dc)
        {
            for (int rowIndex = 0; rowIndex < RowDefinitions.Count - 1; rowIndex++)
            {
                if (_columnSpanInfos != null && _columnSpanInfos.Count > 0 && _columnSpanInfos.Exists(i => i.RowIndex <= rowIndex && i.RowIndex + i.SpanCount - 1 > rowIndex))
                    DrawRowLine(rowIndex, _columnSpanInfos, dc);
                else
                {
                    var row = RowDefinitions[rowIndex];

                    if (DoubleUtil.IsZero(row.ActualHeight))
                        continue;

                    dc.DrawLine(_pen,
                        new Point(0, row.Offset + row.ActualHeight),
                        new Point(ActualWidth, row.Offset + row.ActualHeight));
                }
            }
        }

        private void DrawColumnInnerLine(DrawingContext dc)
        {
            for (int columnIndex = 0; columnIndex < ColumnDefinitions.Count - 1; columnIndex++)
            {
                if (_rowSpanInfos != null && _rowSpanInfos.Count > 0 && _rowSpanInfos.Exists(i => i.ColumnIndex <= columnIndex && i.ColumnIndex + i.SpanCount - 1 > columnIndex))
                    DrawColumnLine(columnIndex, _rowSpanInfos, dc);
                else
                {
                    var column = ColumnDefinitions[columnIndex];

                    if (DoubleUtil.IsZero(column.ActualWidth))
                        continue;

                    dc.DrawLine(_pen,
                        new Point(column.Offset + column.ActualWidth, 0),
                        new Point(column.Offset + column.ActualWidth, ActualHeight));
                }
            }
        }

        private void DrawOuterLine(DrawingContext dc)
        {

        }

        private void ClearSpanInfos()
        {
            if (_rowSpanInfos != null)
                _rowSpanInfos.Clear();

            if (_columnSpanInfos != null)
                _columnSpanInfos.Clear();
        }

        private void InitSpanInfos()
        {
            ClearSpanInfos();

            foreach (UIElement child in Children)
            {
                if (GetRowSpan(child) > 1)
                {
                    if (_columnSpanInfos == null)
                        _columnSpanInfos = new List<SpanInfo>();

                    _columnSpanInfos.Add(
                        new SpanInfo
                        {
                            RowIndex = GetRow(child),
                            ColumnIndex = GetColumn(child),
                            SpanCount = GetRowSpan(child),
                            Offset = ColumnDefinitions[GetColumn(child)].Offset,
                            Size = ColumnDefinitions[GetColumn(child)].ActualWidth
                        });
                }

                if (GetColumnSpan(child) > 1)
                {
                    if (_rowSpanInfos == null)
                        _rowSpanInfos = new List<SpanInfo>();

                    _rowSpanInfos.Add(
                        new SpanInfo
                        {
                            RowIndex = GetRow(child),
                            ColumnIndex = GetColumn(child),
                            SpanCount = GetColumnSpan(child),
                            Offset = RowDefinitions[GetRow(child)].Offset,
                            Size = RowDefinitions[GetRow(child)].ActualHeight
                        });
                }
            }
        }

        private void DrawRowLine(int rowIndex, IEnumerable<SpanInfo> columnSpanInfos, DrawingContext dc)
        {
            var row = RowDefinitions[rowIndex];

            SpanInfo? preInfo = null;
            foreach (var curInfo in columnSpanInfos.Where(i => rowIndex >= i.RowIndex && rowIndex < i.SpanCount + i.RowIndex - 1).OrderBy(i => i.ColumnIndex))
            {
                if (curInfo.ColumnIndex != 0)
                {
                    if (preInfo == null)
                    {
                        dc.DrawLine(_pen,
                            new Point(0, row.Offset + row.ActualHeight),
                            new Point(curInfo.Offset, row.Offset + row.ActualHeight));
                    }
                    else
                    {
                        if (curInfo.ColumnIndex - preInfo.Value.ColumnIndex > 1)
                        {
                            dc.DrawLine(_pen,
                                new Point(preInfo.Value.Offset + preInfo.Value.Size, row.Offset + row.ActualHeight),
                                new Point(curInfo.Offset, row.Offset + row.ActualHeight));
                        }
                    }
                }

                preInfo = curInfo;
            }

            if (preInfo.HasValue && preInfo.Value.ColumnIndex < ColumnDefinitions.Count - 1)
            {
                dc.DrawLine(_pen,
                    new Point(preInfo.Value.Offset + preInfo.Value.Size, row.Offset + row.ActualHeight),
                    new Point(ActualWidth, row.Offset + row.ActualHeight));
            }
        }

        private void DrawColumnLine(int columnIndex, IEnumerable<SpanInfo> rowSpanInfos, DrawingContext dc)
        {
            var column = ColumnDefinitions[columnIndex];

            SpanInfo? preInfo = null;
            foreach (var curInfo in rowSpanInfos.Where(i => columnIndex >= i.ColumnIndex && columnIndex < i.SpanCount + i.ColumnIndex - 1).OrderBy(i => i.RowIndex))
            {
                if (curInfo.RowIndex != 0)
                {
                    if (preInfo == null)
                    {
                        dc.DrawLine(_pen,
                            new Point(column.Offset + column.ActualWidth, 0),
                            new Point(column.Offset + column.ActualWidth, curInfo.Offset));
                    }
                    else
                    {
                        if (curInfo.RowIndex - preInfo.Value.RowIndex > 1)
                        {
                            dc.DrawLine(_pen,
                                new Point(column.Offset + column.ActualWidth, preInfo.Value.Offset + preInfo.Value.Size),
                                new Point(column.Offset + column.ActualWidth, curInfo.Offset));
                        }
                    }
                }

                preInfo = curInfo;
            }

            if (preInfo.HasValue && preInfo.Value.RowIndex < RowDefinitions.Count - 1)
            {
                dc.DrawLine(_pen,
                    new Point(column.Offset + column.ActualWidth, preInfo.Value.Offset + preInfo.Value.Size),
                    new Point(column.Offset + column.ActualWidth, ActualHeight));
            }
        }

    }
}
