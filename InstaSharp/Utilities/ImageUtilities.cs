using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace InstaSharp.Utilities
{
    public class ImageUtilities
    {
        private static Bitmap CropImage(Bitmap original, Rectangle cropArea)
        {
            return original.Clone(cropArea, original.PixelFormat);
        }

        public static Bitmap SquareWithBorder(Bitmap original, Brush fillBrush = null)
        {
            var largestDimension = Math.Max(original.Height, original.Width);
            var squareSize = new Size(largestDimension, largestDimension);
            var squareImage = new Bitmap(squareSize.Width, squareSize.Height);
            using (var graphics = Graphics.FromImage(squareImage))
            {
                graphics.FillRectangle(fillBrush ?? Brushes.White, 0, 0, squareSize.Width, squareSize.Height);
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;

                graphics.DrawImage(original, squareSize.Width/2 - original.Width/2,
                    squareSize.Height/2 - original.Height/2, original.Width, original.Height);
            }
            return squareImage;
        }

        public static Bitmap SquareImage(Bitmap original)
        {
            var width = original.Width;
            var height = original.Height;
            var cropArea = new Rectangle();
            if (width > height)
            {
                cropArea.Width = height;
                cropArea.Height = height;
                cropArea.X = 0;
                cropArea.Y = 0;
            }
            else if (width < height)
            {
                cropArea.Width = width;
                cropArea.Height = width;
                cropArea.X = 0;
                cropArea.Y = 0;
            }
            Bitmap croppedImage = null;
            if (width != height) croppedImage = CropImage(original, cropArea);
            return croppedImage;
        }
    }
}