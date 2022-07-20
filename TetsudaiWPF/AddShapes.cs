using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;
using System.Windows.Controls;

namespace TetsudaiWPF
{
    internal class AddShapes : UndoableEditAction
    {
        private List<Shape> _shapes;
        private Canvas _canvas;

        public AddShapes(List<Shape> shapes, Canvas canvas)
        {
            _shapes = shapes;
            _canvas = canvas;
        }

        public void Redo()
        {
            _shapes.ForEach(shape => _canvas.Children.Add(shape));
        }

        public void Undo()
        {
            _shapes.ForEach(shape => _canvas.Children.Remove(shape));
        }
    }
}
