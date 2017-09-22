using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace UIResources.Helps
{  
    public class DeferRefresh : IDisposable
    {
        private Action _action;

        public DeferRefresh(Action action)
        {
            _action = action;
        }

        public void Dispose()
        {
            if (_action != null)
            {
                _action();
                _action = null;
            }

            GC.SuppressFinalize(this);
        }
    }
}
