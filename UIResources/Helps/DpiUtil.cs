﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using UIResources.Controls;

namespace UIResources.Helps
{
    /// <summary>
    /// DIP (Device Independent Pixel)
    /// DIU (Device Independent Unit)
    /// </summary>
    public static class DpiUtil
    {
        private const double DIP = 96.0; 
        private const double DIU = 1 / 96.0;

        public static Point GetDpiFactor(Visual visual)
        {
            if (visual == null)
                throw new ArgumentNullException("visual");

            var source = PresentationSource.FromVisual(visual);
            var matrix = source.CompositionTarget.TransformToDevice;
            return new Point(matrix.M11, matrix.M22);
        }

        public static double PtToPixel(double pt)
        {
            return (pt * 1 / 72.0 * DIP);
        }

        public static Point GetDpi(Visual visual)
        {
            if (visual == null)
                throw new ArgumentNullException("visual");

            Point sysDpiFactor = GetDpiFactor(visual);
            return new Point(
                 sysDpiFactor.X * DIP,
                 sysDpiFactor.Y * DIP);
        }

        public static double GetDevicePixelUnit(Visual visual)
        {
            if (visual == null)
                throw new ArgumentNullException("visual");
                
            return DIU * GetDpi(visual).X;
        }

        public static decimal GetPixelPerUnit(RulerUnit unit)
        {
            decimal result = 1;
            switch (unit)
            {
                case RulerUnit.Pixel:
                    result = 1;
                    break;
                case RulerUnit.Inch:
                    result = 96;
                    break;
                case RulerUnit.Foot:
                    result = 1152;
                    break;
                case RulerUnit.Millimeter:
                    result = 3.7795m;
                    break;
                case RulerUnit.Centimeter:
                    result = 37.795m;
                    break;
            }

            return result;
        }
    }
}