
namespace MvvmUtils.Messaging.Messages
{
    public class Message<T> : Message
    {
        public T Content { get; protected set; }

        public Message(T content)
        {
            Content = content;
        }

        public Message(object sender, T content)
            : base(sender)
        {
            Content = content;
        }

        public Message(object sender, object target, T content)
            : base(sender, target)
        {
            Content = content;
        }
    }
}
