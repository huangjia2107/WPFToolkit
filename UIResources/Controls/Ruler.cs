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
    public enum Unit
    {
        Pixel,
        Millimeter,
        Centimeter,
        Meter,
        Inch,
        Foot
    }

    public class Ruler : FrameworkElement
    {
        private VisualCollection _visualCollection;

        public Ruler()
        {
            _visualCollection = new VisualCollection(this);
        }




        protected override int VisualChildrenCount
        {
            get { return _visualCollection.Count; }
        }

        protected override Visual GetVisualChild(int index)
        {
            if (index < 0 || index >= _visualCollection.Count)
            {
                throw new ArgumentOutOfRangeException();
            }

            return _visualCollection[index];
        }

        private void Draw()
        {
            var visual = new DrawingVisual();
            using (var dc = visual.RenderOpen())
            {

            }

            _visualCollection.Add(visual);
        }
    }
}
