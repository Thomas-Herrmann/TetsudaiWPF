using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace TetsudaiWPF
{
    internal interface EditTool
    {
        public void MouseDown(object sender, MouseButtonEventArgs e);
        public void MouseUp(object sender, MouseButtonEventArgs e);
        public void MouseMove(object sender, MouseEventArgs e);
        public void KeyDown(object sender, KeyEventArgs e);
        public void KeyUp(object sender, KeyEventArgs e);
        public Cursor ContextualCursor(Point position);
        public List<UIElement> SettingsUIElements { get; }
    }
}
