using System;

namespace MvvmUtils.Messaging.Messages
{
    public class NotificationMessageWithCallback : NotificationMessage
    {
        private readonly Delegate _callback;

        public NotificationMessageWithCallback(string notification, Delegate callback)
            : base(notification)
        {
            _callback = callback;
        }

        public NotificationMessageWithCallback(object sender, string notification, Delegate callback)
            : base(sender, notification)
        {
            _callback = callback;
        }

        public NotificationMessageWithCallback(object sender, object target, string notification, Delegate callback)
            : base(sender, target, notification)
        {
            _callback = callback;
        }

        public virtual object Execute(params object[] parameters)
        {
            if (_callback == null)
                throw new ArgumentNullException("callback", "Callback cannot be null.");

            return _callback.DynamicInvoke(parameters);
        }
    }
}
