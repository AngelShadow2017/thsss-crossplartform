// Cross-platform DPI detection (replaces System.Management WMI)
using System;

namespace Shooting
{
    public class Dpi
    {
        public static int PixelsPerXLogicalInch = 96;
        public static int PixelsPerYLogicalInch = 96;

        public static void GetDpi()
        {
            // Cross-platform stub: default to 96 DPI
            // Real implementation would use SDL_GetDisplayDPI or similar
            PixelsPerXLogicalInch = 96;
            PixelsPerYLogicalInch = 96;
        }
    }
}
