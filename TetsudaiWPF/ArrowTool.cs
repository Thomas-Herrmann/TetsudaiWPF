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
    internal class ArrowTool : EditTool
    {
        private bool isDrawing;
        private Canvas _canvas;
        private Point lineStart;
        private Line line;
        private Path arrowhead;
        private PathFigure lineSegments;
        private double strokeThickness = 5;
        private bool shouldSnap;
        private BindingList<UndoableEditAction> _undoList;
        private BindingList<UndoableEditAction> _redoList;

        public ArrowTool(Canvas canvas, BindingList<UndoableEditAction> undoList, BindingList<UndoableEditAction> redoList)
        {
            _canvas = canvas;
            isDrawing = false;
            shouldSnap = false;
            _undoList = undoList;
            _redoList = redoList;
        }

        public Cursor ContextualCursor(Point position)
        {
            if (!isDrawing || lineStart == position)
                return Cursors.ScrollAll;
            else if (position.X == lineStart.X)
                return Cursors.ScrollWE;
            else if (position.Y == lineStart.Y)
                return Cursors.ScrollNS;
            else if (position.X <= lineStart.X && position.Y <= lineStart.Y)
                return Cursors.ScrollNW;
            else if (position.X <= lineStart.X && position.Y >= lineStart.Y)
                return Cursors.ScrollSW;
            else if (position.X >= lineStart.X && position.Y <= lineStart.Y)
                return Cursors.ScrollNE;
            else
                return Cursors.ScrollSE;
        }

        public void KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.LeftShift)
                shouldSnap = true;
        }

        public void KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.LeftShift)
                shouldSnap = false;
        }

        public void MouseDown(object sender, MouseButtonEventArgs e)
        {
            isDrawing = true;
            lineStart = e.GetPosition(_canvas);
            line = new Line
            {
                X1 = lineStart.X,
                Y1 = lineStart.Y,
                X2 = lineStart.X,
                Y2 = lineStart.Y,
                Stroke = Brushes.Red,
                StrokeThickness = strokeThickness,
                StrokeStartLineCap = PenLineCap.Round,
                StrokeEndLineCap = PenLineCap.Flat
            };

            var figures = new PathFigureCollection();
            
            lineSegments = new PathFigure();

            figures.Add(lineSegments);

            arrowhead = new Path
            {
                Stroke = Brushes.Red,
                Fill = Brushes.Red,
                StrokeThickness = 1,
                StrokeStartLineCap = PenLineCap.Round,
                StrokeEndLineCap = PenLineCap.Round,
                Data = new PathGeometry(figures)
            };

            _canvas.Children.Add(line);
            _canvas.Children.Add(arrowhead);
        }

        public void MouseMove(object sender, MouseEventArgs e)
        {
            if (isDrawing)
            {
                var mousePosition = e.GetPosition(_canvas);

                if (shouldSnap && mousePosition != lineStart)
                {
                    var ix = new Vector(1, 0);
                    var lineEnd = mousePosition - lineStart;
                    var theta = Math.Sign(lineEnd.Y) >= 0 ? Math.Acos(ix * lineEnd / lineEnd.Length) : -Math.Acos(ix * lineEnd / lineEnd.Length);
                    var quotient = Math.Floor(theta / (Math.PI / 4));
                    var snapAngle = Math.PI / 4;

                    if (Math.Abs(theta) % (Math.PI / 4) == 0)
                        snapAngle *= quotient;
                    else if ((Math.Abs(theta) % (Math.PI / 4) <= Math.PI / 8))
                        snapAngle *= theta >= 0 ? quotient : quotient + 1;
                    else
                        snapAngle *= theta >= 0 ? quotient + 1 : quotient;

                    var snapUnit = new Vector(1, Math.Tan(snapAngle));

                    snapUnit.Normalize();
                    mousePosition = lineEnd * snapUnit / (snapUnit * snapUnit) * snapUnit + lineStart;
                }

                lineSegments.Segments.Clear();

                if ((mousePosition - lineStart).Length >= strokeThickness * 2)
                {
                    var mouseVector = mousePosition - lineStart;
                    var normPos = mousePosition - lineStart;

                    normPos.Normalize();

                    var arrowStart = (Point)(normPos * (mouseVector.Length - strokeThickness * 2));
                    var arrowConcave = normPos * (mouseVector.Length - strokeThickness) + lineStart;
                    var arrowLeft = normPos.Y == 0 ? new Vector(0, -1) : new Vector(-1, normPos.X / normPos.Y);
                    var arrowRight = normPos.Y == 0 ? new Vector(0, 1) : new Vector(1, -(normPos.X / normPos.Y));

                    arrowLeft.Normalize();
                    arrowRight.Normalize();

                    arrowLeft *= strokeThickness;
                    arrowRight *= strokeThickness;
                    arrowLeft += (Vector)arrowStart + (Vector)lineStart;
                    arrowRight += (Vector)arrowStart + (Vector)lineStart;

                    lineSegments.StartPoint = mousePosition;

                    lineSegments.Segments.Add(new LineSegment((Point)arrowRight, true));
                    lineSegments.Segments.Add(new LineSegment(arrowConcave, true));
                    lineSegments.Segments.Add(new LineSegment((Point)arrowLeft, true));
                    lineSegments.Segments.Add(new LineSegment(mousePosition, true));

                    line.X2 = arrowConcave.X;
                    line.Y2 = arrowConcave.Y;
                    line.Visibility = Visibility.Visible;
                }
                else
                    line.Visibility = Visibility.Hidden;
            }
        }

        public void MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (isDrawing && line != null && arrowhead != null)
                _undoList.Add(new AddShapes(new List<Shape>{ line, arrowhead }, _canvas));

            isDrawing = false;
        }
    }
}
