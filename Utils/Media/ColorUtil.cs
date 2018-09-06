using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Utils.Common;

namespace Utils.Media
{
    public static class ColorUtil
    {
        /*
		//https://jacobmsaylor.com/invert-a-color-c/
        //Modified from http://snipplr.com/view/13358/
        public static Color HexToColor(string hexColor)
        {
            //Remove # if present
            if (hexColor.IndexOf('#') != -1)
                hexColor = hexColor.Replace("#", "");
         
            byte red = 0;
            byte green = 0;
            byte blue = 0;
         
            if (hexColor.Length == 8)
            {
                //We need to remove the preceding FF
                hexColor = hexColor.Substring(2);
            }
         
            if (hexColor.Length == 6)
            {
                //#RRGGBB
                red = byte.Parse(hexColor.Substring(0, 2), NumberStyles.AllowHexSpecifier);
                green = byte.Parse(hexColor.Substring(2, 2), NumberStyles.AllowHexSpecifier);
                blue = byte.Parse(hexColor.Substring(4, 2), NumberStyles.AllowHexSpecifier);
            }
            else if (hexColor.Length == 3)
            {
                //#RGB
                red = byte.Parse(hexColor[0].ToString() + hexColor[0].ToString(), NumberStyles.AllowHexSpecifier);
                green = byte.Parse(hexColor[1].ToString() + hexColor[1].ToString(), NumberStyles.AllowHexSpecifier);
                blue = byte.Parse(hexColor[2].ToString() + hexColor[2].ToString(), NumberStyles.AllowHexSpecifier);
            }
         
            return Color.FromRgb(red, green, blue);
        }
        
        public static Brush invertColor(string value)
        {
            if (value != null)
            {                
                Color ColorToConvert = HexToColor(value);
                Color invertedColor = Color.FromRgb((byte)~ColorToConvert.R, (byte)~ColorToConvert.G, (byte)~ColorToConvert.B);
                return new SolidColorBrush(invertedColor);
            }
            else
            {
                return new SolidColorBrush(Color.FromRgb(0,0,0));
            }
        }*/

        public static Color InvertColor(Color color)
        {
            return Color.FromRgb((byte)~color.R, (byte)~color.G, (byte)~color.B);
        } 

        public static void HslFromColor(Color C, ref double H, ref double S, ref double L)
        {
            double r = C.R / 255d;
            double g = C.G / 255d;
            double b = C.B / 255d;

            var max = Math.Max(Math.Max(r, g), b);
            var min = Math.Min(Math.Min(r, g), b);
            var delta = max - min;

            var hue = 0d;
            var saturation = DoubleUtil.GreaterThan(max, 0) ? (delta / max) : 0.0;
            var luminosity = max;

            if (!DoubleUtil.IsZero(delta))
            {
                if (DoubleUtil.AreClose(r, max))
                    hue = (g - b) / delta;
                else if (DoubleUtil.AreClose(g, max))
                    hue = 2 + (b - r) / delta;
                else if (DoubleUtil.AreClose(b, max))
                    hue = 4 + (r - g) / delta;

                hue = hue * 60;
                if (DoubleUtil.LessThan(hue, 0d))
                    hue += 360;
            }

            H = hue / 360d;
            S = saturation;
            L = luminosity;
        }

        public static Color ColorFromAhsl(double A, double H, double S, double L)
        {
            var r = ColorFromHsl(H, S, L);
            r.A = (byte)Math.Round(A * 255d);

            return r;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="H">0---1</param>
        /// <param name="S">0---1</param>
        /// <param name="L">0---1</param>
        /// <returns></returns>  
        public static Color ColorFromHsl(double H, double S, double L)
        {
            double red = 0.0, green = 0.0, blue = 0.0;

            if (DoubleUtil.IsZero(S))
                red = green = blue = L;
            else
            {
                var h = DoubleUtil.IsOne(H) ? 0d : (H * 6.0);
                int i = (int)Math.Floor(h);

                var f = h - i;
                var r = L * (1.0 - S);
                var s = L * (1.0 - S * f);
                var t = L * (1.0 - S * (1.0 - f));

                switch (i)
                {
                    case 0: red = L; green = t; blue = r; break;
                    case 1: red = s; green = L; blue = r; break;
                    case 2: red = r; green = L; blue = t; break;
                    case 3: red = r; green = s; blue = L; break;
                    case 4: red = t; green = r; blue = L; break;
                    case 5: red = L; green = r; blue = s; break;
                }
            }

            return Color.FromRgb((byte)Math.Round(red * 255.0), (byte)Math.Round(green * 255.0), (byte)Math.Round(blue * 255.0));
        }
    }
}
