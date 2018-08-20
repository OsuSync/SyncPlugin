using Sync.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BanManagerPlugin
{
    public static class Log
    {
        public static bool IsDebug { get; set; }

        private static readonly Logger logger = new Logger(BanManagerPlugin.PLUGIN_NAME);

        public static void Output(string message) => logger.LogInfomation(message);

        public static void Error(string message) => logger.LogError(message);

        public static void Warn(string message) => logger.LogWarning(message);

        public static void Debug(string message)
        {
            if (IsDebug)
                logger.LogInfomation("DEBUG:" + message);
        }
    }
}
