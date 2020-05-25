using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bossinfo.Caller.MqttLogParserTesting
{
    public class LogService
    {
        public static NLog.Logger logger;

        public LogService()
        {
            if (logger == null)
                logger = NLog.LogManager.GetCurrentClassLogger();
        }

        public void Info(string str)
        {
            logger.Info(str);
        }

        public void Info(string str, object obj)
        {
            var detail = JsonConvert.SerializeObject(obj);
            logger.Info($"{str}\n{detail}");
        }

        public void Trace(string message)
        {
            Trace(message, null);
        }

        public void Trace(string message, object obj)
        {
            string msg = string.Empty;

            if (obj != null)
            {
                msg = $"{message} 資料：{JsonConvert.SerializeObject(obj)}";
            }
            else
            {
                msg = $"{message}";
            }

            msg += System.Environment.NewLine;
            msg += "--------------------------------------------";
            msg += System.Environment.NewLine;

            logger.Trace(msg);

            System.Diagnostics.Debug.WriteLine(msg);
        }

        public void Error(string message)
        {
            logger.Error(message);
        }

        public void Error(string message, object obj)
        {
            string msg = string.Empty;

            if (obj != null)
            {
                msg = $"{message} 資料：{JsonConvert.SerializeObject(obj)}";
            }
            else
            {
                msg = $"{message}";
            }

            logger.Error(msg);

            System.Diagnostics.Debug.WriteLine($"{DateTime.Now}:{msg}");
        }

        public void Error(Exception ex, string message)
        {
            logger.Error(ex, message);
        }
    }
}
