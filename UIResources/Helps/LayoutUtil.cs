using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Interop;
using System.Windows.Media;

namespace UIResources.Helps
{
    public class LayoutUtil
    {
        //使Popup不置顶
        public static readonly DependencyProperty TopIndexProperty = DependencyProperty.RegisterAttached("TopIndex", typeof(int), typeof(LayoutUtil), new FrameworkPropertyMetadata(0, TopIndexPropertyChangedCallback));
        public static int GetTopIndex(DependencyObject obj)
        {
            return (int)obj.GetValue(TopIndexProperty);
        }
        public static void SetTopIndex(DependencyObject obj, int value)
        {
            obj.SetValue(TopIndexProperty, value);
        }

        private static Action<IntPtr, int, Win32.RECT> winAction = (h, i, rect) => Win32.SetWindowPos(h, i, rect.Left, rect.Top, Math.Abs(rect.Right - rect.Left), Math.Abs(rect.Bottom - rect.Top), 0);

        private static Action<Visual, int> popupAction = (v, i) =>
        {
            var hwndSource = (HwndSource)PresentationSource.FromVisual(v);
            if (hwndSource != null)
            {
                Win32.RECT rect;
                if (Win32.GetWindowRect(hwndSource.Handle, out rect))
                    Win32.SetWindowPos(hwndSource.Handle, i, rect.Left, rect.Top, Math.Abs(rect.Right - rect.Left), Math.Abs(rect.Bottom - rect.Top), 0);
            }
        };

        static void TopIndexPropertyChangedCallback(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is Window)
            {
                var obj = sender as Window;
                var hwndSource = (HwndSource)PresentationSource.FromVisual(obj);
                if (hwndSource != null)
                {
                    Win32.RECT rect;
                    if (Win32.GetWindowRect(hwndSource.Handle, out rect))
                    {
                        RoutedEventHandler handler = null;
                        handler = (s, _) => { winAction(hwndSource.Handle, GetTopIndex(obj), rect); obj.Loaded -= handler; };
                        obj.Loaded += handler;
                    }
                }
            }

            if (sender is Popup)
            {
                var obj = sender as Popup;

                EventHandler handler = null;
                handler = (s, _) => { popupAction(obj.Child, GetTopIndex(obj)); obj.Opened -= handler; };
                obj.Opened += handler;
            }
        }
    }
}
