using System; 

namespace MvvmUtils.Messaging.Messages
{
    public class NotificationMessageAction<T> : NotificationMessageWithCallback
    {
        public NotificationMessageAction(string notification, Action<T> callback)
            : base(notification, callback)
        {

        }

        public NotificationMessageAction(object sender, string notification, Action<T> callback)
            : base(sender, notification, callback)
        {

        }

        public NotificationMessageAction(object sender, object target, string notification, Action<T> callback)
            : base(sender, target, notification, callback)
        {

        }

        public void Execute(T parameter)
        {
            base.Execute(new object[] { parameter });
        }
    }
}
