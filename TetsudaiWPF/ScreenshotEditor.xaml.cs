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
using System.Windows.Shapes;
using System.Drawing;
using System.IO;
using System.Windows.Controls.Primitives;
using Microsoft.Windows.Themes;
using System.ComponentModel;
using System.Windows.Forms;
using IronOcr;

namespace TetsudaiWPF
{
    enum EditPhase
    {
        Tool,
        ResizingNW,
        ResizingNWE,
        ResizingNE,
        ResizingSW,
        ResizingSWE,
        ResizingSE,
        ResizingWNS,
        ResizingENS
    }

    /// <summary>
    /// Interaction logic for ScreenshotEditor.xaml
    /// </summary>
    public partial class ScreenshotEditor : Window
    {
        private readonly ImageSource screenshot;
        private EditTool tool;
        private EditPhase editPhase;
        private List<MenuItem> registeredTools;
        private BindingList<UndoableEditAction> undoList;
        private BindingList<UndoableEditAction> redoList;
        private readonly double maxResizeDistance = 8;

        public ScreenshotEditor(Bitmap screenshotBmp)
        {
            this.screenshot = ConvertBmp(screenshotBmp);
            editPhase = EditPhase.Tool;
            registeredTools = new List<MenuItem>();
            undoList = new BindingList<UndoableEditAction>();
            redoList = new BindingList<UndoableEditAction>();

            undoList.ListChanged += (sender, e) => 
            {
                undoItem.IsEnabled = undoList.Count > 0;

                if (e.ListChangedType == ListChangedType.ItemAdded && !undoList[undoList.Count - 1].WasRedone)
                    undoList[undoList.Count - 1].OnAddedToUndoList(redoList);
            };
            redoList.ListChanged += (sender, e) => redoItem.IsEnabled = redoList.Count > 0;

            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.Left = 0;
            this.Top = 0;
            this.Width = SystemParameters.VirtualScreenWidth;
            this.Height = SystemParameters.VirtualScreenHeight;
            this.Cursor = System.Windows.Input.Cursors.Cross;

            tool = new MoveTool(canvas, selection);

            var brush = new ImageBrush();

            brush.ImageSource = screenshot;
            this.drawingArea.Background = brush;
            shade.Rect = new Rect(new System.Windows.Point(this.Left, this.Top), new System.Windows.Point(this.Width, this.Height));
        }
        private BitmapImage ConvertBmp(Bitmap bmp)
        {
            BitmapImage imageSource = new BitmapImage();

            using (var ms = new MemoryStream())
            {
                bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
                imageSource.BeginInit();
                imageSource.CacheOption = BitmapCacheOption.OnLoad;
                ms.Seek(0, SeekOrigin.Begin);
                imageSource.StreamSource = ms;
                imageSource.EndInit();
            }

            return imageSource;
        }

        private bool isWithinResizeTolerance(System.Windows.Point position, System.Windows.Point offset) => editPhase == EditPhase.Tool && System.Windows.Point.Subtract(position, offset).Length <= maxResizeDistance;

        private void HideResizingIndicators()
        {
            cornerNW.Visibility = Visibility.Hidden;
            cornerNE.Visibility = Visibility.Hidden;
            cornerSW.Visibility = Visibility.Hidden;
            cornerSE.Visibility = Visibility.Hidden;
            cornerNWE.Visibility = Visibility.Hidden;
            cornerSWE.Visibility = Visibility.Hidden;
            cornerENS.Visibility = Visibility.Hidden;
            cornerWNS.Visibility = Visibility.Hidden;
        }

