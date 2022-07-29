using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;
using System.Windows.Controls;
using System.ComponentModel;

namespace TetsudaiWPF
{
    internal class AddShapes : UndoableEditAction
    {
        private List<Shape> _shapes;
        private Canvas _canvas;

        public bool WasRedone { get; set; }

        public AddShapes(List<Shape> shapes, Canvas canvas)
        {
            _shapes = shapes;
            _canvas = canvas;
        }

        public void Redo()
        {
            _shapes.ForEach(shape => _canvas.Children.Add(shape));
            WasRedone = true;
        }

        public void Undo()
        {
            _shapes.ForEach(shape => _canvas.Children.Remove(shape));
        }

        public void OnAddedToUndoList(BindingList<UndoableEditAction> redoList)
        {
            redoList.Clear();
        }
    }
}
