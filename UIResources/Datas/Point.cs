using System.Windows.Media;

namespace UIResources.Datas
{
    public struct PointPair<xT, yT>
    {
        public xT X { get; set; }
        public yT Y { get; set; }
    }

    public class PointData<vT, xT, yT>
    {
        public vT Value { get; set; }
        public PointPair<xT, yT>[] Points { get; set; }
    }
}
