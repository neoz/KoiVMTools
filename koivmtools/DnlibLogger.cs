using dnlib.DotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tool.Logging;

namespace koivmtools
{
    internal class DnlibLogger : dnlib.DotNet.ILogger
    {
        private static readonly DnlibLogger _instance = new DnlibLogger();

        private DnlibLogger()
        {
        }

        public static DnlibLogger Instance => _instance;

        public bool IgnoresEvent(LoggerEvent loggerEvent)
        {
            return false;
        }

        public void Log(object sender, LoggerEvent loggerEvent, string format, params object[] args)
        {
            string text = $"{loggerEvent}: {string.Format(format, args)}";
            switch (loggerEvent)
            {
                case LoggerEvent.Error: Logger.Error(text); break;
                case LoggerEvent.Warning: Logger.Warning(text); break;
                case LoggerEvent.Info: Logger.Info(text); break;
                case LoggerEvent.Verbose:
                case LoggerEvent.VeryVerbose: Logger.Verbose3(text); break;
                default: throw new ArgumentOutOfRangeException(nameof(loggerEvent));
            }
        }
    }
}
