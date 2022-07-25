using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Xceed.Wpf.Toolkit;

namespace TetsudaiWPF
{
    abstract class DrawingEditTool : EditTool
    {
        public SolidColorBrush StrokeColor { get; set; }
        public uint StrokeThickness { get; set; }

        public DrawingEditTool()
        {
            StrokeColor = new SolidColorBrush(Colors.Red);
            StrokeThickness = 4;

            var backgroundColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#222222"));
            var foregroundColor = new SolidColorBrush(Colors.WhiteSmoke);

            var colorPicker = new ColorPicker
            {
                SelectedColor = Colors.Red,
                Background = backgroundColor,
                Foreground = foregroundColor,
                HeaderBackground = backgroundColor,
                HeaderForeground = foregroundColor,
                DropDownBackground = backgroundColor,
                DropDownBorderBrush = backgroundColor,
                BorderBrush = backgroundColor,
                TabBackground = backgroundColor,
                TabForeground = foregroundColor,
                FontWeight = FontWeights.Bold,
                BorderThickness = new Thickness(0),
                DropDownBorderThickness = new Thickness(3),
                FlowDirection = FlowDirection.LeftToRight,
                DisplayColorAndName = true,
                Width = 100,
                ColorMode = ColorMode.ColorCanvas,
                ShowTabHeaders = false,
                ShowDropDownButton = false
            };

            colorPicker.SelectedColorChanged += (sender, e) => StrokeColor = e.NewValue != null ? new SolidColorBrush(e.NewValue.Value) : StrokeColor;

            var sizePicker = new ComboBox { IsEditable = true };

            for (uint i = 4; i <= 64; i *= 2)
                sizePicker.Items.Add(i);

            sizePicker.SelectedIndex = 0;
            sizePicker.AddHandler(System.Windows.Controls.Primitives.TextBoxBase.TextChangedEvent,
                      new TextChangedEventHandler((sender, e) =>
                      {
                          try
                          {
                              StrokeThickness = UInt32.Parse(sizePicker.Text);
                          }
                          catch (Exception)
                          {

                          }
                      }));

            sizePicker.SelectionChanged += (sender, e) =>
            {
                if (sizePicker.SelectedValue != null)
                    StrokeThickness = (uint)sizePicker.SelectedValue;
            };

            SettingsUIElements = new List<UIElement>
            {
                colorPicker,
                sizePicker
            };
        }

        public List<UIElement> SettingsUIElements { get; }

        public abstract Cursor ContextualCursor(Point position);

        public abstract void KeyDown(object sender, KeyEventArgs e);

        public abstract void KeyUp(object sender, KeyEventArgs e);

        public abstract void MouseDown(object sender, MouseButtonEventArgs e);

        public abstract void MouseMove(object sender, MouseEventArgs e);

        public abstract void MouseUp(object sender, MouseButtonEventArgs e);
    }
}
