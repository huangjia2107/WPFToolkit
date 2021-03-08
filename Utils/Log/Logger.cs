/*
using System;

using log4net;
using log4net.Config;

namespace CASApp.Util.Log
{
    /// <summary>
    /// 日志记录器
    /// </summary>
    public class Logger
    {
        //获取配置
        private static readonly ILog _common_logger = LogManager.GetLogger("common_logger");
        private static readonly ILog _cas_Logger = LogManager.GetLogger("cas_logger");
        private static readonly ILog _pg_Logger = LogManager.GetLogger("pg_logger");
        private static readonly ILog _systemManage_Logger = LogManager.GetLogger("systemManage_Logger");
        private static readonly ILog _webapi_Logger = LogManager.GetLogger("webapi_logger");

        //单例
        public static readonly Logger Instance = new Logger();

        private Logger()
        {
            //设置配置
            XmlConfigurator.Configure(new Uri(AppDomain.CurrentDomain.BaseDirectory + "log4net.config"));
        }

        /// <summary>
        /// 通用的 Log 记录器
        /// </summary>
        public ILog Common => _common_logger;

        /// <summary>
        /// CAS Log 记录器
        /// </summary>
        public ILog CAS => _cas_Logger;

        /// <summary>
        /// PG Log 记录器
        /// </summary>
        public ILog PG => _pg_Logger;

        public ILog SystemManage => _systemManage_Logger;

        /// <summary>
        /// WebApi Log 记录器
        /// </summary>
        public ILog WebApi => _webapi_Logger;
    }
}
*/