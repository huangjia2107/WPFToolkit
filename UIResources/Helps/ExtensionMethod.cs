using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace UIResources.Helps
{
    public static class ExtensionMethod
    {
        public static bool IsValid(this double doubleValue, bool allowNegative, bool allowNaN, bool allowPositiveInfinity, bool allowNegativeInfinity)
        {
            if (!allowNegative)
            {
                if (doubleValue < 0d)
                {
                    return (false);
                }
            }

            if (!allowNaN)
            {
                if (DoubleUtil.IsNaN(doubleValue))
                {
                    return (false);
                }
            }

            if (!allowPositiveInfinity)
            {
                if (Double.IsPositiveInfinity(doubleValue))
                {
                    return (false);
                }
            }

            if (!allowNegativeInfinity)
            {
                if (Double.IsNegativeInfinity(doubleValue))
                {
                    return (false);
                }
            }

            return (true);
        }

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

        public static CornerRadius Coerce(this CornerRadius cornerRadius, double availableWidth, double availableHeight)
        {
            double? topLeft = null;
            double? bottomLeft = null;
            if (availableHeight < cornerRadius.TopLeft + cornerRadius.BottomLeft)
            {
                topLeft = cornerRadius.TopLeft * availableHeight / (cornerRadius.TopLeft + cornerRadius.BottomLeft);
                bottomLeft = cornerRadius.BottomLeft * availableHeight / (cornerRadius.TopLeft + cornerRadius.BottomLeft);
            }

            double? topRight = null;
            double? bottomRight = null;
            if (availableHeight < cornerRadius.TopRight + cornerRadius.BottomRight)
            {
                topRight = cornerRadius.TopRight * availableHeight / (cornerRadius.TopRight + cornerRadius.BottomRight);
                bottomRight = cornerRadius.BottomRight * availableHeight / (cornerRadius.TopRight + cornerRadius.BottomRight);
            }

            if (availableWidth < cornerRadius.TopLeft + cornerRadius.TopRight)
            {
                var tl = cornerRadius.TopLeft * availableWidth / (cornerRadius.TopLeft + cornerRadius.TopRight);
                topLeft = topLeft == null ? tl : Math.Min(tl, topLeft.Value);

                var tr = cornerRadius.TopRight * availableWidth / (cornerRadius.TopLeft + cornerRadius.TopRight);
                topRight = topRight == null ? tr : Math.Min(tr, topRight.Value);
            }

            if (availableWidth < cornerRadius.BottomLeft + cornerRadius.BottomRight)
            {
                var bl = cornerRadius.BottomLeft * availableWidth / (cornerRadius.BottomLeft + cornerRadius.BottomRight);
                bottomLeft = bottomLeft == null ? bl : Math.Min(bl, bottomLeft.Value);

                var br = cornerRadius.BottomRight * availableWidth / (cornerRadius.BottomLeft + cornerRadius.BottomRight);
                bottomRight = bottomRight == null ? br : Math.Min(br, bottomRight.Value);
            }

            if (topLeft != null || topRight != null || bottomLeft != null || bottomRight != null)
                return new CornerRadius(topLeft.Value, topRight.Value, bottomRight.Value, bottomLeft.Value);

            return cornerRadius;
        }
    }
}
