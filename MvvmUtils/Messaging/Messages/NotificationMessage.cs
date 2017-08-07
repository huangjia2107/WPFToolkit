 
namespace MvvmUtils.Messaging.Messages
{
    public class NotificationMessage : Message
    {
        public string Notification { get; private set; }

        public NotificationMessage(string notification)
        {
            Notification = notification;
        }

        public NotificationMessage(object sender, string notification)
            : base(sender)
        {
            Notification = notification;
        }

        public NotificationMessage(object sender, object target, string notification)
            : base(sender, target)
        {
            Notification = notification;
        }
    }
}
