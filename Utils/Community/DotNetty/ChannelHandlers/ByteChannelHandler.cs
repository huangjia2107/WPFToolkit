using DotNetty.Buffers;
using DotNetty.Transport.Channels;

namespace CASApp.Service.Community.DotNetty.ChannelHandlers
{
    public class ByteChannelHandler : BaseChannelHandler
    {
        private byte[] _messageWhileActive = null;

        public ByteChannelHandler(byte[] messageWhileActive = null)
        {
            _messageWhileActive = messageWhileActive;
        }

        /// <summary>
        /// 连接上服务器后，发消息到服务端
        /// </summary>
        /// <param name="context"></param>
        public override void ChannelActive(IChannelHandlerContext context)
        {
            if (_messageWhileActive != null && _messageWhileActive.Length > 0)
            {
                context.WriteAndFlushAsync(Unpooled.Buffer(_messageWhileActive.Length).WriteBytes(_messageWhileActive));
            }

            base.ChannelActive(context);
        }

        /// <summary>
        /// 读取数据
        /// </summary>
        /// <param name="context"></param>
        /// <param name="message"></param>
        public override void ChannelRead(IChannelHandlerContext context, object message)
        {
            if (message is IByteBuffer buffer && buffer.HasArray)
            {
                int length = buffer.ReadableBytes;

                byte[] array = new byte[length];
                buffer.GetBytes(buffer.ReaderIndex, array);

                base.InvokeStateChanged(this, new ChannelHandlerArgs { State = ChannelHandlerState.Read, Data = array });
            }
        }
    }
}
