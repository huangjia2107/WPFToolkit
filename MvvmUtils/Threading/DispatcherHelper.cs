using System;
using System.Text;
using System.Windows.Threading;

namespace MvvmUtils.Threading
{
    public static class DispatcherHelper
    {
        public static Dispatcher UIDispatcher { get; private set; }

        //call this in UI thread
        public static void Initialize()
        {
            if (UIDispatcher != null && UIDispatcher.Thread.IsAlive)
                return;

            UIDispatcher = Dispatcher.CurrentDispatcher;
        }

        public static void Reset()
        {
            UIDispatcher = null;
        }

        public static void RunInvoke(Action action)
        {
            CheckDispatcher();
            UIDispatcher.Invoke(action);
        }

        public static void RunBeginInvoke(Action action)
        {
            CheckDispatcher();
            UIDispatcher.BeginInvoke(action);
        }

        public static void CheckAndInvoke(Action action)
        {
            CheckDispatcher();

            if (action == null || UIDispatcher == null || !UIDispatcher.Thread.IsAlive)
                return;

            if (UIDispatcher.CheckAccess())
                action();
            else
                UIDispatcher.Invoke(action);
        }

        public static void CheckAndBeginInvoke(Action action)
        {
            CheckDispatcher();

            if (action == null || UIDispatcher == null || UIDispatcher.Thread.IsAlive == false)
                return;

            if (UIDispatcher.CheckAccess())
                action();
            else
                UIDispatcher.BeginInvoke(action);
        }

        private static void CheckDispatcher()
        {
            if (UIDispatcher == null)
            {
                StringBuilder stringBuilder = new StringBuilder("The DispatcherHelper is not initialized.");
                stringBuilder.AppendLine();
                stringBuilder.Append("Call DispatcherHelper.Initialize() in the static App constructor.");
                throw new InvalidOperationException(stringBuilder.ToString());
            }
        }
    }
}
