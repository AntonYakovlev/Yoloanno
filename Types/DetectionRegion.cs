using System;
using System.Drawing;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Yoloanno.Yolo;

namespace Yoloanno.Types
{
    public class DetectionRegion
    {
        private static readonly Random rnd = new Random();
        public Frame Frame;
        public System.Windows.Media.Color RegionColor;
        public Category Category;
        public int MyOrderNum;
        Canvas _parentCanvas;
        private YoloRegion _trainDataRegion;
        public DetectionRegion(Canvas parentCanvas, double x, double y, double regionWidth, double regionHeight, int maxOrderNum, Category category, SizeChangedEventHandler e)
        {
            _parentCanvas = parentCanvas;
            double width = regionWidth != 0 ? regionWidth : 100;
            double height = regionHeight != 0 ? regionHeight : 100;
            Category = category;
            RegionColor = System.Windows.Media.Color.FromRgb((byte)rnd.Next(128,256), (byte)rnd.Next(128,256), (byte)rnd.Next(128,256));
            MyOrderNum = maxOrderNum + 1;
            Frame = new Frame
            {
                Height = height,
                Width = width,
                BorderThickness = new Thickness(1, 1, 1, 1),
                BorderBrush = new SolidColorBrush(RegionColor),
                Content = "",
                IsTabStop = false                
            };
            Frame.SizeChanged += e;
            Canvas.SetLeft(Frame, x);
            Canvas.SetTop(Frame, y);            
            Frame.Name = "Region" + MyOrderNum;
            _parentCanvas.Children.Add(Frame);            
        }

        public void RemoveFromCanvas()
        {
            _parentCanvas.Children.Remove(Frame);
        }

        public override string ToString()
        {
            return string.Format("{0} - {1}", Frame.Name, Category);
        }

        public YoloRegion YoloRegion
        {
            get
            {
                var image = _parentCanvas.Children.OfType<System.Windows.Controls.Image>().First(img => img.Name == "MarkableImage");
                _trainDataRegion = YoloRegion.RegionFromRectangle(
                    new RectangleF(
                        (float)Canvas.GetLeft(Frame),
                        (float)Canvas.GetTop(Frame),
                        (float)Frame.Width,
                        (float)Frame.Height
                        ),
                    new SizeF((float)image.ActualWidth, (float)image.ActualHeight)
                    );
                _trainDataRegion.RegionIndex = Categories.CategoriesArray.IndexOf(Category);
                return _trainDataRegion;
            }
        }
    }
}
