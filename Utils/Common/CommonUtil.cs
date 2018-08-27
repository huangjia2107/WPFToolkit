using System;

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
    }
}
