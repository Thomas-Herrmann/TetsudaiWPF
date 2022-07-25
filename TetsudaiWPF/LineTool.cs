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
    internal class LineTool : DrawingEditTool
    {
        private bool isDrawing;
        private Canvas _canvas;
        private Point lineStart;
        private Line line;
        private bool shouldSnap;
        private BindingList<UndoableEditAction> _undoList;

        public LineTool(Canvas canvas, BindingList<UndoableEditAction> undoList)
        {
            _canvas = canvas;
            isDrawing = false;
            shouldSnap = false;
            _undoList = undoList;
        }

        public override Cursor ContextualCursor(Point position)
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

        public override void KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.LeftShift)
                shouldSnap = true;
        }

        public override void KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.LeftShift)
                shouldSnap = false;
        }

        public override void MouseDown(object sender, MouseButtonEventArgs e)
        {
            isDrawing = true;
            lineStart = e.GetPosition(_canvas);
            line = new Line {
                X1 = lineStart.X,
                Y1 = lineStart.Y,
                X2 = lineStart.X,
                Y2 = lineStart.Y,
                Stroke = StrokeColor,
                StrokeThickness = StrokeThickness,
                StrokeStartLineCap = PenLineCap.Round,
                StrokeEndLineCap = PenLineCap.Round
        };
            
            _canvas.Children.Add(line);

        }

        public override void MouseMove(object sender, MouseEventArgs e)
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

                line.X2 = mousePosition.X;
                line.Y2 = mousePosition.Y;
            }
        }

        public override void MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (isDrawing && line != null)
                _undoList.Add(new AddShape(line, _canvas));

            isDrawing = false;
        }
    }
}
