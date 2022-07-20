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

namespace TetsudaiWPF
{
    internal class PenTool : EditTool
    {
        private Canvas _canvas;
        private PathFigure lineSegments;
        private Path path;
        private bool isDrawing;
        private BindingList<UndoableEditAction> _undoList;
        private BindingList<UndoableEditAction> _redoList;

        public PenTool(Canvas canvas, BindingList<UndoableEditAction> undoList, BindingList<UndoableEditAction> redoList)
        {
            _canvas = canvas;
            isDrawing = false;
            _undoList = undoList;
            _redoList = redoList;
        }

        public Cursor ContextualCursor(Point position)
        {
            return Cursors.Pen;
        }

        public void KeyDown(object sender, KeyEventArgs e)
        {
            // TODO
        }

        public void KeyUp(object sender, KeyEventArgs e)
        {
            // TODO
        }

        public void MouseDown(object sender, MouseButtonEventArgs e)
        {
            lineSegments = new PathFigure();
            lineSegments.StartPoint = e.GetPosition(_canvas);
            lineSegments.Segments.Add(new LineSegment(e.GetPosition(_canvas), true));

            var figures = new PathFigureCollection();

            figures.Add(lineSegments);

            path = new Path
            {
                Stroke = new SolidColorBrush(Colors.Red),
                StrokeThickness = 5,
                Data = new PathGeometry(figures),
                StrokeStartLineCap = PenLineCap.Round,
                StrokeLineJoin = PenLineJoin.Round,
                StrokeEndLineCap = PenLineCap.Round,
                StrokeDashCap = PenLineCap.Round
        };

            _canvas.Children.Add(path);
            isDrawing = true;

        }

        public void MouseMove(object sender, MouseEventArgs e)
        {
            if (isDrawing)
                lineSegments.Segments.Add(new LineSegment(e.GetPosition(_canvas), true));
        }

        public void MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (isDrawing && path != null)
                _undoList.Add(new AddShape(path, _canvas));

            isDrawing = false;
        }
    }
}
