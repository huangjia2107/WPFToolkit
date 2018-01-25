using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace UIResources.Datas
{
    public interface IRelativeBinding
    {
        Type AncestorType { get; set; }
        uint AncestorLevel { get; set; }}

    public class RelativeBinding : Binding, IRelativeBinding
    {
        public RelativeBinding()
            : this(null)
        {
        }

        public RelativeBinding(string path)
            : base(path)
        {
            AncestorLevel = 1;
        }

        public Type AncestorType { get; set; }
        public uint AncestorLevel { get; set; }
    }

    public class RelativeMultiBinding : MultiBinding, IRelativeBinding
    {
        public RelativeMultiBinding()
            : base()
        {
            AncestorLevel = 1;
        }

        public Type AncestorType { get; set; }
        public uint AncestorLevel { get; set; }
    }
}
