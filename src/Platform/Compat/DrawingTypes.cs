// Drawing types compatibility
// NOTE: Color, Point, PointF, Size, SizeF, Rectangle, RectangleF, Font, FontStyle, Brush,
//       SolidBrush, TextureBrush, Pen, Image, Bitmap, Graphics, StringFormat, StringAlignment,
//       SystemColors, GraphicsUnit, PixelFormat are defined in AdditionalTypes.cs.
//       Do NOT redefine them here.
using System;

namespace System.Drawing
{
    public class GraphicsPath : IDisposable
    {
        public void AddLine(float x1, float y1, float x2, float y2) { }
        public void AddRectangle(RectangleF rect) { }
        public void AddEllipse(RectangleF rect) { }
        public void AddBezier(float x1, float y1, float x2, float y2, float x3, float y3, float x4, float y4) { }
        public void CloseFigure() { }
        public void Dispose() { }
    }

    public class Region : IDisposable
    {
        public Region() { }
        public Region(Rectangle rect) { }
        public Region(RectangleF rect) { }
        public Region(GraphicsPath path) { }
        public void Dispose() { }
    }

    public class Icon : IDisposable
    {
        public Icon(string filename) { }
        public void Dispose() { }
    }

    public class BitmapData
    {
        public IntPtr Scan0;
        public int Stride;
        public int Width;
        public int Height;
        public Imaging.PixelFormat PixelFormat;
    }

    public class ImageAttributes : IDisposable
    {
        public void SetColorMatrix(object matrix) { }
        public void Dispose() { }
    }
}

namespace System.Drawing.Imaging
{
    public enum ImageFormat
    {
        Bmp = 0, Jpeg = 1, Gif = 2, Tiff = 3, Png = 4,
    }

    public static class ImageFormatHelper
    {
        public static ImageFormat Bmp => ImageFormat.Bmp;
        public static ImageFormat Jpeg => ImageFormat.Jpeg;
        public static ImageFormat Gif => ImageFormat.Gif;
        public static ImageFormat Tiff => ImageFormat.Tiff;
        public static ImageFormat Png => ImageFormat.Png;
    }
}
