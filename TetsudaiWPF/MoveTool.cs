using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace TetsudaiWPF
{
    internal class MoveTool : EditTool
    {
        private enum MovePhase
        {
            Idle,
            Selecting,
            Dragging
        }

        private Point? selectionStart;
        private Point? selectionEnd;
        private Point dragOffset;
        private MovePhase movePhase;
        private Canvas _canvas;
        private RectangleGeometry _selection;

        public MoveTool(Canvas canvas, RectangleGeometry selection)
        {
            _canvas = canvas;
            _selection = selection;
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
            var position = e.GetPosition(_canvas);

            if (_selection.Rect.Contains(position))
            {
                movePhase = MovePhase.Dragging;
                dragOffset = new Point(position.X - _selection.Rect.X, position.Y - _selection.Rect.Y);
            }
            else
            {
                selectionStart = position;
                movePhase = MovePhase.Selecting;
            }
        }

        public void MouseMove(object sender, MouseEventArgs e)
        {
            var mousePosition = e.GetPosition(_canvas);
            var rectCopy = _selection.Rect;

            switch (movePhase)
            {
                case MovePhase.Selecting:
                    if (mousePosition != selectionStart)
                    {
                        selectionEnd = mousePosition;
                        _selection.Rect = new Rect(selectionStart.Value, selectionEnd.Value);
                    }
                    break;
                case MovePhase.Dragging:
                    rectCopy.Offset(mousePosition.X - (rectCopy.X + dragOffset.X), mousePosition.Y - (rectCopy.Y + dragOffset.Y));
                    _selection.Rect = rectCopy;
                    break;
            }
        }

        public void MouseUp(object sender, MouseButtonEventArgs e)
        {
            movePhase = MovePhase.Idle;
        }

        public Cursor ContextualCursor(Point position)
        {
            return _selection.Rect.Contains(position) ? Cursors.SizeAll : Cursors.Cross;
        }
    }
}
