using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
