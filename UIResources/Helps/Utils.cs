using System; 
using System.Windows;
using System.Windows.Media;

namespace UIResources.Helps
{
    public static class Utils
    {
        public static Brush GetBrush()
        {
            var tick = DateTime.Now.Ticks;
            var ran = new Random((int)(tick & 0xffffffffL) | (int)(tick >> 32));

            var R = ran.Next(255);
            var G = ran.Next(255);
            var B = ran.Next(255);
            B = (R + G > 400) ? R + G - 400 : B;//0 : 380 - R - G;
            B = (B > 255) ? 255 : B;

            return new SolidColorBrush(Color.FromRgb((byte)R, (byte)G, (byte)B));
        }

        public static System.Drawing.Color GetDrawingColor()
        {
            var tick = DateTime.Now.Ticks;
            var ran = new Random((int)(tick & 0xffffffffL) | (int)(tick >> 32));

            var R = ran.Next(255);
            var G = ran.Next(255);
            var B = ran.Next(255);
            B = (R + G > 400) ? R + G - 400 : B;//0 : 380 - R - G;
            B = (B > 255) ? 255 : B;

            return System.Drawing.Color.FromArgb(R, G, B);
        }

        public static (double X, double Y) GetDpi(Visual visual)
        {
            var source = PresentationSource.FromVisual(visual);

            var dpiX = 96.0;
            var dpiY = 96.0;

            if (source?.CompositionTarget != null)
            {
                dpiX = 96.0 * source.CompositionTarget.TransformToDevice.M11;
                dpiY = 96.0 * source.CompositionTarget.TransformToDevice.M22;
            }

            return (dpiX, dpiY);
        }
    }
}
