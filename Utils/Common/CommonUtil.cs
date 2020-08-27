using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Utils.Common
{
    public static class CommonUtil
    {
        /*
         *    1   2   3   4             16  11   6   1
         *    5   6   7   8             17  12   7   2
         *    9  10  11  12      ->     18  13   8   3
         *   13  14  15  16             19  14   9   4
         *   17  18  19  20             20  15  10   5
         * 
         */
        public static int RotateMatrixIndex(int index, int r, int c)
        {
            return (c - 1 - (index - 1) % c) * r + (index - 1) / c + 1;
        }
        
        public static string ResizePathData(string originalData, double curWidth, double curHeight, double desiredWidth, double desiredHeight)
        {
            var pattern = @"^[a-zA-Z][0-9]+(\.[0-9]+)?$";
            var rw = curWidth / desiredWidth;
            var rh = curHeight / desiredHeight;
            
            var pathSpans = originalData.Split(' ');
            
            return string.Join(" ",pathSpans.Select(span=>
            {
                var pos = span.Split(',');
                var match = Regex.IsMatch(pos[0], pattern);
                var match1 = Regex.IsMatch(pos[1], pattern);
                
                return string.Format("{0}{1:#0.##},{2}{3:#0.##}",
                              match ? pos[0].Substring(0,1) : "",
                              double.Parse(match ? pos[0].Remove(0,1) : pos[0]) / rw,
                              match1 ? pos[1].Substring(0,1):"",
                              double.Parse(match1 ? pos[1].Remove(0,1) : pos[1]) / rh);
            }).ToArray());
        }
    }
}
