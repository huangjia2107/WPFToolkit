using System;
using System.Timers;

using DotNetty.Common.Concurrency;
using DotNetty.Transport.Channels;

namespace CASApp.Service.Community.DotNetty.ChannelHandlers
{
    /*
    public class BaseChannelHandler : ChannelHandlerAdapter
    {
        private readonly int _timeout;

        private Timer _timer = null;
        private DateTime? _writeDateTime = null;

        public event EventHandler<ChannelHandlerArgs> StateChanged;

        public BaseChannelHandler(int timeout = 10)
        {
            _timeout = timeout;

            _timer = new Timer(1000);
            _timer.Elapsed += OnTimerElapsed;
        }

        private void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            if (_writeDateTime.HasValue && DateTime.Now - _writeDateTime.Value > TimeSpan.FromSeconds(_timeout))
            {
                _timer.Stop();
                _writeDateTime = null;

                InvokeStateChanged(this, new ChannelHandlerArgs { State = ChannelHandlerState.Timeout });
            }
        }

        public override void ChannelActive(IChannelHandlerContext context)
        {
            base.ChannelActive(context);
            InvokeStateChanged(this, new ChannelHandlerArgs { State = ChannelHandlerState.Active });
        }

        public override void ChannelInactive(IChannelHandlerContext context)
        {
            StopTimer();

            base.ChannelInactive(context);
            InvokeStateChanged(this, new ChannelHandlerArgs { State = ChannelHandlerState.Inactive });
        }

        public override void ChannelRead(IChannelHandlerContext context, object message)
        {
            StopTimer();

            base.ChannelRead(context, message);
            InvokeStateChanged(this, new ChannelHandlerArgs { State = ChannelHandlerState.Read, Data = message });
        }

        public override void ChannelReadComplete(IChannelHandlerContext context)
        {
            base.ChannelReadComplete(context);

            context.Flush();
            InvokeStateChanged(this, new ChannelHandlerArgs { State = ChannelHandlerState.ReadComplete });
        }

        public override void Disconnect(IChannelHandlerContext context, IPromise promise)
        {
            StopTimer();

            base.Disconnect(context, promise);
            InvokeStateChanged(this, new ChannelHandlerArgs { State = ChannelHandlerState.Disconnect });
        }

        public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
        {
            StopTimer();

            base.ExceptionCaught(context, exception);
            InvokeStateChanged(this, new ChannelHandlerArgs { State = ChannelHandlerState.Exception, Data = exception });
        }

        public override void Close(IChannelHandlerContext context, IPromise promise)
        {
            StopTimer();

            base.Close(context, promise);
            InvokeStateChanged(this, new ChannelHandlerArgs { State = ChannelHandlerState.Close });
        }

        public override void HandlerRemoved(IChannelHandlerContext context)
        {
            StopTimer();

            base.HandlerRemoved(context);
            InvokeStateChanged(this, new ChannelHandlerArgs { State = ChannelHandlerState.Removed });
        }

        public override void Write(IChannelHandlerContext context, object message, IPromise promise)
        {
            base.Write(context, message, promise);

            _timer.Start();
            _writeDateTime = DateTime.Now;
        }

        protected void InvokeStateChanged(object sender, ChannelHandlerArgs e)
        {
            StateChanged?.Invoke(sender, e);
        }

        private void StopTimer()
        {
            _timer.Stop();
            _writeDateTime = null;
        }

        protected void Dispose()
        {
            if (StateChanged != null)
            {
                foreach (var del in StateChanged.GetInvocationList())
                {
                    StateChanged -= del as EventHandler<ChannelHandlerArgs>;
                }
            }

            if (_timer != null)
            {
                _timer.Elapsed -= OnTimerElapsed;
                _timer.Stop();
                _timer.Dispose();
                _timer = null;
            }
        }
    }
    */
}
