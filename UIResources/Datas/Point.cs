using System.Windows.Media;

namespace UIResources.Datas
{
    public struct PointPair<xT, yT>
    {
        public xT X { get; set; }
        public yT Y { get; set; }
    }

    public class PointData<penT, xT, yT>
    {
        public penT Pen { get; set; }
        public PointPair<xT, yT>[] Points { get; set; }
    }
}
