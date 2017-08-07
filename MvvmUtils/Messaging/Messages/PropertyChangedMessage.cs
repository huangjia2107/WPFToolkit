 
namespace MvvmUtils.Messaging.Messages
{
    public abstract class PropertyChangedMessage : Message
    {
        public string PropertyName { get; protected set; }

        protected PropertyChangedMessage(string propertyName)
        {
            PropertyName = propertyName;
        }

        protected PropertyChangedMessage(object sender, string propertyName)
            : base(sender)
        {
            PropertyName = propertyName;
        }

        protected PropertyChangedMessage(object sender, object target, string propertyName)
            : base(sender, target)
        {
            PropertyName = propertyName;
        }
    }
}
