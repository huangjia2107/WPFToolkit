 
namespace MvvmUtils.Messaging.Messages
{
    public class Message
    {
        public object Sender { get; protected set; }
        public object Target { get; protected set; }

        public Message() { }

        public Message(object sender)
        {
            Sender = sender;
        }

        public Message(object sender, object target)
            : this(sender)
        {
            Target = target;
        }
    }
}
