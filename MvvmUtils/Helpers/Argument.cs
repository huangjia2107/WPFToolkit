using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MvvmUtils.Helpers
{
    public static class Argument
    {
        public static void IsNotNull(string paramName, object paramValue)
        {
            if (paramValue == null)
                throw new ArgumentNullException(paramName);
        }
    }
}
