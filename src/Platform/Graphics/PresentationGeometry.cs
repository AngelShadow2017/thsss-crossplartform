using System;

namespace THSSS.Graphics;

public readonly record struct PixelViewport(int X, int Y, int Width, int Height);

public static class PresentationGeometry
{
    public static PixelViewport Fit(int width, int height, int logicalWidth = 640, int logicalHeight = 480)
    {
        if (width <= 0 || height <= 0) return default;
        if ((long)width * logicalHeight > (long)height * logicalWidth)
        {
            int fittedWidth = Math.Max(1, (int)((long)height * logicalWidth / logicalHeight));
            return new((width - fittedWidth) / 2, 0, fittedWidth, height);
        }
        int fittedHeight = Math.Max(1, (int)((long)width * logicalHeight / logicalWidth));
        return new(0, (height - fittedHeight) / 2, width, fittedHeight);
    }
}
