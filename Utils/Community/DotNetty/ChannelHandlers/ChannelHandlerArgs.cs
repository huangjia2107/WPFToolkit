
namespace CASApp.Service.Community.DotNetty.ChannelHandlers
{
    public enum ChannelHandlerState
    {
        Active,
        Inactive,
        Read,
        ReadComplete,
        Disconnect,
        Exception,
        Close,
        Removed,
        Timeout
    }

    public class ChannelHandlerArgs
    {
        public ChannelHandlerState State { get; set; }
        public object Data { get; set; }
    }
}
