using System;
using System.Drawing;
using Yoloanno.Types;

namespace Yoloanno.Yolo
{
    public class YoloRegion
    {
        public int RegionIndex;
        public float YoloX;
        public float YoloY;
        public float YoloWidth;
        public float YoloHeight;

        public static YoloRegion RegionFromRectangle(RectangleF rectangle, SizeF imageSize)
        {
            var result = new YoloRegion();
            result.Apply(rectangle, imageSize);
            return result;
        }

        public static YoloRegion RegionFromTrainData(string data)
        {
            var result = new YoloRegion();
            result.ApplyFromStr(data);
            return result;
        }

        public override string ToString()
        {
            return string.Format("{0} {1} {2} {3} {4}", RegionIndex, YoloX, YoloY, YoloWidth, YoloHeight);
        }

        public void Apply(RectangleF rectangle, SizeF imageSize)
        {
            YoloX = rectangle.X / imageSize.Width;
            YoloY = rectangle.Y / imageSize.Height;
            YoloWidth = rectangle.Width / imageSize.Width;
            YoloHeight = rectangle.Height / imageSize.Height;
        }

        public Rectangle GetRectangle(Size imageSize)
        {
            return new Rectangle(
                (int)Convert.ToDecimal(YoloX * imageSize.Width),
                (int)Convert.ToDecimal(YoloY * imageSize.Height),
                (int)Convert.ToDecimal(YoloWidth * imageSize.Width),
                (int)Convert.ToDecimal(YoloHeight * imageSize.Height)
                );
        }

        public void ApplyFromStr(string yoloTrainDataStr)
        {
            string[] @params;
            if (string.IsNullOrEmpty(yoloTrainDataStr) || (@params = yoloTrainDataStr.Split(new char[] { ' ' },StringSplitOptions.RemoveEmptyEntries)).Length != 5) return;
            RegionIndex = Convert.ToInt16(@params[0]);
            YoloX = float.Parse(@params[1]);
            YoloY = float.Parse(@params[2]);
            YoloWidth = float.Parse(@params[3]);
            YoloHeight = float.Parse(@params[4]);
        }

        public Category UiCategory
        {
            get
            {
                return Categories.CategoriesArray[RegionIndex];
            }
        }
    }
}
