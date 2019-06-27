using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils.Common;

namespace UIResources.Helps
{
    public class DoubleEqualityComparer : IEqualityComparer<double>
    {
        public bool Equals(double x, double y)
        {
            return DoubleUtil.AreClose(x, y);
        }

        public int GetHashCode(double obj)
        {
            return obj.GetHashCode();
        }
    }

    public class DoubleComparer : IComparer<double>
    {
        public int Compare(double x, double y)
        {
            if (DoubleUtil.AreClose(x, y))
                return 0;

            if (DoubleUtil.GreaterThan(x,y))
                return 1;

            return -1;
        }
    }
}
