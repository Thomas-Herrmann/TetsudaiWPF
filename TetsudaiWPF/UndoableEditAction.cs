using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TetsudaiWPF
{
    internal interface UndoableEditAction
    {
        public void Undo();

        public void Redo();
    }
}
