using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace UIResources.Controls
{
    [TemplatePart(Name = ScrollViewerTemplateName, Type = typeof(ScrollViewer))]
    [TemplatePart(Name = ScaleTransformTemplateName, Type = typeof(ScaleTransform))]
    [TemplatePart(Name = HorizontalRulerTemplateName, Type = typeof(Ruler))]
    [TemplatePart(Name = VerticalRulerTemplateName, Type = typeof(Ruler))]
    [TemplatePart(Name = ContentPresenterTemplateName, Type = typeof(ContentPresenter))]
    public class ZoomBox : ContentControl
    {
        private static readonly Type _typeofSelf = typeof(ZoomBox);

        private const string ScrollViewerTemplateName = "PART_ScrollViewer";
        private const string ScaleTransformTemplateName = "PART_ScaleTransform";

        private const string HorizontalRulerTemplateName = "PART_HorizontalRuler";
        private const string VerticalRulerTemplateName = "PART_VerticalRuler";

        private const string ContentPresenterTemplateName = "PART_ContentPresenter";

        private ScrollViewer _partScrollViewer;
        private ScaleTransform _partScaleTransform;
        private Ruler _partHorizontalRuler;
        private Ruler _partVerticalRuler;
        private ContentPresenter _partContentPresenter;

        static ZoomBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(_typeofSelf, new FrameworkPropertyMetadata(_typeofSelf));
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _partScrollViewer = GetTemplateChild(ScrollViewerTemplateName) as ScrollViewer;
            _partScaleTransform = GetTemplateChild(ScaleTransformTemplateName) as ScaleTransform;

            _partHorizontalRuler = GetTemplateChild(HorizontalRulerTemplateName) as Ruler;
            _partVerticalRuler = GetTemplateChild(VerticalRulerTemplateName) as Ruler;

            _partContentPresenter = GetTemplateChild(ContentPresenterTemplateName) as ContentPresenter;

            if (_partScrollViewer == null || _partScaleTransform == null || _partHorizontalRuler == null || _partVerticalRuler == null || _partContentPresenter == null)
            {
                throw new NullReferenceException(string.Format("You have missed to specify {0}, {1}, {2}, {3} or {4} in your template",
                    ScrollViewerTemplateName, ScaleTransformTemplateName, HorizontalRulerTemplateName, VerticalRulerTemplateName, ContentPresenterTemplateName));
            }

            _partContentPresenter.PreviewMouseWheel += _partContentPresenter_PreviewMouseWheel;
        }

        private void _partContentPresenter_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
            {
                ZoomIn();
            }
            if (e.Delta < 0)
            {
                ZoomOut();
            }

            e.Handled = true;
        }

        private void ZoomIn()
        {
            if (_partScaleTransform == null)
                return;

            _partScaleTransform.ScaleX = Math.Max(0, _partScaleTransform.ScaleX + 0.5);
            _partScaleTransform.ScaleY = Math.Max(0, _partScaleTransform.ScaleY + 0.5);
        }

        private void ZoomOut()
        {
            if (_partScaleTransform == null)
                return;

            _partScaleTransform.ScaleX *= 0.5;//Math.Max(0, _partScaleTransform.ScaleX - 0.5);
            _partScaleTransform.ScaleY *= 0.5;// Math.Max(0, _partScaleTransform.ScaleY - 0.5);
        }


    }
}
