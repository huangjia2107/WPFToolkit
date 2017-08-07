 
namespace MvvmUtils.Messaging.Messages
{
    public class NotificationMessage<T> : Message<T>
    {
        public string Notification { get; private set; }

        public NotificationMessage(T content, string notification)
            : base(content)
        {
            Notification = notification;
        }

        public NotificationMessage(object sender, T content, string notification)
            : base(sender, content)
        {
            Notification = notification;
        }

        public NotificationMessage(object sender, object target, T content, string notification)
            : base(sender, target, content)
        {
            Notification = notification;
        }
    }
}
