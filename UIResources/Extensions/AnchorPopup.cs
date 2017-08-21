using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Interop;
using System.Windows.Media;
using UIResources.Helps;
using UIResources.Panels;

namespace UIResources.Extensions
{
    public sealed class AnchorPopup
    {
        private static readonly AnchorPopup _anchorPopup = new AnchorPopup();

        private Popup _popup;
        private Grid _grid;
        private AnchorBorder _anchorBorder;
        private ContentPresenter _contentPresenter;

        private AnchorPopup()
        {
            Construct();
        }

        public static void Show(UIElement placementTarget, object content, double dockLength = 20, double anchorHeight = 20)
        {
            _anchorPopup.InnerShow(placementTarget, content, dockLength, anchorHeight);
        }

        private void InnerShow(UIElement placementTarget, object content, double dockLength = 20, double anchorHeight = 20)
        {
            if (placementTarget == null || content == null)
                throw new ArgumentNullException();

            _contentPresenter.Content = content is string ? ConstructTextBlock((string)content) : content;
            _popup.PlacementTarget = placementTarget;

            _anchorBorder.DockLength = dockLength;
            if (!DoubleUtil.AreClose(Math.Abs(_anchorBorder.AnchorOffset.Y), anchorHeight))
                _anchorBorder.AnchorOffset = new Point(_anchorBorder.DockOffset + _anchorBorder.DockLength / 2, DoubleUtil.LessThan(_anchorBorder.AnchorOffset.Y, 0) ? -anchorHeight : anchorHeight);

            _popup.IsOpen = true;
        }

        private void Construct()
        {
            _contentPresenter = new ContentPresenter();
            _anchorBorder = ConstructAnchorBorder();
            _grid = new Grid();
            _popup = ConstructPopup();

            _anchorBorder.Child = _contentPresenter;
            _grid.Children.Add(_anchorBorder);

            _popup.Child = _grid;
        }

        private TextBlock ConstructTextBlock(string text)
        {
            return new TextBlock
            {
                Text = text,
                TextWrapping = TextWrapping.Wrap,
                TextTrimming = TextTrimming.CharacterEllipsis
            };
        }

        private AnchorBorder ConstructAnchorBorder()
        {
            return new AnchorBorder
            {
                Background = Brushes.CadetBlue,
                Padding = new Thickness(15),
                VerticalAlignment = VerticalAlignment.Top,
                CornerRadius = new CornerRadius(6),
                BorderThickness = 0,
                AnchorDock = AnchorDock.BottomLeft,
                AnchorOffset = new Point(25, 20),
                DockOffset = 15,
                DockLength = 20
            };
        }

        private Popup ConstructPopup()
        {
            var popup = new Popup
            {
                Placement = PlacementMode.Top,
                StaysOpen = false,
                PopupAnimation = PopupAnimation.Fade,
                AllowsTransparency = true,
                MaxWidth = 300,
            };
            popup.Opened += Popup_OnOpened;

            return popup;
        }

        private void Popup_OnOpened(object sender, EventArgs e)
        {
            if (_popup == null || _grid == null || _anchorBorder == null)
                throw new ArgumentNullException("Popup_OnOpened");

            var placementTarget = _popup.PlacementTarget as FrameworkElement;
            var fromVisual = (HwndSource)PresentationSource.FromVisual(_popup.Child);

            if (fromVisual != null && placementTarget != null)
            {
                Win32.RECT rect;

                if (Win32.GetWindowRect(fromVisual.Handle, out rect))
                {
                    var topLeftPoint = placementTarget.PointFromScreen(new Point(rect.Left, rect.Top));
                    var topLeftToTargetPoint = new Point(topLeftPoint.X + _anchorBorder.CornerRadius.TopLeft, topLeftPoint.Y);
                    var topRightToTargetPoint = new Point(topLeftPoint.X + _grid.ActualWidth - _anchorBorder.CornerRadius.TopRight, topLeftToTargetPoint.Y + _grid.ActualHeight);  //  testBtn.PointFromScreen(new Point(rect.Right, rect.Top));

                    bool isAbove = (topLeftToTargetPoint.Y < 0);

                    _anchorBorder.AnchorDock = isAbove ? AnchorDock.BottomLeft : AnchorDock.TopLeft;
                    _anchorBorder.VerticalAlignment = isAbove ? VerticalAlignment.Top : VerticalAlignment.Bottom;

                    _anchorBorder.AnchorOffset = GetAnchorOffset(_anchorBorder, placementTarget, topLeftToTargetPoint, topRightToTargetPoint, isAbove);
                    _anchorBorder.DockOffset = GetDockOffset(_anchorBorder, placementTarget, topLeftToTargetPoint, topRightToTargetPoint, isAbove);

                    _grid.Height = _anchorBorder.ActualRenderHeight;
                }
            }
        }

