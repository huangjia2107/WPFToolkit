using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace UIResources.Helps
{
    public static class Algorithm
    {
        public static bool IsThicknessValid(Thickness thickness, bool allowNegative, bool allowNaN, bool allowPositiveInfinity, bool allowNegativeInfinity)
        {
            if (!allowNegative)
            {
                if (thickness.Left < 0d || thickness.Right < 0d || thickness.Top < 0d || thickness.Bottom < 0d)
                {
                    return (false);
                }
            }

            if (!allowNaN)
            {
                if (Double.IsNaN(thickness.Left) || Double.IsNaN(thickness.Right) || Double.IsNaN(thickness.Top) || Double.IsNaN(thickness.Bottom))
                {
                    return (false);
                }
            }

            if (!allowPositiveInfinity)
            {
                if (Double.IsPositiveInfinity(thickness.Left) || Double.IsPositiveInfinity(thickness.Right) || Double.IsPositiveInfinity(thickness.Top) || Double.IsPositiveInfinity(thickness.Bottom))
                {
                    return (false);
                }
            }

            if (!allowNegativeInfinity)
            {
                if (Double.IsNegativeInfinity(thickness.Left) || Double.IsNegativeInfinity(thickness.Right) || Double.IsNegativeInfinity(thickness.Top) || Double.IsNegativeInfinity(thickness.Bottom))
                {
                    return (false);
                }
            }

            return (true);
        } 
    } 
}
