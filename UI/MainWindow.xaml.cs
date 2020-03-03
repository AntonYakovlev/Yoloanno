using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Yoloanno.Config;
using Yoloanno.Tools;
using Yoloanno.Types;
using Yoloanno.Yolo;

namespace Yoloanno.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        bool isDown, isDragging, isSelected;
        UIElement selectedElement = null;
        double originalLeft, originalTop;
        System.Windows.Point startPoint;

        AdornerLayer adornerLayer;
        List<DetectionRegion> _regions = new List<DetectionRegion>();

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
            categoriesCb.ItemsSource = Categories.CategoriesArray;
            categoriesCb.SelectedIndex = 0;
            LoadConfig();            
        }

        private void LoadConfig()
        {
            var cfg = UIConfig.Instance;
            SizeTemplatesLb.ItemsSource = cfg.RegionTemplates;
            fileSystemPathTb.Text = cfg.FileSystemPath;
            fileNameNumber.Text = cfg.LastFileNumber;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            //registering mouse events
            MouseLeftButtonDown += MainWindow_MouseLeftButtonDown;
            MouseLeftButtonUp += MainWindow_MouseLeftButtonUp;
            MouseMove += MainWindow_MouseMove;
            MouseLeave += MainWindow_MouseLeave;

            myCanvas.PreviewMouseLeftButtonDown += MyCanvas_PreviewMouseLeftButtonDown;
            myCanvas.PreviewMouseLeftButtonUp += MyCanvas_PreviewMouseLeftButtonUp;
        }

        private void StopDragging()
        {
            if (isDown)
            {
                isDown = isDragging = false;
            }
        }


        private void MyCanvas_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            StopDragging();            
            e.Handled = e.Source is Frame;
        }

        private void MyCanvas_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (DoSkipMouseDown(e.Source)) return;
            //removing selected element
            RemoveAdorner();

            // select element if any element is clicked other then canvas
            if (e.Source != myCanvas && e.Source is Frame)
            {
                isDown = sender is Canvas;
                startPoint = e.GetPosition(myCanvas);
                SelectControl((Frame)e.Source);
                e.Handled = true;
            }            
        }

        private void MainWindow_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (DoSkipMouseDown(e.Source)) return;
            //remove selected element on mouse down
            RemoveAdorner();
        }

        private bool DoSkipMouseDown(object source)
        {
            return !(source is Frame);
        }

        private void RemoveAdorner()
        {
            if (isSelected)
            {
                isSelected = false;
                if (selectedElement != null)
                {
                    adornerLayer.Remove(adornerLayer.GetAdorners(selectedElement)[0]);
                    selectedElement = null;
                }
            }
        }

        private void SelectControl(Frame element)
        {
            selectedElement = element;

            UpdateSizeFields();

            originalLeft = Canvas.GetLeft(selectedElement);
            originalTop = Canvas.GetTop(selectedElement);

            //adding adorner on selected element
            adornerLayer = AdornerLayer.GetAdornerLayer(selectedElement);
            adornerLayer.Add(new BorderAdorner(selectedElement));
            isSelected = true;

            RegionsListBox.SelectedItem = _regions.First(itm => itm.Frame == selectedElement);

            RegionPreviewWnd.Instance.UpdatePreview(GetActiveRegionSelection(), SelectedRegion);
        }

        private void MainWindow_MouseMove(object sender, MouseEventArgs e)
        {
            //handling mouse move event and setting canvas top and left value based on mouse movement
            if (isDown && selectedElement != null)
            {
                if ((!isDragging) &&
                    ((Math.Abs(e.GetPosition(myCanvas).X - startPoint.X) > SystemParameters.MinimumHorizontalDragDistance) ||
                    (Math.Abs(e.GetPosition(myCanvas).Y - startPoint.Y) > SystemParameters.MinimumVerticalDragDistance)))
                    isDragging = true;

                if (isDragging)
                {
                    System.Windows.Point position = Mouse.GetPosition(myCanvas);
                    Canvas.SetTop(selectedElement, position.Y - (startPoint.Y - originalTop));
                    Canvas.SetLeft(selectedElement, position.X - (startPoint.X - originalLeft));
                    UpdateSizeFields();
                    RegionPreviewWnd.Instance.UpdatePreview(GetActiveRegionSelection(), SelectedRegion);
                }                
            }
        }

        private void UpdateSizeFields()
        {
            if (selectedElement != null)
            {
                widthTb.Text = "" + ((Frame)selectedElement).Width;
                heightTb.Text = "" + ((Frame)selectedElement).Height;
                topTb.Text = "" + Canvas.GetTop(selectedElement);
                LeftTb.Text = "" + Canvas.GetLeft(selectedElement);
            }
            UpdateYoloAnnotation();
            FlushRegionsToYoloData();
        }

        private void WidthTb_KeyDown(object sender, KeyEventArgs e)
        {
            UpdateSelectedFrameSize(sender, e);
        }

        private void UpdateSelectedFrameSize(object sender, KeyEventArgs e)
        {
            bool doUpdate;

            switch (e.Key)
            {
                case Key.Enter:
                    doUpdate = selectedElement != null;
                    break;
                case Key.OemMinus:
                    ((TextBox)sender).Text = "" + (int.Parse(((TextBox)sender).Text) - 1);
                    doUpdate = selectedElement != null;
                    break;
                case Key.OemPlus:
                    ((TextBox)sender).Text = "" + (int.Parse(((TextBox)sender).Text) + 1);
                    doUpdate = selectedElement != null;
                    break;
                default:
                    doUpdate = false;
                    break;
            }

            if (doUpdate)
            {
                ((Frame)selectedElement).Width = FieldWidth;
                ((Frame)selectedElement).Height = Convert.ToDouble(heightTb.Text);
                Canvas.SetTop(selectedElement, Convert.ToDouble(topTb.Text));
                Canvas.SetLeft(selectedElement, Convert.ToDouble(LeftTb.Text));
            }
        }

        private void WidthTb_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsTextAllowed(e.Text);
        }

        private static readonly Regex _regex = new Regex("[^0-9.-]+"); //regex that matches disallowed text
        private static bool IsTextAllowed(string text)
        {
            return !_regex.IsMatch(text);
        }        

        private void Frame_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateSizeFields();
            UpdateYoloAnnotation();
            RegionPreviewWnd.Instance.UpdatePreview(GetActiveRegionSelection(), SelectedRegion);
        }        

        private void CheckMoveRegion(object sender, KeyEventArgs e)
        {
            var keys = new List<Key> { Key.Down, Key.Up, Key.Left, Key.Right};
            if (selectedElement != null && keys.Contains(e.Key))
            {
                bool changed = false;
                switch (e.Key)
                {
                    case Key.Down:
                        changed = true;
                        Canvas.SetTop(selectedElement, ((double)selectedElement.GetValue(Canvas.TopProperty)) + 1);
                        break;
                    case Key.Up:
                        changed = true;
                        Canvas.SetTop(selectedElement, ((double)selectedElement.GetValue(Canvas.TopProperty)) - 1);
                        break;
                    case Key.Left:
                        changed = true;
                        Canvas.SetLeft(selectedElement, ((double)selectedElement.GetValue(Canvas.LeftProperty)) - 1);
                        break;
                    case Key.Right:
                        changed = true;
                        Canvas.SetLeft(selectedElement, ((double)selectedElement.GetValue(Canvas.LeftProperty)) + 1);
                        break;
                }
                if (changed)
                {
                    UpdateSizeFields();
                    RegionPreviewWnd.Instance.UpdatePreview(GetActiveRegionSelection(), SelectedRegion);
                }
                e.Handled = !myCanvas.Children.OfType<TextBox>().Any(itm => itm.IsFocused);
            }
            switch(e.Key)
            {
                case Key.Add:
                    NextBtn_Click(null, new RoutedEventArgs());
                    break;
                case Key.Subtract:
                    PrevBtn_Click(null, new RoutedEventArgs());
                    break;
                case Key.Delete:
                    RemoveRegionBtn_Click(null, new RoutedEventArgs());
                    break;
            }            
        }

        private void RemoveBtn_Click(object sender, RoutedEventArgs e)
        {            
            if(SizeTemplatesLb.SelectedItems.Count > 0)
            {
                var selectedSize = (RegionSize)SizeTemplatesLb.SelectedItem;
                var cfg = UIConfig.Instance;
                cfg.RegionTemplates = new List<RegionSize>(cfg.RegionTemplates.Where(itm => "" + itm != "" + selectedSize).ToList());
                cfg.Save();
                LoadConfig();
            }
        }

        private void SizeTemplatesLb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            removeBtn.IsEnabled = SizeTemplatesLb.SelectedItems.Count > 0;            
        }

        private void SizeTemplatesLb_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (SizeTemplatesLb.SelectedItems.Count > 0)
            {
                var size = (RegionSize)SizeTemplatesLb.SelectedItem;
                if (selectedElement != null)
                {
                    ((Frame)selectedElement).Width = size.Width;
                    ((Frame)selectedElement).Height = size.Height;
                }
                else
                {
                    widthTb.Text = "" + size.Width;
                    heightTb.Text = "" + size.Height;
                }
            }            
        }

        private double FieldWidth
        {
            get
            {
                double result = 100;
                if (widthTb.Text != string.Empty)
                {
                    var parsed = Convert.ToDouble(widthTb.Text);
                    if (parsed > 0)
                    {
                        result = parsed;
                    }
                }
                return result;
            }
        }

        private double FieldHeight
        {
            get
            {
                double result = 100;
                if (heightTb.Text != string.Empty)
                {
                    var parsed = Convert.ToDouble(heightTb.Text);
                    if (parsed > 0)
                    {
                        result = parsed;
                    }
                }
                return result;
            }
        }

        private Category FieldCategory
        {
            get
            {
                return (Category)categoriesCb.SelectedItem;
            }
        }

        private void AddRegionBtn_Click(object sender, RoutedEventArgs e)
        {
            int x = 100, y = 100;
            if(LastRegion != null)
            {
                x = (int)Canvas.GetLeft(LastRegion.Frame) + 10;
                y = (int)Canvas.GetTop(LastRegion.Frame);
            }
            AddRegion(x, y, FieldWidth, FieldHeight, FieldCategory);                        
            FlushRegionsToYoloData();
            RegionPreviewWnd.Instance.UpdatePreview(GetActiveRegionSelection(), SelectedRegion);
        }

        private DetectionRegion LastRegion
        {
            get
            {
                if(_regions.Count > 0)
                {
                    return _regions.Last();
                }
                return null;
            }
        }

        private void FlushRegionsToYoloData()
        {            
            if (_regions.Count != 0)
            {
                var lines = _regions.Select(rgn => rgn.YoloRegion.ToString()).ToArray();
                File.WriteAllLines(Path.Combine(fileSystemPathTb.Text, yoloNameTb.Text), lines);
            }
            else
            {
                var filePath = FileSystemHelper.GetCheckedFilePath(fileSystemPathTb.Text, yoloNameTb.Text);
                if (filePath != string.Empty)
                {
                    File.Delete(filePath);
                }
            }
        }

        private DetectionRegion AddRegion(int x, int y, double width, double height, Category category)
        {
            var maxPrevNumber = _regions.Count > 0 ? _regions.Max(itm => itm.MyOrderNum) : 0;
            var region = new DetectionRegion(myCanvas, x,y,width, height, maxPrevNumber, category, Frame_SizeChanged);
            _regions.Add(region);
            UpdateRegionsList();
            SelectControl(region.Frame);
            return region;
        }

        private BitmapSource GetActiveRegionSelection()
        {
            if(selectedElement != null)
            {
                var image = TryLoadImage();
                if(!ActiveRegionRectangle.IsEmpty && IsContainedInRectangle(ImageRectangle,ActiveRegionRectangle) && image != null)
                {                    
                    return new CroppedBitmap(image, ActiveRegionRectangle);
                }
            }
            return null;
        }

        private Int32Rect ImageRectangle
        {
            get
            {
                return new Int32Rect(
                        0,
                        0,
                        (int)MarkableImage.ActualWidth,
                        (int)MarkableImage.ActualHeight);
            }
        }

        private Int32Rect ActiveRegionRectangle
        {
            get
            {
                if (selectedElement != null)
                {
                    return new Int32Rect(
                        (int)Canvas.GetLeft(selectedElement),
                        (int)Canvas.GetTop(selectedElement),
                        (int)((Frame)selectedElement).ActualWidth,
                        (int)((Frame)selectedElement).ActualHeight);
                }
                return new Int32Rect();
            }
        }

        private bool IsContainedInRectangle(Int32Rect rectangleContainer, Int32Rect rectangleContainee)
        {
            return new Rectangle(rectangleContainer.X, rectangleContainer.Y, rectangleContainer.Width, rectangleContainer.Height).
                Contains(new Rectangle(rectangleContainee.X, rectangleContainee.Y, rectangleContainee.Width, rectangleContainee.Height)) && 
                rectangleContainee.Width != 0 && 
                rectangleContainee.Height != 0;
        }

        private void RemoveRegionBtn_Click(object sender, RoutedEventArgs e)
        {
            if (RegionsListBox.SelectedItems.Count > 0 && selectedElement != null)
            {
                isSelected = false;
                selectedElement = null;
                var region = (DetectionRegion)RegionsListBox.SelectedItem;
                region.RemoveFromCanvas();
                _regions.Remove(region);
                UpdateRegionsList();
                FlushRegionsToYoloData();
            }
        }

        private void UpdateRegionsList()
        {
            RegionsListBox.ItemsSource = null;
            RegionsListBox.ItemsSource = _regions;
            clearAllBtn.IsEnabled = RegionsListBox.Items.Count > 0;
            UpdateYoloAnnotation();
        }

        private void UpdateYoloAnnotation()
        {
            yoloRegionsTb.Clear();
            foreach (var region in _regions)
            {
                yoloRegionsTb.Text += region.YoloRegion + Environment.NewLine;
            }
        }

        private void RegionsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            removeRegionBtn.IsEnabled = RegionsListBox.SelectedItems.Count > 0;            
            if (RegionsListBox.SelectedItem != null)
            {
                var region = (DetectionRegion)RegionsListBox.SelectedItem;
                MyCanvas_PreviewMouseLeftButtonDown(sender, new MouseButtonEventArgs(
                    Mouse.PrimaryDevice, 0, MouseButton.Left) {
                    RoutedEvent = PreviewMouseLeftButtonDownEvent,
                    Source = region.Frame });
                SelectCategoryInCombo();
            }
        }

        private void ClearAllBtn_Click(object sender, RoutedEventArgs e)
        {
            ClearRegions();
        }

        private void ClearRegions(bool callFromUi = true)
        {
            isSelected = false;
            selectedElement = null;
            foreach (var region in _regions)
            {
                region.RemoveFromCanvas();
            }
            _regions.Clear();
            UpdateRegionsList();
            if (callFromUi)
            {
                FlushRegionsToYoloData();
            }
        }

        private void BrowseBtn_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                if (dialog.ShowDialog() != System.Windows.Forms.DialogResult.Cancel)
                {
                    fileSystemPathTb.Text = dialog.SelectedPath;
                }
                var cfg = UIConfig.Instance;
                cfg.FileSystemPath = fileSystemPathTb.Text;
                cfg.Save();
                TryLoadImage();
            }
        }        

        private void FileNameNumber_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (fileNameNumber.Text.Length == 0) return;
            imageNameTb.Text = FileSystemHelper.GetFileName(fileNameNumber.Text);
            yoloNameTb.Text = FileSystemHelper.GetFileName(fileNameNumber.Text,"txt");
            var cfg = UIConfig.Instance;
            cfg.LastFileNumber = fileNameNumber.Text;
            cfg.Save();
            TryLoadImage();            
        }

        private BitmapImage TryLoadImage()
        {
            BitmapImage result = null;
            var filePath = FileSystemHelper.GetCheckedFilePath(fileSystemPathTb.Text, imageNameTb.Text);
            if (filePath != string.Empty)
            {
                MarkableImage.Source = result = new BitmapImage(new Uri(filePath));
            }
            return result;
        }

        private bool TryLoadYoloData()
        {
            var filePath = FileSystemHelper.GetCheckedFilePath(fileSystemPathTb.Text, yoloNameTb.Text);
            if (filePath != string.Empty)
            {
                string[] lines = File.ReadAllLines(filePath);
                foreach(var line in lines)
                {
                    var regionFromLine = YoloRegion.RegionFromTrainData(line);
                    var rectangle = 
                        regionFromLine.GetRectangle(new System.Drawing.Size(ImageRectangle.Width, ImageRectangle.Height));
                    AddRegion(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height, regionFromLine.UiCategory);
                }                
                return true;
            }
            return false;
        }

        private void NextBtn_Click(object sender, RoutedEventArgs e)
        {
            int parseResult;
            if (!int.TryParse(fileNameNumber.Text, out parseResult)) return;
            fileNameNumber.Text = "" + (parseResult + 1);
            ClearRegions(false);
            TryLoadImage();
            TryLoadYoloData();
            RegionPreviewWnd.Instance.Hide();
        }

        private void PrevBtn_Click(object sender, RoutedEventArgs e)
        {
            int parseResult;
            if (!int.TryParse(fileNameNumber.Text, out parseResult)) return;
            fileNameNumber.Text = "" + (parseResult - 1);
            ClearRegions(false);
            TryLoadImage();
            TryLoadYoloData();
            RegionPreviewWnd.Instance.Hide();
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            TryLoadYoloData();
        }

        private void AddBtn_Click(object sender, RoutedEventArgs e)
        {
            var cfg = UIConfig.Instance;
            cfg.RegionTemplates.Add(new RegionSize
            {
                Width = Convert.ToDouble(widthTb.Text),
                Height = Convert.ToDouble(heightTb.Text)
            });
            cfg.Save();
            LoadConfig();
        }

        private void CategoriesCb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(SelectedRegion != null)
            {
                SelectedRegion.Category = FieldCategory;
                UpdateRegionsList();
                SelectRegionInList();
            }
        }

        private void MainWindow_MouseLeave(object sender, MouseEventArgs e)
        {
            //stop dragging on mouse leave
            StopDragging();
            e.Handled = true;
        }

        private void MainWindow_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            //stop dragging on mouse left button up
            StopDragging();
            e.Handled = true;
        }

        private void ZoomWndBtn_Click(object sender, RoutedEventArgs e)
        {
            RegionPreviewWnd.Instance.Owner = Window.GetWindow(this);
            if (!RegionPreviewWnd.Instance.IsVisible)
            {                
                RegionPreviewWnd.Instance.Show();
            }
            else
            {
                RegionPreviewWnd.Instance.Activate();
            }
            Activate();
        }

        private DetectionRegion SelectedRegion
        {
            get
            {
                if (selectedElement != null)
                {
                    return _regions.First(itm => itm.Frame == selectedElement);
                }
                return null;
            }
        }

        private void SelectRegionInList()
        {
            if(selectedElement != null)
            {
                RegionsListBox.SelectedItem = SelectedRegion;
            }
        }

        private void SelectCategoryInCombo()
        {
            if(selectedElement != null)
            {
                categoriesCb.SelectedItem = SelectedRegion.Category;
            }
        }
    }
}
