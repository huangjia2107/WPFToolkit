using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace UIResources.Helps
{
    public static class Utils
    {
        public static bool IsValid(this CornerRadius cornerRadius, bool allowNegative, bool allowNaN, bool allowPositiveInfinity, bool allowNegativeInfinity)
        {
            if (!allowNegative)
            {
                if (cornerRadius.BottomLeft < 0d || cornerRadius.BottomRight < 0d || cornerRadius.TopLeft < 0d || cornerRadius.TopRight < 0d)
                {
                    return (false);
                }
            }

            if (!allowNaN)
            {
                if (Double.IsNaN(cornerRadius.BottomLeft) || Double.IsNaN(cornerRadius.BottomRight) || Double.IsNaN(cornerRadius.TopLeft) || Double.IsNaN(cornerRadius.TopRight))
                {
                    return (false);
                }
            }

            if (!allowPositiveInfinity)
            {
                if (Double.IsPositiveInfinity(cornerRadius.BottomLeft) || Double.IsPositiveInfinity(cornerRadius.BottomRight) || Double.IsPositiveInfinity(cornerRadius.TopLeft) || Double.IsPositiveInfinity(cornerRadius.TopRight))
                {
                    return (false);
                }
            }

            if (!allowNegativeInfinity)
            {
                if (Double.IsNegativeInfinity(cornerRadius.BottomLeft) || Double.IsNegativeInfinity(cornerRadius.BottomRight) || Double.IsNegativeInfinity(cornerRadius.TopLeft) || Double.IsNegativeInfinity(cornerRadius.TopRight))
                {
                    return (false);
                }
            }

            return (true);
        }

        public static bool IsValid(this Thickness thickness, bool allowNegative, bool allowNaN, bool allowPositiveInfinity, bool allowNegativeInfinity)
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

        /// <summary>
        /// Rounds the given value based on the DPI scale
        /// </summary>
        /// <param name="value">Value to round</param>
        /// <param name="dpiScale">DPI Scale</param>
        /// <returns></returns>
        public static double RoundLayoutValue(double value, double dpiScale)
        {
            double newValue;

            // If DPI == 1, don't use DPI-aware rounding. 
            if (!DoubleUtil.AreClose(dpiScale, 1.0))
            {
                newValue = Math.Round(value * dpiScale) / dpiScale;
                // If rounding produces a value unacceptable to layout (NaN, Infinity or MaxValue), use the original value.
                if (DoubleUtil.IsNaN(newValue) ||
                    Double.IsInfinity(newValue) ||
                    DoubleUtil.AreClose(newValue, Double.MaxValue))
                {
                    newValue = value;
                }
            }
            else
                newValue = Math.Round(value);

            return newValue;
        }
    }
}