        private double GetDockOffset(AnchorBorder anchorBorder, FrameworkElement placementTarget, Point topLeftToTargetPoint, Point topRightToTargetPoint, bool isAbove)
        {
            if (anchorBorder == null || placementTarget == null)
                throw new ArgumentNullException(anchorBorder == null ? "anchorBorder" : "placementTarget");

            double leftDockOffset = 0;
            if (topRightToTargetPoint.X <= anchorBorder.DockLength)
            {
                leftDockOffset = topRightToTargetPoint.X - topLeftToTargetPoint.X - anchorBorder.DockLength + (isAbove ? anchorBorder.CornerRadius.TopLeft : anchorBorder.CornerRadius.BottomLeft);
            }

            if (topRightToTargetPoint.X > anchorBorder.DockLength && topLeftToTargetPoint.X <= placementTarget.ActualWidth - anchorBorder.DockLength)
            {
                leftDockOffset = anchorBorder.AnchorOffset.X - (anchorBorder.DockLength / 2);
            }

            if (topLeftToTargetPoint.X > (placementTarget.ActualWidth - anchorBorder.DockLength))
            {
                leftDockOffset = (isAbove ? anchorBorder.CornerRadius.TopLeft : anchorBorder.CornerRadius.BottomLeft);
            }

            return leftDockOffset;
        }

        private Point GetAnchorOffset(AnchorBorder anchorBorder, FrameworkElement placementTarget, Point topLeftToTargetPoint, Point topRightToTargetPoint, bool isAbove)
        {
            if (anchorBorder == null || placementTarget == null)
                throw new ArgumentNullException(anchorBorder == null ? "anchorBorder" : "placementTarget");

            var anchorPoint = new Point();
            if (topRightToTargetPoint.X <= 0)
            {
                anchorPoint.X = 0;
                anchorPoint.Y = isAbove ? 0 : placementTarget.ActualHeight;
            }

            if (topLeftToTargetPoint.X <= 0 && topRightToTargetPoint.X > 0)
            {
                anchorPoint.X = Math.Min(topRightToTargetPoint.X, placementTarget.ActualWidth) / 2;
                anchorPoint.Y = isAbove ? 0 : placementTarget.ActualHeight;
            }

            if (topLeftToTargetPoint.X > 0)
            {
                anchorPoint.X = Math.Min((placementTarget.ActualWidth - topLeftToTargetPoint.X) / 2 + topLeftToTargetPoint.X, placementTarget.ActualWidth);
                anchorPoint.Y = isAbove ? 0 : placementTarget.ActualHeight;
            }

            return new Point(anchorPoint.X - topLeftToTargetPoint.X + (isAbove ? anchorBorder.CornerRadius.TopLeft : anchorBorder.CornerRadius.BottomLeft), isAbove ? Math.Abs(anchorBorder.AnchorOffset.Y) : (anchorPoint.Y - topLeftToTargetPoint.Y - Math.Abs(anchorBorder.AnchorOffset.Y)));
        }
    }
}
