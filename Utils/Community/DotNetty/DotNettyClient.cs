using System;
using System.Net;
using System.Threading.Tasks;

using DotNetty.Buffers;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;

using DotNetty.Codecs;
using System.Linq;

namespace Utils.Community.DotNetty
{
    public class DotNettyClient
    {
        private MultithreadEventLoopGroup _group = null;
        private IChannel _channel = null;

        public DotNettyClient()
        {
        }

        public bool IsConnected => _channel != null && _channel.Active;

        public async Task<bool> ConnectAsync(string ip, int port, ChannelHandlerAdapter channelHandler, params ByteToMessageDecoder[] decoders)
        {
            if (channelHandler == null)
                throw new ArgumentNullException(nameof(channelHandler));

            if (!IPAddress.TryParse(ip, out IPAddress ipAddress))
                ipAddress = null;

            if (ipAddress == null)
            {
                //Logger.Instance.Common.Info($"[ Netty ] ConnectAsync, Invalid IP Format, IP {ip}");
                return false;
            }

            try
            {
                _group = new MultithreadEventLoopGroup();

                
				var bootstrap = new Bootstrap();
				bootstrap.Group(_group)
						  .Channel<TcpSocketChannel>()
						  .Option(ChannelOption.TcpNodelay, false)
						  .Option(ChannelOption.SoKeepalive, true)
						  .Option(ChannelOption.ConnectTimeout, TimeSpan.FromSeconds(10))
						  .Handler(new ActionChannelInitializer<ISocketChannel>(c =>
						  {
							  var pipeline = c.Pipeline;

							  if (decoders != null && decoders.Length > 0)
								  pipeline.AddLast(decoders);

							  pipeline.AddLast(channelHandler);
						  }));

                _channel = await _bootstrap.ConnectAsync(new IPEndPoint(ipAddress, port));

                if (_channel == null)
                {
                    //Logger.Instance.Common.Info($"[ Netty ] ConnectAsync, IChannel Null, Address {ipAddress}:{port}");
                    return false;
                }

                if (!_channel.Active)
                {
                    await _channel.CloseAsync();
                    await _group.ShutdownGracefullyAsync();

                    //Logger.Instance.Common.Info($"[ Netty ] ConnectAsync, Connection Inactive, Address {ipAddress}:{port}");
                    return false;
                }

                //Logger.Instance.Common.Info($"[ Netty ] ConnectAsync, Connection Succeeded, Address {ipAddress}:{port}");
                return true;
            }
            catch (Exception ex)
            {
                if (_channel != null)
                {
                    await _channel.CloseAsync();
                    _channel = null;
                }

                if (_group != null)
                {
                    await _group?.ShutdownGracefullyAsync();
                    _group = null;
                }

                //Logger.Instance.Common.Info($"[ Netty ] ConnectAsync, Connection Failed, Address {ipAddress}:{port}\nError:{ex.Message}");
                return false;
            }
        }

        public async Task<bool> SendAsync(byte[] message)
        {
            if (message == null)
                return false;

            if (_channel == null || !_channel.Active)
            {
                //Logger.Instance.Common.Info($"[ Netty ] SendAsync, Connection Inactive");
                return false;
            }

            await _channel.WriteAndFlushAsync(Unpooled.CopiedBuffer(message));
            return true;
        }

        public async Task DisconnectAsync()
        {
            if (_channel != null)
            {
				/*
                if (_channel.Open || _channel.Active)
                    await _channel.DisconnectAsync();
                */
				
                await _channel.CloseAsync();
                _channel = null;
            }

            if (_group != null)
			{
                await _group.ShutdownGracefullyAsync();
				_group = null;
			}
        }
    }
}