        private void ShowResizingIndicators()
        {
            var rectCopy = selection.Rect;

            Canvas.SetLeft(cornerNW, rectCopy.Left - cornerNW.Width / 2);
            Canvas.SetTop(cornerNW, rectCopy.Top - cornerNW.Height / 2);
            Canvas.SetLeft(cornerNE, rectCopy.Left + rectCopy.Width - cornerNW.Width / 2);
            Canvas.SetTop(cornerNE, rectCopy.Top - cornerNW.Height / 2);
            Canvas.SetLeft(cornerSW, rectCopy.Left - cornerNW.Width / 2);
            Canvas.SetTop(cornerSW, rectCopy.Top + rectCopy.Height - cornerNW.Height / 2);
            Canvas.SetLeft(cornerSE, rectCopy.Left + rectCopy.Width - cornerNW.Width / 2);
            Canvas.SetTop(cornerSE, rectCopy.Top + rectCopy.Height - cornerNW.Height / 2);
            Canvas.SetLeft(cornerNWE, rectCopy.Left + rectCopy.Width / 2 - cornerNW.Width / 2);
            Canvas.SetTop(cornerNWE, rectCopy.Top - cornerNW.Height / 2);
            Canvas.SetLeft(cornerSWE, rectCopy.Left + rectCopy.Width / 2 - cornerNW.Width / 2);
            Canvas.SetTop(cornerSWE, rectCopy.Top + rectCopy.Height - cornerNW.Height / 2);
            Canvas.SetLeft(cornerWNS, rectCopy.Left - cornerNW.Width / 2);
            Canvas.SetTop(cornerWNS, rectCopy.Top + rectCopy.Height / 2 - cornerNW.Height / 2);
            Canvas.SetLeft(cornerENS, rectCopy.Left + rectCopy.Width - cornerNW.Width / 2);
            Canvas.SetTop(cornerENS, rectCopy.Top + rectCopy.Height / 2 - cornerNW.Height / 2);

            cornerNW.Visibility = Visibility.Visible;
            cornerNE.Visibility = Visibility.Visible;
            cornerSW.Visibility = Visibility.Visible;
            cornerSE.Visibility = Visibility.Visible;
            cornerNWE.Visibility = Visibility.Visible;
            cornerSWE.Visibility = Visibility.Visible;
            cornerENS.Visibility = Visibility.Visible;
            cornerWNS.Visibility = Visibility.Visible;
        }

        private void UpdateCursor(System.Windows.Point position)
        {
            var rectCopy = selection.Rect;

            if ((cornerNW.IsVisible && isWithinResizeTolerance(rectCopy.TopLeft, position)) || (cornerSE.IsVisible && isWithinResizeTolerance(rectCopy.BottomRight, position)))
                this.Cursor = System.Windows.Input.Cursors.SizeNWSE;
            else if ((cornerNE.IsVisible && isWithinResizeTolerance(rectCopy.TopRight, position)) || (cornerSW.IsVisible && isWithinResizeTolerance(selection.Rect.BottomLeft, position)))
                this.Cursor = System.Windows.Input.Cursors.SizeNESW;
            else if ((cornerWNS.IsVisible && isWithinResizeTolerance(rectCopy.TopLeft + (Vector)new System.Windows.Point(0, rectCopy.Height / 2), position)) || (cornerENS.IsVisible && isWithinResizeTolerance(rectCopy.TopRight + (Vector)new System.Windows.Point(0, rectCopy.Height / 2), position)))
                this.Cursor = System.Windows.Input.Cursors.SizeWE;
            else if ((cornerNWE.IsVisible && isWithinResizeTolerance(rectCopy.TopLeft + (Vector)new System.Windows.Point(rectCopy.Width / 2, 0), position)) || (cornerSWE.IsVisible && isWithinResizeTolerance(rectCopy.BottomLeft + (Vector)new System.Windows.Point(rectCopy.Width / 2, 0), position)))
                this.Cursor = System.Windows.Input.Cursors.SizeNS;
            else if ((featureBar.Visibility == Visibility.Visible && new Rect(new System.Windows.Point(Canvas.GetLeft(featureBar), Canvas.GetTop(featureBar)), featureBar.RenderSize).Contains(position)) || (editBar.Visibility == Visibility.Visible && new Rect(new System.Windows.Point(Canvas.GetLeft(editBar), Canvas.GetTop(editBar)), editBar.RenderSize).Contains(position)) || (undoRedoBar.Visibility == Visibility.Visible && new Rect(new System.Windows.Point(Canvas.GetLeft(undoRedoBar), Canvas.GetTop(undoRedoBar)), undoRedoBar.RenderSize).Contains(position)) || (toolSettingsPanel.Visibility == Visibility.Visible && new Rect(new System.Windows.Point(Canvas.GetLeft(toolSettingsPanel), Canvas.GetTop(toolSettingsPanel)), toolSettingsPanel.RenderSize).Contains(position)))
                this.Cursor = System.Windows.Input.Cursors.Arrow;
            else
                this.Cursor = tool.ContextualCursor(position);
        }

        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                this.Close();
            else if (e.Key == Key.C && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && featureBar.Visibility == Visibility.Visible)
                SetClipboard();
            else if (e.Key == Key.Z && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && featureBar.Visibility == Visibility.Visible)
                Undo();
            else if (e.Key == Key.Y && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && featureBar.Visibility == Visibility.Visible)
                Redo();
            else if (e.Key == Key.S && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && featureBar.Visibility == Visibility.Visible)
                SaveScreenshot();
            else if (e.Key == Key.J && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && featureBar.Visibility == Visibility.Visible)
                UseTetsudai();
            else
                tool.KeyDown(sender, e);
        }

