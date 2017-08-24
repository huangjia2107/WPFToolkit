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
        private StreamGeometry _borderGeometryCache = null;
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

        private double AdjustThickness { get { return 5 * BorderThickness; } }

        #region Properties

        public static readonly DependencyProperty BorderThicknessProperty = DependencyProperty.Register("BorderThickness", typeof(double), typeof(GridEx), new FrameworkPropertyMetadata(1d, FrameworkPropertyMetadataOptions.AffectsRender, null, CoerceBorderThickness), ValidateBorderThickness);
        public double BorderThickness
        {
            get { return (double)GetValue(BorderThicknessProperty); }
            set { SetValue(BorderThicknessProperty, value); }
        }

        static object CoerceBorderThickness(DependencyObject d, object baseValue)
        {
            return Math.Max(0, (double)baseValue);
        }

        static bool ValidateBorderThickness(object value)
        {
            return ((double)value).IsValid(false, false, false, false);
        }

        public static readonly DependencyProperty BorderBrushProperty = DependencyProperty.Register("BorderBrush", typeof(Brush), typeof(GridEx), new FrameworkPropertyMetadata(Brushes.Black, FrameworkPropertyMetadataOptions.AffectsRender));
        public Brush BorderBrush
        {
            get { return (Brush)GetValue(BorderBrushProperty); }
            set { SetValue(BorderBrushProperty, value); }
        }

        #endregion

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);

            if (BorderThickness == 0 || BorderBrush == Brushes.Transparent || BorderBrush == null)
            {
                _borderGeometryCache = null;
                ClearSpanInfos();

                return;
            }

            _borderGeometryCache = DrawLine();
            dc.DrawGeometry(null, new Pen(BorderBrush, BorderThickness), _borderGeometryCache);
        }

        private StreamGeometry DrawLine()
        {
            StreamGeometry borderGeometry = new StreamGeometry();
            using (StreamGeometryContext ctx = borderGeometry.Open())
            {
                DrawInnerLine(ctx);
                DrawOuterLine(ctx);
            }

            borderGeometry.Freeze();

            return borderGeometry;
        }

        private void DrawInnerLine(StreamGeometryContext ctx)
        {
            InitSpanInfos();

            DrawRowInnerLine(ctx);
            DrawColumnInnerLine(ctx);
        }

        private void DrawRowInnerLine(StreamGeometryContext ctx)
        {
            for (int rowIndex = 0; rowIndex < RowDefinitions.Count - 1; rowIndex++)
            {
                if (_columnSpanInfos.Count > 0 && _columnSpanInfos.Exists(i => i.RowIndex <= rowIndex && i.RowIndex + i.SpanCount - 1 > rowIndex))
                    DrawRowLine(rowIndex, _columnSpanInfos, ctx);
                else
                {
                    RowDefinition row = RowDefinitions[rowIndex];

                    if (row.ActualHeight == 0)
                        continue;

                    ctx.BeginFigure(new Point(AdjustThickness, row.Offset + row.ActualHeight), false, true);
                    ctx.LineTo(new Point(ActualWidth - AdjustThickness, row.Offset + row.ActualHeight), true, false);
                }
            }
        }

        private void DrawColumnInnerLine(StreamGeometryContext ctx)
        {
            for (int columnIndex = 0; columnIndex < ColumnDefinitions.Count - 1; columnIndex++)
            {
                if (_rowSpanInfos.Count > 0 && _rowSpanInfos.Exists(i => i.ColumnIndex <= columnIndex && i.ColumnIndex + i.SpanCount - 1 > columnIndex))
                    DrawColumnLine(columnIndex, _rowSpanInfos, ctx);
                else
                {
                    ColumnDefinition column = ColumnDefinitions[columnIndex];

                    if (column.ActualWidth == 0)
                        continue;

                    ctx.BeginFigure(new Point(column.Offset + column.ActualWidth, AdjustThickness), false, true);
                    ctx.LineTo(new Point(column.Offset + column.ActualWidth, ActualHeight - AdjustThickness), true, false);
                }
            }
        }

        private void DrawOuterLine(StreamGeometryContext ctx)
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

        private void DrawRowLine(int rowIndex, IEnumerable<SpanInfo> columnSpanInfos, StreamGeometryContext ctx)
        {
            var row = RowDefinitions[rowIndex];

            SpanInfo? preInfo = null;
            foreach (var curInfo in columnSpanInfos.Where(i => rowIndex >= i.RowIndex && rowIndex < i.SpanCount + i.RowIndex - 1).OrderBy(i => i.ColumnIndex))
            {
                if (curInfo.ColumnIndex == 0)
                    preInfo = curInfo;
                else
                {
                    if (preInfo == null)
                    {
                        ctx.BeginFigure(new Point(AdjustThickness, row.Offset + row.ActualHeight), false, true);
                        ctx.LineTo(new Point(curInfo.Offset - AdjustThickness, row.Offset + row.ActualHeight), true, false);

                        preInfo = curInfo;
                    }
                    else
                    {
                        if (curInfo.ColumnIndex - preInfo.Value.ColumnIndex > 1)
                        {
                            ctx.BeginFigure(new Point(preInfo.Value.Offset + preInfo.Value.Size + AdjustThickness, row.Offset + row.ActualHeight), false, true);
                            ctx.LineTo(new Point(curInfo.Offset - AdjustThickness, row.Offset + row.ActualHeight), true, false);
                        }

                        preInfo = curInfo;
                    }
                }
            }

            if (preInfo.Value.ColumnIndex < ColumnDefinitions.Count - 1)
            {
                ctx.BeginFigure(new Point(preInfo.Value.Offset + preInfo.Value.Size + AdjustThickness, row.Offset + row.ActualHeight), false, true);
                ctx.LineTo(new Point(ActualWidth - AdjustThickness, row.Offset + row.ActualHeight), true, false);
            }
        }

        private void DrawColumnLine(int columnIndex, IEnumerable<SpanInfo> rowSpanInfos, StreamGeometryContext ctx)
        {
            var column = ColumnDefinitions[columnIndex];

            SpanInfo? preInfo = null;
            foreach (var curInfo in rowSpanInfos.Where(i => columnIndex >= i.ColumnIndex && columnIndex < i.SpanCount + i.ColumnIndex - 1).OrderBy(i => i.RowIndex))
            {
                if (curInfo.RowIndex == 0)
                    preInfo = curInfo;
                else
                {
                    if (preInfo == null)
                    {
                        ctx.BeginFigure(new Point(column.Offset + column.ActualWidth, AdjustThickness), false, true);
                        ctx.LineTo(new Point(column.Offset + column.ActualWidth, curInfo.Offset - AdjustThickness), true, false);

                        preInfo = curInfo;
                    }
                    else
                    {
                        if (curInfo.RowIndex - preInfo.Value.RowIndex > 1)
                        {
                            ctx.BeginFigure(new Point(column.Offset + column.ActualWidth, preInfo.Value.Offset + preInfo.Value.Size + AdjustThickness), false, true);
                            ctx.LineTo(new Point(column.Offset + column.ActualWidth, curInfo.Offset - AdjustThickness), true, false);
                        }

                        preInfo = curInfo;
                    }
                }
            }

            if (preInfo.Value.RowIndex < RowDefinitions.Count - 1)
            {
                ctx.BeginFigure(new Point(column.Offset + column.ActualWidth, preInfo.Value.Offset + preInfo.Value.Size + AdjustThickness), false, true);
                ctx.LineTo(new Point(column.Offset + column.ActualWidth, ActualHeight - AdjustThickness), true, false);
            }
        }

    }
}
