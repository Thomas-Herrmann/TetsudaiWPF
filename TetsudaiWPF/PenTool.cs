using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Xceed.Wpf.Toolkit;

namespace TetsudaiWPF
{
    internal class PenTool : DrawingEditTool
    {
        private Canvas _canvas;
        private PathFigure lineSegments;
        private Path path;
        private bool isDrawing;
        private BindingList<UndoableEditAction> _undoList;

        public PenTool(Canvas canvas, BindingList<UndoableEditAction> undoList)
        {
            _canvas = canvas;
            isDrawing = false;
            _undoList = undoList;
        }

        public override Cursor ContextualCursor(Point position)
        {
            return Cursors.Pen;
        }

        public override void KeyDown(object sender, KeyEventArgs e)
        {
            // TODO
        }

        public override void KeyUp(object sender, KeyEventArgs e)
        {
            // TODO
        }

        public override void MouseDown(object sender, MouseButtonEventArgs e)
        {
            lineSegments = new PathFigure();
            lineSegments.StartPoint = e.GetPosition(_canvas);
            lineSegments.Segments.Add(new LineSegment(e.GetPosition(_canvas), true));

            var figures = new PathFigureCollection();

            figures.Add(lineSegments);

            path = new Path
            {
                Stroke = StrokeColor,
                StrokeThickness = StrokeThickness,
                Data = new PathGeometry(figures),
                StrokeStartLineCap = PenLineCap.Round,
                StrokeLineJoin = PenLineJoin.Round,
                StrokeEndLineCap = PenLineCap.Round,
                StrokeDashCap = PenLineCap.Round
        };

            _canvas.Children.Add(path);
            isDrawing = true;

        }

        public override void MouseMove(object sender, MouseEventArgs e)
        {
            if (isDrawing)
                lineSegments.Segments.Add(new LineSegment(e.GetPosition(_canvas), true));
        }

        public override void MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (isDrawing && path != null)
                _undoList.Add(new AddShape(path, _canvas));

            isDrawing = false;
        }
    }
}
