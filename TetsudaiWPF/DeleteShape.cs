﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;
using System.Windows.Controls;

namespace TetsudaiWPF
{
    internal class DeleteShape : UndoableEditAction
    {
        private Shape _shape;
        private Canvas _canvas;

        public DeleteShape(Shape shape, Canvas canvas)
        {
            _shape = shape;
            _canvas = canvas;
        }

        public void Redo()
        {
            _canvas.Children.Remove(_shape);
        }

        public void Undo()
        {
            _canvas.Children.Add(_shape);
        }
    }
}
