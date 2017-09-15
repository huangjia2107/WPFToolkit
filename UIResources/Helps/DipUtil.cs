using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace UIResources.Helps
{
    /// <summary>
    /// DIP (Device Independent Pixels)
    /// </summary>
    public static class DipUtil
    {
        private const double DpiBase = 96.0; 

        public static Point GetDpiFactor(Visual visual)
        {
            if (visual == null)
                throw new ArgumentNullException("visual");

            var source = PresentationSource.FromVisual(visual);
            var matrix = source.CompositionTarget.TransformToDevice;
            return new Point(matrix.M11, matrix.M22);
        }

        public static double PtToDip(double pt)
        {
            return (pt * 1 / 72.0 * 96.0);
        }

        public static Point GetDpi(Visual visual)
        {
            if (visual == null)
                throw new ArgumentNullException("visual");

            Point sysDpiFactor = GetDpiFactor(visual);
            return new Point(
                 sysDpiFactor.X * DpiBase,
                 sysDpiFactor.Y * DpiBase);
        }
    }
}