        private void UseTetsudai()
        {
            Cursor = System.Windows.Input.Cursors.Wait;
            ForceCursor = true;
            IsEnabled = false;

            RenderTargetBitmap rtb = new RenderTargetBitmap((int)drawingArea.Width, (int)drawingArea.Height, 96d, 96d, PixelFormats.Default);

            rtb.Render(drawingArea);

            var cropped = new CroppedBitmap(rtb, new Int32Rect((int)selection.Rect.X, (int)selection.Rect.Y, (int)selection.Rect.Width, (int)selection.Rect.Height));

            new Tetsudai(cropped).Show();

            Close();
        }

        private void SaveScreenshot()
        {
            var dialog = new SaveFileDialog{ 
                Filter = "Images|*.png",
                DefaultExt = "png"
            };

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                RenderTargetBitmap rtb = new RenderTargetBitmap((int)drawingArea.Width, (int)drawingArea.Height, 96d, 96d, PixelFormats.Default);

                rtb.Render(drawingArea);

                var cropped = new CroppedBitmap(rtb, new Int32Rect((int)selection.Rect.X, (int)selection.Rect.Y, (int)selection.Rect.Width, (int)selection.Rect.Height));
                byte[] data;
                JpegBitmapEncoder encoder = new JpegBitmapEncoder();

                encoder.Frames.Add(BitmapFrame.Create(cropped));

                using (MemoryStream ms = new MemoryStream())
                {
                    encoder.Save(ms);
                    data = ms.ToArray();
                }

                if (data != null)
                {
                    File.WriteAllBytes(dialog.FileName, data);
                    this.Close();
                }
            }
        }

        private void SetClipboard()
        {
            RenderTargetBitmap rtb = new RenderTargetBitmap((int)drawingArea.Width, (int)drawingArea.Height, 96d, 96d, PixelFormats.Default);

            rtb.Render(drawingArea);

            System.Windows.Clipboard.SetImage(new CroppedBitmap(rtb, new Int32Rect((int)selection.Rect.X, (int)selection.Rect.Y, (int)selection.Rect.Width, (int)selection.Rect.Height)));

            this.Close();
        }

        private void Window_Deactivated(object sender, EventArgs e) => this.Topmost = true;

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var position = e.GetPosition(this);
            var rectCopy = selection.Rect;

            if (isWithinResizeTolerance(rectCopy.TopLeft, position))
                editPhase = EditPhase.ResizingNW;
            else if (isWithinResizeTolerance(rectCopy.BottomRight, position))
                editPhase = EditPhase.ResizingSE;
            else if (isWithinResizeTolerance(rectCopy.TopRight, position))
                editPhase = EditPhase.ResizingNE;
            else if (isWithinResizeTolerance(selection.Rect.BottomLeft, position))
                editPhase = EditPhase.ResizingSW;
            else if (isWithinResizeTolerance(rectCopy.TopLeft + (Vector)new System.Windows.Point(0, rectCopy.Height / 2), position))
                editPhase = EditPhase.ResizingWNS;
            else if (isWithinResizeTolerance(rectCopy.TopRight + (Vector)new System.Windows.Point(0, rectCopy.Height / 2), position))
                editPhase = EditPhase.ResizingENS;
            else if (isWithinResizeTolerance(rectCopy.TopLeft + (Vector)new System.Windows.Point(rectCopy.Width / 2, 0), position))
                editPhase = EditPhase.ResizingNWE;
            else if (isWithinResizeTolerance(rectCopy.BottomLeft + (Vector)new System.Windows.Point(rectCopy.Width / 2, 0), position))
                editPhase = EditPhase.ResizingSWE;
            else if (!(toolSettingsPanel.Visibility == Visibility.Visible && new Rect(new System.Windows.Point(Canvas.GetLeft(toolSettingsPanel), Canvas.GetTop(toolSettingsPanel)), toolSettingsPanel.RenderSize).Contains(position)))
                tool.MouseDown(sender, e);

            if (!(toolSettingsPanel.Visibility == Visibility.Visible && new Rect(new System.Windows.Point(Canvas.GetLeft(toolSettingsPanel), Canvas.GetTop(toolSettingsPanel)), toolSettingsPanel.RenderSize).Contains(position)))
            {
                HideResizingIndicators();
                featureBar.Visibility = Visibility.Hidden;
                editBar.Visibility = Visibility.Hidden;
                toolSettingsPanel.Visibility = Visibility.Hidden;
                undoRedoBar.Visibility = Visibility.Hidden;
            }
        }

        private void Window_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (editPhase == EditPhase.Tool)
                tool.MouseUp(sender, e);
            else
                editPhase = EditPhase.Tool;

            ShowResizingIndicators();
            MoveSelectionMenus();
            featureBar.Visibility = Visibility.Visible;
            editBar.Visibility = Visibility.Visible;
            undoRedoBar.Visibility = Visibility.Visible;
            toolSettingsPanel.Visibility = toolSettingsPanel.Children.Count > 0 ? Visibility.Visible : Visibility.Hidden;
            UpdateCursor(e.GetPosition(this));
        }

        private void Window_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var mousePosition = e.GetPosition(this);
            var rectCopy = selection.Rect;
            
            switch (editPhase)
            {
                case EditPhase.ResizingNW:
                    selection.Rect = new Rect(new System.Windows.Point(Math.Min(mousePosition.X, rectCopy.Right), Math.Min(mousePosition.Y, rectCopy.Bottom)), rectCopy.BottomRight);
                    break;
                case EditPhase.ResizingNWE:
                    selection.Rect = new Rect(new System.Windows.Point(rectCopy.X, Math.Min(mousePosition.Y, rectCopy.Bottom)), rectCopy.BottomRight);
                    break;
                case EditPhase.ResizingNE:
                    selection.Rect = new Rect(new System.Windows.Point(Math.Max(mousePosition.X, rectCopy.Left), Math.Min(mousePosition.Y, rectCopy.Bottom)), rectCopy.BottomLeft);
                    break;
                case EditPhase.ResizingSW:
                    selection.Rect = new Rect(new System.Windows.Point(Math.Min(mousePosition.X, rectCopy.Right), Math.Max(mousePosition.Y, rectCopy.Top)), rectCopy.TopRight);
                    break;
                case EditPhase.ResizingSWE:
                    selection.Rect = new Rect(new System.Windows.Point(rectCopy.X, Math.Max(mousePosition.Y, rectCopy.Top)), rectCopy.TopRight);
                    break;
                case EditPhase.ResizingSE:
                    selection.Rect = new Rect(new System.Windows.Point(Math.Max(mousePosition.X, rectCopy.Left), Math.Max(mousePosition.Y, rectCopy.Top)), rectCopy.TopLeft);
                    break;
                case EditPhase.ResizingWNS:
                    selection.Rect = new Rect(new System.Windows.Point(Math.Min(mousePosition.X, rectCopy.Right), rectCopy.Top), rectCopy.BottomRight);
                    break;
                case EditPhase.ResizingENS:
                    selection.Rect = new Rect(new System.Windows.Point(Math.Max(mousePosition.X, rectCopy.Left), rectCopy.Top), rectCopy.BottomLeft);
                    break;
                default:
                    UpdateCursor(mousePosition);
                    tool.MouseMove(sender, e);
                    break;
            }
        }

        private void MoveSelectionMenus()
        {
            var rectCopy = selection.Rect;

            Canvas.SetTop(featureBar, rectCopy.Bottom + 10);
            Canvas.SetLeft(featureBar, rectCopy.Left);

            Canvas.SetTop(editBar, rectCopy.Top);
            Canvas.SetLeft(editBar, rectCopy.Right + 10);

            Canvas.SetTop(undoRedoBar, rectCopy.Bottom + 10);
            Canvas.SetLeft(undoRedoBar, rectCopy.Right - undoRedoBar.ActualWidth);

            Canvas.SetTop(toolSettingsPanel, rectCopy.Top);
            Canvas.SetLeft(toolSettingsPanel, rectCopy.Right + editBar.ActualWidth + 20);
        }

        private void CopyMenuItem_Click(object sender, RoutedEventArgs e) => SetClipboard();

        private void CancelMenuItem_Click(object sender, RoutedEventArgs e) => this.Close();

        private void selectTool(MenuItem tool)
        {
            registeredTools.ForEach(item => { 
                item.Background = (System.Windows.Media.Brush)editBar.FindResource("deselectBrush");
                item.IsEnabled = true;
            });

            tool.Background = (System.Windows.Media.Brush)editBar.FindResource("selectBrush");
            tool.IsEnabled = false;
        }

        private void editBar_Loaded(object sender, RoutedEventArgs e)
        {
            editBar.Items.OfType<MenuItem>().ToList().ForEach(item => {
                item.Click += (sender, e) => selectTool(item);
                registeredTools.Add(item);
                });
        }

        private void SetTool(EditTool newTool)
        {
            tool = newTool;
            toolSettingsPanel.Children.Clear();
            tool.SettingsUIElements.ForEach(uie => toolSettingsPanel.Children.Add(uie));
            toolSettingsPanel.Visibility = toolSettingsPanel.Children.Count > 0 ? Visibility.Visible : Visibility.Hidden;
        }

        private void PenMenuItem_Click(object sender, RoutedEventArgs e) => SetTool(new PenTool(drawingArea, undoList));


        private void LineMenuItem_Click(object sender, RoutedEventArgs e) => SetTool(new LineTool(drawingArea, undoList));
        private void ArrowMenuItem_Click(object sender, RoutedEventArgs e) => SetTool(new ArrowTool(drawingArea, undoList));
        private void SquareMenuItem_Click(object sender, RoutedEventArgs e) => SetTool(new RectangleTool(drawingArea, undoList));
        private void TextMenuItem_Click(object sender, RoutedEventArgs e)
        {
            // TODO!
        }
        private void MoveMenuItem_Click(object sender, RoutedEventArgs e) => SetTool(new MoveTool(canvas, selection));

        private void Window_KeyUp(object sender, System.Windows.Input.KeyEventArgs e) => tool.KeyUp(sender, e);

        private void UndoItem_Click(object sender, RoutedEventArgs e) => Undo();

        private void RedoItem_Click(object sender, RoutedEventArgs e) => Redo();

        private void Undo()
        {
            if (undoList.Count > 0)
            {
                var undoAction = undoList[undoList.Count - 1];

                undoAction.Undo();
                undoList.RemoveAt(undoList.Count - 1);
                redoList.Add(undoAction);
            }
        }

        private void  Redo()
        {
            if (redoList.Count > 0)
            {
                var redoAction = redoList[redoList.Count - 1];

                redoAction.Redo();
                redoList.RemoveAt(redoList.Count - 1);
                undoList.Add(redoAction);
            }
        }

        private void SaveMenuItem_Click(object sender, RoutedEventArgs e)
        {
            SaveScreenshot();
        }

        private void TetsudaiMenuItem_Click(object sender, RoutedEventArgs e)
        {
            UseTetsudai();
        }
    }
}
