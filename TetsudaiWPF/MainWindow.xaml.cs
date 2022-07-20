using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Drawing;

namespace TetsudaiWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private System.Windows.Forms.NotifyIcon trayIcon;
        private KeyboardHook globalHook;
        private bool isEditing;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Initialized(object sender, EventArgs e)
        {
            trayIcon = new System.Windows.Forms.NotifyIcon();
            trayIcon.Icon = System.Drawing.Icon.FromHandle(TetsudaiWPF.Properties.Resources.tetsudai32.GetHicon());
            trayIcon.MouseClick += new System.Windows.Forms.MouseEventHandler(HandleClick);

            globalHook = new KeyboardHook();
            globalHook.KeyPressed += new System.EventHandler<KeyPressedEventArgs>(HandleGlobalKey);
            globalHook.RegisterHotKey(GlobalModifierKeys.None, System.Windows.Forms.Keys.PrintScreen);
            
            trayIcon.Visible = true;
            isEditing = false;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // todo menu
        }

        private void HandleClick(object sender, System.Windows.Forms.MouseEventArgs e) // todo rightclick menu!
        {
            switch (e.Button)
            {
                case System.Windows.Forms.MouseButtons.Left:
                    TakeScreenshot();
                    break;
            }
        }

        private void HandleGlobalKey(object sender, KeyPressedEventArgs e)
        {
            TakeScreenshot();
        }

        private void TakeScreenshot()
        {
            if (isEditing)
                return;

            isEditing = true;

            using (var bmp = new Bitmap((int) SystemParameters.VirtualScreenWidth, (int) SystemParameters.VirtualScreenHeight))
            {
                using (var g = Graphics.FromImage(bmp))
                    g.CopyFromScreen((int) SystemParameters.VirtualScreenLeft, (int) SystemParameters.VirtualScreenTop, 0, 0, bmp.Size); 

                var editor = new ScreenshotEditor(bmp);

                editor.ShowDialog();
            }

            isEditing = false;
        }
    }
}
