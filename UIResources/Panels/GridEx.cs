using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace UIResources.Panels
{
    public class GridEx : Grid
    {
        public static readonly DependencyProperty BorderThicknessProperty = DependencyProperty.Register("BorderThickness", typeof(double), typeof(GridEx), new FrameworkPropertyMetadata(1d));
        public double BorderThickness
        {
            get { return (double)GetValue(BorderThicknessProperty); }
            set { SetValue(BorderThicknessProperty, value); }
        }

        public static readonly DependencyProperty BorderBrushProperty = DependencyProperty.Register("BorderBrush", typeof(Brush), typeof(GridEx), new FrameworkPropertyMetadata(Brushes.Black));
        public Brush BorderBrush
        {
            get { return (Brush)GetValue(BorderBrushProperty); }
            set { SetValue(BorderBrushProperty, value); }
        }

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);

            dc.DrawGeometry(null, new Pen(BorderBrush, BorderThickness), DrawLine());
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
                if (_columnSpanInfos.Count > 0)
                    DrawRowLine(rowIndex, _columnSpanInfos, ctx);
                else
                {
                    RowDefinition row = RowDefinitions[rowIndex];

                    if (row.ActualHeight == 0)
                        continue;

                    ctx.BeginFigure(new Point(5, row.Offset + row.ActualHeight), false, true);
                    ctx.LineTo(new Point(ActualWidth - 5, row.Offset + row.ActualHeight), true, false);
                }
            }
            /*
            for (int rowIndex = 0; rowIndex < RowDefinitions.Count - 1; rowIndex++)
            {
                RowDefinition row = RowDefinitions[rowIndex];

                if (row.ActualHeight == 0)
                    continue;

                ctx.BeginFigure(new Point(5, row.Offset + row.ActualHeight), false, true);
                ctx.LineTo(new Point(ActualWidth - 5, row.Offset + row.ActualHeight), true, false);
            }
             */
        }

        private void DrawColumnInnerLine(StreamGeometryContext ctx)
        {
            for (int columnIndex = 0; columnIndex < ColumnDefinitions.Count - 1; columnIndex++)
            {
                if (_rowSpanInfos.Count > 0)
                    DrawColumnLine(columnIndex, _rowSpanInfos, ctx);
                else
                {
                    ColumnDefinition column = ColumnDefinitions[columnIndex];

                    if (column.ActualWidth == 0)
                        continue;

                    ctx.BeginFigure(new Point(column.Offset + column.ActualWidth, 5), false, true);
                    ctx.LineTo(new Point(column.Offset + column.ActualWidth, ActualHeight - 5), true, false);
                }
            }
            /*
            for (int columnIndex = 0; columnIndex < ColumnDefinitions.Count - 1; columnIndex++)
            {
                ColumnDefinition column = ColumnDefinitions[columnIndex];

                if (column.ActualWidth == 0)
                    continue;

                ctx.BeginFigure(new Point(column.Offset + column.ActualWidth, 5), false, true);
                ctx.LineTo(new Point(column.Offset + column.ActualWidth, ActualHeight - 5), true, false);
            }
             * */
        }

        private void DrawOuterLine(StreamGeometryContext ctx)
        {

        }


        private List<SpanInfo> _rowSpanInfos = new List<SpanInfo>();
        private List<SpanInfo> _columnSpanInfos = new List<SpanInfo>();

        struct SpanInfo
        {
            public int ColumnIndex { get; set; }
            public int RowIndex { get; set; }
            public int SpanCount { get; set; }
            public double Offset { get; set; }
            public double Size { get; set; }
        }

        private void InitSpanInfos()
        {
            _rowSpanInfos.Clear();
            _columnSpanInfos.Clear();

            foreach (UIElement child in Children)
            {
                if (GetRowSpan(child) > 1)
                {
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
            foreach (var curInfo in columnSpanInfos.Where(i => rowIndex >= i.RowIndex && rowIndex < i.SpanCount + i.RowIndex).OrderBy(i => i.ColumnIndex))
            {
                if (curInfo.ColumnIndex == 0)
                    preInfo = curInfo;
                else
                {
                    if (preInfo == null)
                    {
                        ctx.BeginFigure(new Point(0, row.Offset + row.ActualHeight), false, true);
                        ctx.LineTo(new Point(curInfo.Offset, row.Offset + row.ActualHeight), true, false);

                        preInfo = curInfo;
                    }
                    else
                    {
                        if (curInfo.ColumnIndex - preInfo.Value.ColumnIndex > 1)
                        {
                            ctx.BeginFigure(new Point(preInfo.Value.Offset + preInfo.Value.Size, row.Offset + row.ActualHeight), false, true);
                            ctx.LineTo(new Point(curInfo.Offset, row.Offset + row.ActualHeight), true, false);
                        }

                        preInfo = curInfo;
                    }
                }

            }
        }

        private void DrawColumnLine(int columnIndex, IEnumerable<SpanInfo> rowSpanInfos, StreamGeometryContext ctx)
        {
            var column = ColumnDefinitions[columnIndex];

            SpanInfo? preInfo = null;
            foreach (var curInfo in rowSpanInfos.Where(i => columnIndex >= i.ColumnIndex && columnIndex < i.SpanCount + i.ColumnIndex).OrderBy(i => i.RowIndex))
            {
                if (curInfo.RowIndex == 0)
                    preInfo = curInfo;
                else
                {
                    if (preInfo == null)
                    {
                        ctx.BeginFigure(new Point(column.Offset + column.ActualWidth, 0), false, true);
                        ctx.LineTo(new Point(column.Offset + column.ActualWidth, curInfo.Offset), true, false);

                        preInfo = curInfo;
                    }
                    else
                    {
                        if (curInfo.RowIndex - preInfo.Value.RowIndex > 1)
                        {
                            ctx.BeginFigure(new Point(preInfo.Value.Offset + preInfo.Value.Size, column.Offset + column.ActualWidth), false, true);
                            ctx.LineTo(new Point(column.Offset + column.ActualWidth, curInfo.Offset), true, false);
                        }

                        preInfo = curInfo;
                    }
                }

            }
        }

    }
}
