﻿using System;
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
    internal class RectangleTool : DrawingEditTool
    {
        private bool isDrawing;
        private Canvas _canvas;
        private Point topLeft;
        private RectangleGeometry rectangle;
        private bool shouldSnap;
        private Path path;
        private BindingList<UndoableEditAction> _undoList;

        public RectangleTool(Canvas canvas, BindingList<UndoableEditAction> undoList)
        {
            _canvas = canvas;
            isDrawing = false;
            shouldSnap = false;
            _undoList = undoList;
        }

        public override Cursor ContextualCursor(Point position)
        {
            if (!isDrawing || topLeft == position)
                return Cursors.ScrollAll;
            else if (position.X == topLeft.X)
                return Cursors.ScrollWE;
            else if (position.Y == topLeft.Y)
                return Cursors.ScrollNS;
            else if (position.X <= topLeft.X && position.Y <= topLeft.Y)
                return Cursors.ScrollNW;
            else if (position.X <= topLeft.X && position.Y >= topLeft.Y)
                return Cursors.ScrollSW;
            else if (position.X >= topLeft.X && position.Y <= topLeft.Y)
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
            topLeft = e.GetPosition(_canvas);
            rectangle = new RectangleGeometry(new Rect(topLeft, topLeft));

            path = new Path
            {
                Data = rectangle,
                Stroke = StrokeColor,
                StrokeThickness = StrokeThickness,
                StrokeStartLineCap = PenLineCap.Round,
                StrokeEndLineCap = PenLineCap.Round
            };

            _canvas.Children.Add(path);

        }

        public override void MouseMove(object sender, MouseEventArgs e)
        {
            if (isDrawing)
            {
                var mousePosition = e.GetPosition(_canvas);

                if (shouldSnap)
                {
                    var relativePosition = mousePosition - topLeft;

                    mousePosition = new Vector(Math.Sign(relativePosition.X) * Math.Min(Math.Abs(relativePosition.X), Math.Abs(relativePosition.Y)), Math.Sign(relativePosition.Y) * Math.Min(Math.Abs(relativePosition.X), Math.Abs(relativePosition.Y))) + topLeft;
                }

                rectangle.Rect = new Rect(topLeft, mousePosition);
            }

        }

        public override void MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (isDrawing && path != null)
                _undoList.Add(new AddShape(path, _canvas));

            isDrawing = false;
        }
    }
}
