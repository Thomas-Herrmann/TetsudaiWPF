using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TetsudaiWPF
{
    internal interface UndoableEditAction
    {
        public bool WasRedone { get; }

        public void Undo();

        public void Redo();

        public void OnAddedToUndoList(BindingList<UndoableEditAction> redoList);
    }
}
