using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UIResources.Controls;

namespace Test
{
    public sealed class Map
    {
        public static SortedDictionary<RulerUnit, string> RulerUnitDictionary = new SortedDictionary<RulerUnit, string>
        {
            {RulerUnit.Pixel,          "Pixel"},
            {RulerUnit.Millimeter,     "Millimeter"},
            {RulerUnit.Centimeter,     "Centimeter"},
            {RulerUnit.Inch,           "Inch"},
            {RulerUnit.Foot,           "Foot"},
        };
    }
}
