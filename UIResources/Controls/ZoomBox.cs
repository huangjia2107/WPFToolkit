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
    [TemplatePart(Name = ScaleTransformTemplateName, Type = typeof(ScaleTransform))]
    [TemplatePart(Name = HorizontalRulerTemplateName, Type = typeof(Ruler))]
    [TemplatePart(Name = VerticalRulerTemplateName, Type = typeof(Ruler))] 
    [TemplatePart(Name = ScrollContentPresenterTemplateName, Type = typeof(ScrollContentPresenter))]
    public class ZoomBox : ScrollViewer
    {
        private static readonly Type _typeofSelf = typeof(ZoomBox);

        private const string HorizontalRulerTemplateName = "PART_HorizontalRuler";
        private const string VerticalRulerTemplateName = "PART_VerticalRuler";
 
        private const string ScrollContentPresenterTemplateName = "PART_ScrollContentPresenter";
        private const string ScaleTransformTemplateName = "PART_ScaleTransform";
 
        private ScaleTransform _partScaleTransform;
        private Ruler _partHorizontalRuler;
        private Ruler _partVerticalRuler; 
        private ScrollContentPresenter _partScrollContentPresenter;

        static ZoomBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(_typeofSelf, new FrameworkPropertyMetadata(_typeofSelf));
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _partHorizontalRuler = GetTemplateChild(HorizontalRulerTemplateName) as Ruler;
            _partVerticalRuler = GetTemplateChild(VerticalRulerTemplateName) as Ruler;
 
            _partScrollContentPresenter = GetTemplateChild(ScrollContentPresenterTemplateName) as ScrollContentPresenter;
            _partScaleTransform = GetTemplateChild(ScaleTransformTemplateName) as ScaleTransform;
            

            if (_partScaleTransform == null || _partHorizontalRuler == null 
            || _partVerticalRuler == null || _partScrollContentPresenter == null)
            {
                throw new NullReferenceException(string.Format("You have missed to specify {0}, {1}, {2} or {3} in your template",
                    HorizontalRulerTemplateName, VerticalRulerTemplateName, ScrollContentPresenterTemplateName, ScaleTransformTemplateName));
            }

            _partScrollContentPresenter.PreviewMouseWheel += _partContentGrid_PreviewMouseWheel;
        }

        private void _partContentGrid_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
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
