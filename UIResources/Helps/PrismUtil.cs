/*
using System;
using System.Windows;

using Prism.Regions;

namespace CASApp.Theme.Utils
{
    public static class PrismUtil
    {
        public static void DisposeViewModelInRegions(DependencyObject d, params string[] regionNames)
        {
            if (d == null || regionNames == null || regionNames.Length == 0)
                return;

            var rg = RegionManager.GetRegionManager(d);
            if (rg == null)
                return;

            Array.ForEach(regionNames, rn =>
            {
                if (rg.Regions.ContainsRegionWithName(rn))
                {
                    var views = rg.Regions[rn].Views;
                    if (views != null)
                    {
                        foreach (FrameworkElement view in views)
                        {
                            if (view == null)
                                continue;

                            (view as IDisposable)?.Dispose();
                            (view.DataContext as IDisposable)?.Dispose();
                        }
                    }
                }
            });
        }
    }
}
*/